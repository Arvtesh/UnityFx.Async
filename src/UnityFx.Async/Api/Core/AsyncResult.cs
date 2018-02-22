// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A lightweight <c>net35</c>-compatible analog of <c>Task</c> for Unity3d.
	/// </summary>
	/// <remarks>
	/// <para>This class is the core entity of the library. In many aspects it mimics <c>Task</c> interface and behaviour.
	/// For example, any <see cref="AsyncResult"/> instance can have any number of continuations (added either explicitly
	/// via <see cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/> call or implicitly via
	/// <c>async</c>/<c>await</c> usage). These continuations can be invoked on a captured <see cref="SynchronizationContext"/>
	/// (if any). The class inherits <see cref="IAsyncResult"/> (just like <c>Task</c>) and can be used for implementation of
	/// <see href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</see>.
	/// There is a number of operation state accessors that can be used exactly like matching properties of <c>Task</c>.
	/// </para>
	/// <para>Design goals for <see cref="AsyncResult"/> are:
	/// </para>
	/// <list type="bullet">
	/// <item>
	///   <term>Minimum size.</term>
	///   <description>That means just storing <see cref="IAsyncOperation"/> properties.</description>
	/// </item>
	/// <item>
	///   <term>Multithreading support.</term>
	///   <description>All class methods exception <see cref="Dispose()"/> are thread-safe.</description>
	/// </item>
	/// <item>
	///   <term><c>Task</c>-like interface and behaviour.</term>
	///   <description>This includes <c>async</c>/<c>await</c> (net46+ only), continuations and <see cref="SynchronizationContext"/> switching support.</description>
	/// </item>
	/// <item>
	///   <term>Unity3d compatibility.</term>
	///   <description>This includes possibility to <c>yield</c> any <see cref="AsyncResult"/> in coroutines and net35-compilance.</description>
	/// </item>
	/// </list>
	/// <para>The class implements <see cref="IDisposable"/> interface. So strictly speaking <see cref="Dispose()"/> should be called when the operation
	/// is no longed in use. In practice that is only required if <see cref="AsyncWaitHandle"/> property was used. Also keep in mind that <see cref="Dispose()"/>
	/// implementation is not thread-safe.
	/// </para>
	/// <para>Please note that while the class is designed as a lightweight and portable Task-like object, it's NOT a replacement for .NET <c>Task</c>.
	/// It is recommended to use <c>Task</c> when possible and only switch to <see cref="AsyncResult"/> only if one of the following applies:
	/// </para>
	/// <list type="bullet">
	///   <item><c>net35</c> compatibility is required.</item>
	///   <item>The operation should be used in Unity3d coroutines.</item>
	///   <item>Memory usage is your concern.</item>
	///   <item>You are implementing <see href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</see> and need <see cref="IAsyncResult"/> implementation.</item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <see cref="AsyncResult"/> class can be used very similarly to <c>Task</c>:
	/// <code>
	/// async Task Foo()
	/// {
	///     await AsyncResult.Delay(10);
	/// }
	/// </code>
	/// Or (in Unity3d) it can be used in coroutines:
	/// <code>
	/// IEnumerator FooEnum()
	/// {
	///     yield return AsyncResult.Delay(10);
	/// }
	///
	/// Coroutine Foo()
	/// {
	///     return StartCoroutine(FooEnum());
	/// }
	/// </code>
	/// <see cref="AsyncCompletionSource"/> can be used much like <c>TaskCompletionSource</c>:
	/// <code>
	/// IAsyncOperation&lt;int&gt; Foo()
	/// {
	///     var op = new AsyncCompletionSource&lt;int&gt;();
	///     op.SetResult(25);
	///     return op; // Can use op.Operation here as well
	/// }
	/// </code>
	/// </example>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Task</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming">Task-based Asynchronous Pattern (TAP)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/">How to implement the IAsyncResult design pattern</seealso>
	/// <seealso cref="AsyncCompletionSource"/>
	/// <seealso cref="AsyncResult{T}"/>
	/// <seealso cref="IAsyncResult"/>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AsyncResult : IAsyncOperation, IEnumerator
	{
		#region data

		private const int _flagCompletionReserved = 0x00100000;
		private const int _flagCompleted = 0x00200000;
		private const int _flagSynchronous = 0x00400000;
		private const int _flagCompletedSynchronously = _flagCompleted | _flagCompletionReserved | _flagSynchronous;
		private const int _flagDisposed = 0x01000000;
		private const int _flagDoNotDispose = 0x10000000;
		private const int _statusMask = 0x0000000f;

		private static AsyncResult _completedOperation;
		private static object _continuationCompletionSentinel = new object();

		private readonly object _asyncState;

		private EventWaitHandle _waitHandle;
		private AggregateException _exception;

		private volatile object _continuation;
		private volatile int _flags;

		#endregion

		#region interface

		/// <summary>
		/// Raised when the operation has completed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered in <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		public event EventHandler Completed
		{
			add
			{
				ThrowIfDisposed();

				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (!TryAddContinuation(value, SynchronizationContext.Current))
				{
					value(this, EventArgs.Empty);
				}
			}
			remove
			{
				ThrowIfDisposed();

				if (value != null)
				{
					TryRemoveContinuation(value);
				}
			}
		}

		/// <summary>
		/// Gets whether the operation instance is disposed.
		/// </summary>
		/// <value>The disposed flag.</value>
		protected bool IsDisposed => (_flags & _flagDisposed) != 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		public AsyncResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncCallback asyncCallback, object asyncState)
		{
			_asyncState = asyncState;
			_continuation = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		public AsyncResult(AsyncOperationStatus status)
		{
			var flags = (int)status;

			if (flags == StatusFaulted)
			{
				_exception = new AggregateException();
			}

			if (flags > StatusRunning)
			{
				flags |= _flagCompletedSynchronously;
			}

			_flags = flags;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: this(status)
		{
			_asyncState = asyncState;
			_continuation = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class that is faulted.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="e"/> is <see langword="null"/>.</exception>
		public AsyncResult(Exception e)
		{
			_exception = new AggregateException(e) ?? throw new ArgumentNullException(nameof(e));
			_flags = StatusFaulted | _flagCompletedSynchronously;
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition has failed.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TryStart"/>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="OnStarted"/>
		public void Start()
		{
			if (!TrySetRunning())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transitions the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Start"/>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="OnStarted"/>
		public bool TryStart()
		{
			return TrySetRunning();
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetRunning"/>
		protected internal bool TrySetScheduled()
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusScheduled))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetScheduled"/>
		protected internal bool TrySetRunning()
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusRunning))
			{
				OnStarted();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetCanceled(bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TrySetCompleted(StatusCanceled, completedSynchronously))
			{
				OnStatusChanged(AsyncOperationStatus.Canceled);
				OnCompleted();
				return true;
			}
			else if (!IsCompleted)
			{
				AsyncExtensions.SpinUntilCompleted(this);
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> (or <see cref="AsyncOperationStatus.Canceled"/> if the exception is <see cref="OperationCanceledException"/>) state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetException(Exception exception, bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			if (TryReserveCompletion())
			{
				if (exception is OperationCanceledException)
				{
					SetCompleted(StatusCanceled, completedSynchronously);
				}
				else
				{
					_exception = new AggregateException(exception);
					SetCompleted(StatusFaulted, completedSynchronously);
				}

				return true;
			}
			else if (!IsCompleted)
			{
				AsyncExtensions.SpinUntilCompleted(this);
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="exceptions"/> is empty.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetExceptions(IEnumerable<Exception> exceptions, bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (exceptions == null)
			{
				throw new ArgumentNullException(nameof(exceptions));
			}

			var list = new List<Exception>();

			foreach (var e in exceptions)
			{
				if (e == null)
				{
					throw new ArgumentException("Null exceptions are not allowed.", nameof(exceptions));
				}

				list.Add(e);
			}

			if (list.Count == 0)
			{
				throw new ArgumentException("At least one exception is needed.", nameof(exceptions));
			}

			if (TryReserveCompletion())
			{
				_exception = new AggregateException(list);
				SetCompleted(StatusFaulted, completedSynchronously);
				return true;
			}
			else if (!IsCompleted)
			{
				AsyncExtensions.SpinUntilCompleted(this);
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetCompleted(bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TrySetCompleted(StatusRanToCompletion, completedSynchronously))
			{
				OnStatusChanged(AsyncOperationStatus.RanToCompletion);
				OnCompleted();
				return true;
			}
			else if (!IsCompleted)
			{
				AsyncExtensions.SpinUntilCompleted(this);
			}

			return false;
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> if this operation has been disposed.
		/// </summary>
		protected internal void ThrowIfDisposed()
		{
			if ((_flags & _flagDisposed) != 0)
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		#endregion

		#region virtual interface

		/// <summary>
		/// Called when the operation state has changed.
		/// </summary>
		/// <param name="status">The new status value.</param>
		/// <seealso cref="Status"/>
		/// <seealso cref="TrySetScheduled"/>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCompleted(bool)"/>
		/// <seealso cref="TrySetException(System.Exception, bool)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{System.Exception}, bool)"/>
		protected virtual void OnStatusChanged(AsyncOperationStatus status)
		{
		}

		/// <summary>
		/// Called when the operation is started (status is set to <see cref="AsyncOperationStatus.Running"/>).
		/// </summary>
		/// <seealso cref="Status"/>
		/// <seealso cref="Start"/>
		/// <seealso cref="TryStart"/>
		/// <seealso cref="TrySetRunning"/>
		protected virtual void OnStarted()
		{
		}

		/// <summary>
		/// Called when the operation is completed. Default implementation invokes completion handlers registered.
		/// </summary>
		/// <seealso cref="Status"/>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCompleted(bool)"/>
		/// <seealso cref="TrySetException(System.Exception, bool)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{System.Exception}, bool)"/>
		protected virtual void OnCompleted()
		{
			_waitHandle?.Set();
			InvokeContinuations();
		}

		/// <summary>
		/// Releases unmanaged resources used by the object.
		/// </summary>
		/// <remarks>
		/// Unlike most of the members of <see cref="AsyncResult"/>, this method is not thread-safe.
		/// </remarks>
		/// <param name="disposing">A <see langword="bool"/> value that indicates whether this method is being called due to a call to <see cref="Dispose()"/>.</param>
		/// <seealso cref="Dispose()"/>
		/// <seealso cref="ThrowIfDisposed"/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && (_flags & _flagDoNotDispose) == 0)
			{
				_flags |= _flagDisposed;

				if (_waitHandle != null)
				{
#if NET35
					_waitHandle.Close();
#else
					_waitHandle.Dispose();
#endif
					_waitHandle = null;
				}
			}
		}

		#endregion

		#region static interface

		/// <summary>
		/// Gets an operation that's already been completed successfully.
		/// </summary>
		/// <remarks>
		/// Note that <see cref="Dispose()"/> call have no effect on operations returned with the property. May not always return the same instance.
		/// </remarks>
		/// <value>Completed <see cref="IAsyncOperation"/> instance.</value>
		public static AsyncResult CompletedOperation
		{
			get
			{
				if (_completedOperation == null)
				{
					_completedOperation = new AsyncResult(_flagDoNotDispose | _flagCompletedSynchronously | StatusRanToCompletion);
				}

				return _completedOperation;
			}
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that is canceled.
		/// </summary>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled{T}"/>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromCanceled()
		{
			return new AsyncResult(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> that is canceled.
		/// </summary>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromCanceled<T>()
		{
			return new AsyncResult<T>(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromCanceled"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromException(Exception e)
		{
			return new AsyncResult(e);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromCanceled{T}"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromException<T>(Exception e)
		{
			return new AsyncResult<T>(e);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> that has completed with a specified result.
		/// </summary>
		/// <param name="result">The result value with which to complete the operation.</param>
		/// <returns>A completed operation with the specified result value.</returns>
		/// <seealso cref="FromCanceled{T}"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		public static AsyncResult<T> FromResult<T>(T result)
		{
			return new AsyncResult<T>(result);
		}

		/// <summary>
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="millisecondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(TimeSpan)"/>
		public static AsyncResult Delay(int millisecondsDelay)
		{
			if (millisecondsDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
			}

			if (millisecondsDelay == 0)
			{
				return CompletedOperation;
			}

			if (millisecondsDelay == Timeout.Infinite)
			{
				return new AsyncResult();
			}

			return new DelayResult(millisecondsDelay);
		}

		/// <summary>
		/// Creates a task that completes after a specified time interval.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned task, or <c>TimeSpan.FromMilliseconds(-1)</c> to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="delay"/> represents a negative time interval other than <c>TimeSpan.FromMillseconds(-1)</c>.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(int)"/>
		public static AsyncResult Delay(TimeSpan delay)
		{
			var millisecondsDelay = (long)delay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			return Delay((int)millisecondsDelay);
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <seealso cref="WhenAll(IAsyncOperation[])"/>
		public static AsyncResult WhenAll(IEnumerable<IAsyncOperation> ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops is IAsyncOperation[] opArray)
			{
				if (opArray.Length == 0)
				{
					return CompletedOperation;
				}

				return new WhenAllResult(opArray);
			}

			if (ops is ICollection<IAsyncOperation> opCollection)
			{
				if (opCollection.Count == 0)
				{
					return CompletedOperation;
				}

				var array = new IAsyncOperation[opCollection.Count];
				opCollection.CopyTo(array, 0);
				return new WhenAllResult(array);
			}

			var opList = new List<IAsyncOperation>(ops);
			return new WhenAllResult(opList.ToArray());
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <seealso cref="WhenAll(IEnumerable{IAsyncOperation})"/>
		public static AsyncResult WhenAll(params IAsyncOperation[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				return CompletedOperation;
			}

			return new WhenAllResult(ops);
		}

		/// <summary>
		/// Creates an operation that will complete when any of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of any of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <seealso cref="WhenAny{T}(T[])"/>
		public static AsyncResult<T> WhenAny<T>(IEnumerable<T> ops) where T : IAsyncOperation
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops is T[] opArray)
			{
				if (opArray.Length == 0)
				{
					throw new ArgumentException("The list is empty", nameof(ops));
				}

				return new WhenAnyResult<T>(opArray);
			}

			if (ops is ICollection<T> opCollection)
			{
				if (opCollection.Count == 0)
				{
					throw new ArgumentException("The list is empty", nameof(ops));
				}

				var array = new T[opCollection.Count];
				opCollection.CopyTo(array, 0);
				return new WhenAnyResult<T>(array);
			}

			var opList = new List<T>(ops);

			if (opList.Count == 0)
			{
				throw new ArgumentException("The list is empty", nameof(ops));
			}

			return new WhenAnyResult<T>(opList.ToArray());
		}

		/// <summary>
		/// Creates an operation that will complete when any of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of any of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <seealso cref="WhenAny{T}(IEnumerable{T})"/>
		public static AsyncResult<T> WhenAny<T>(params T[] ops) where T : IAsyncOperation
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				throw new ArgumentException("The list is empty", nameof(ops));
			}

			return new WhenAnyResult<T>(ops);
		}

		/// <summary>
		/// Initializes the <paramref name="waitHandle"/> passed with a new <see cref="EventWaitHandle"/> instance if needed.
		/// </summary>
		/// <param name="waitHandle">The wait handle reference to initialize.</param>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> instance that owns the wait handle.</param>
		/// <returns>Returns the resulting <paramref name="waitHandle"/> value.</returns>
		public static EventWaitHandle TryCreateAsyncWaitHandle(ref EventWaitHandle waitHandle, IAsyncResult asyncResult)
		{
			if (waitHandle == null)
			{
				var done = asyncResult.IsCompleted;
				var mre = new ManualResetEvent(done);

				if (Interlocked.CompareExchange(ref waitHandle, mre, null) != null)
				{
					// Another thread created this object's event; dispose the event we just created.
#if NET35
					mre.Close();
#else
					mre.Dispose();
#endif
				}
				else if (!done && asyncResult.IsCompleted)
				{
					// We published the event as unset, but the operation has subsequently completed;
					// set the event state properly so that callers do not deadlock.
					waitHandle.Set();
				}
			}

			return waitHandle;
		}

		#endregion

		#region internals

		internal const int StatusCreated = 0;
		internal const int StatusScheduled = 1;
		internal const int StatusRunning = 2;
		internal const int StatusRanToCompletion = 3;
		internal const int StatusCanceled = 4;
		internal const int StatusFaulted = 5;

		/// <summary>
		/// Special status setter for <see cref="AsyncOperationStatus.Scheduled"/> and <see cref="AsyncOperationStatus.Running"/>.
		/// </summary>
		internal bool TrySetStatus(int newStatus)
		{
			Debug.Assert(newStatus < StatusRanToCompletion);

			do
			{
				var flags = _flags;

				if ((flags & (_flagCompleted | _flagCompletionReserved)) != 0)
				{
					return false;
				}

				var status = flags & _statusMask;

				if (status >= newStatus)
				{
					return false;
				}

				var newFlags = (flags & ~_statusMask) | newStatus;

				if (Interlocked.CompareExchange(ref _flags, newFlags, flags) == flags)
				{
					OnStatusChanged((AsyncOperationStatus)newStatus);
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Sets the operation status to one of <see cref="AsyncOperationStatus.RanToCompletion"/>/<see cref="AsyncOperationStatus.Canceled"/>/<see cref="AsyncOperationStatus.Faulted"/>.
		/// The call does the same as calling <see cref="TryReserveCompletion"/> and <see cref="SetCompleted(int, bool)"/> but uses one interlocked operation instead of two.
		/// </summary>
		internal bool TrySetCompleted(int status, bool completedSynchronously)
		{
			Debug.Assert(status > StatusRunning);

			status |= _flagCompleted | _flagCompletionReserved;

			if (completedSynchronously)
			{
				status |= _flagSynchronous;
			}

			do
			{
				var flags = _flags;

				if ((flags & (_flagCompletionReserved | _flagCompleted)) != 0)
				{
					return false;
				}

				var newFlags = (flags & ~_statusMask) | status;

				if (Interlocked.CompareExchange(ref _flags, newFlags, flags) == flags)
				{
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Initiates operation completion. Should only be used in pair with <see cref="SetCompleted(int, bool)"/>.
		/// </summary>
		internal bool TryReserveCompletion()
		{
			do
			{
				var flags = _flags;

				if ((flags & (_flagCompletionReserved | _flagCompleted)) != 0)
				{
					return false;
				}

				if (Interlocked.CompareExchange(ref _flags, flags | _flagCompletionReserved, flags) == flags)
				{
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Unconditionally sets the operation status to one of <see cref="AsyncOperationStatus.RanToCompletion"/>/<see cref="AsyncOperationStatus.Canceled"/>/<see cref="AsyncOperationStatus.Faulted"/>.
		/// Should only be called if <see cref="TryReserveCompletion"/> call succeeded.
		/// </summary>
		internal void SetCompleted(int status, bool completedSynchronously)
		{
			Debug.Assert(status > StatusRunning);
			Debug.Assert((_flags & _flagCompletionReserved) != 0);
			Debug.Assert((_flags & _statusMask) < StatusRanToCompletion);

			var oldFlags = _flags & ~_statusMask;
			var newFlags = status | _flagCompleted;

			if (completedSynchronously)
			{
				newFlags |= _flagSynchronous;
			}

			// Set completed status. After this call IsCompleted will return true.
			Interlocked.Exchange(ref _flags, oldFlags | newFlags);

			// Invoke completion callbacks.
			OnStatusChanged((AsyncOperationStatus)status);
			OnCompleted();
		}

		/// <summary>
		/// Special continuation for the awaiter.
		/// </summary>
		internal void SetContinuationForAwait(Action action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (!TryAddContinuation(action, syncContext))
			{
				action();
			}
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public AsyncOperationStatus Status => (AsyncOperationStatus)(_flags & _statusMask);

		/// <inheritdoc/>
		public AggregateException Exception => (_flags & _statusMask) == StatusFaulted ? _exception : null;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => (_flags & _statusMask) == StatusRanToCompletion;

		/// <inheritdoc/>
		public bool IsFaulted => (_flags & _statusMask) == StatusFaulted;

		/// <inheritdoc/>
		public bool IsCanceled => (_flags & _statusMask) == StatusCanceled;

		#endregion

		#region IAsyncOperationEvents

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddContinuation(action, syncContext);
		}

		/// <inheritdoc/>
		public bool RemoveCompletionCallback(AsyncOperationCallback action)
		{
			ThrowIfDisposed();

			if (action != null)
			{
				return TryRemoveContinuation(action);
			}

			return false;
		}

		#endregion

		#region IAsyncResult

		/// <inheritdoc/>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				ThrowIfDisposed();
				return TryCreateAsyncWaitHandle(ref _waitHandle, this);
			}
		}

		/// <inheritdoc/>
		public object AsyncState => _asyncState;

		/// <inheritdoc/>
		public bool CompletedSynchronously => (_flags & _flagSynchronous) != 0;

		/// <inheritdoc/>
		public bool IsCompleted => (_flags & _flagCompleted) != 0;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		object IEnumerator.Current => null;

		/// <inheritdoc/>
		bool IEnumerator.MoveNext() => _flags == StatusRunning;

		/// <inheritdoc/>
		void IEnumerator.Reset() => throw new NotSupportedException();

		#endregion

		#region IDisposable

		/// <summary>
		/// Disposes the <see cref="AsyncResult"/>, releasing all of its unmanaged resources.
		/// </summary>
		/// <remarks>
		/// Unlike most of the members of <see cref="AsyncResult"/>, this method is not thread-safe.
		/// Also, <see cref="Dispose()"/> may only be called on a <see cref="AsyncResult"/> that is in one of
		/// the final states: <see cref="AsyncOperationStatus.RanToCompletion"/>, <see cref="AsyncOperationStatus.Faulted"/> or
		/// <see cref="AsyncOperationStatus.Canceled"/>.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if the operation is not completed.</exception>
		/// <seealso cref="Dispose(bool)"/>
		public void Dispose()
		{
			if (!IsCompleted)
			{
				throw new InvalidOperationException("Cannot dispose non-completed operation.");
			}

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Object

		/// <inheritdoc/>
		public override string ToString()
		{
			return GetType().Name;
		}

		#endregion

		#region implementation

		private string DebuggerDisplay
		{
			get
			{
				var result = ToString();
				var state = Status.ToString();

				if (IsFaulted && _exception != null)
				{
					if (_exception.InnerException != null)
					{
						state += " (" + _exception.InnerException.GetType().Name + ')';
					}
					else
					{
						state += " (" + _exception.GetType().Name + ')';
					}
				}

				result += ", Status = ";
				result += state;

				if (IsDisposed)
				{
					result += ", Disposed";
				}

				return result;
			}
		}

		private AsyncResult(int flags)
		{
			_flags = flags;
		}

		private bool TryAddContinuation(object continuation, SynchronizationContext syncContext)
		{
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				continuation = new AsyncContinuation(this, syncContext, continuation);
			}

			return TryAddContinuation(continuation);
		}

		private bool TryAddContinuation(object valueToAdd)
		{
			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var oldValue = _continuation;

			// Quick return if the operation is completed.
			if (oldValue != _continuationCompletionSentinel)
			{
				// If no continuation is stored yet, try to store it as _continuation.
				if (oldValue == null)
				{
					oldValue = Interlocked.CompareExchange(ref _continuation, valueToAdd, null);

					// Quick return if exchange succeeded.
					if (oldValue == null)
					{
						return true;
					}
				}

				// Logic for the case where we were previously storing a single continuation.
				if (oldValue != _continuationCompletionSentinel && !(oldValue is IList))
				{
					var newList = new List<object>() { oldValue };

					Interlocked.CompareExchange(ref _continuation, newList, oldValue);

					// We might be racing against another thread converting the single into a list,
					// or we might be racing against operation completion, so resample "list" below.
				}

				// If list is null, it can only mean that _continuationCompletionSentinel has been exchanged
				// into _continuation. Thus, the task has completed and we should return false from this method,
				// as we will not be queuing up the continuation.
				if (_continuation is IList list)
				{
					lock (list)
					{
						// It is possible for the operation to complete right after we snap the copy of the list.
						// If so, then fall through and return false without queuing the continuation.
						if (_continuation != _continuationCompletionSentinel)
						{
							list.Add(valueToAdd);
							return true;
						}
					}
				}
			}

			return false;
		}

		private bool TryRemoveContinuation(object valueToRemove)
		{
			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var value = _continuation;

			if (value != _continuationCompletionSentinel)
			{
				var list = value as IList;

				if (list == null)
				{
					// This is not a list. If we have a single object (the one we want to remove) we try to replace it with an empty list.
					// Note we cannot go back to a null state, since it will mess up the TryAddContinuation logic.
					if (Interlocked.CompareExchange(ref _continuation, new List<object>(), valueToRemove) == valueToRemove)
					{
						return true;
					}
					else
					{
						// If we fail it means that either TryAddContinuation won the race condition and _continuation is now a List
						// that contains the element we want to remove. Or it set the _continuationCompletionSentinel.
						// So we should try to get a list one more time.
						list = value as IList;
					}
				}

				// If list is null it means _continuationCompletionSentinel has been set already and there is nothing else to do.
				if (list != null)
				{
					lock (list)
					{
						// There is a small chance that the operation completed since we took a local snapshot into
						// list. In that case, just return; we don't want to be manipulating the continuation list as it is being processed.
						if (_continuation != _continuationCompletionSentinel)
						{
							var index = list.IndexOf(valueToRemove);

							if (index != -1)
							{
								list.RemoveAt(index);
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		private void InvokeContinuations()
		{
			var continuation = Interlocked.Exchange(ref _continuation, _continuationCompletionSentinel);

			if (continuation != null)
			{
				if (continuation is IEnumerable list)
				{
					lock (list)
					{
						foreach (var item in list)
						{
							InvokeContinuation(item);
						}
					}
				}
				else
				{
					InvokeContinuation(continuation);
				}
			}
		}

		private void InvokeContinuation(object continuation)
		{
			if (continuation is AsyncContinuation c)
			{
				c.Invoke();
			}
			else
			{
				AsyncContinuation.Run(this, continuation);
			}
		}

		#endregion
	}
}
