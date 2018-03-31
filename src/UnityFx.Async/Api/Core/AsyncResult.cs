// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A lightweight <c>net35</c>-compatible asynchronous operation for <c>Unity3d</c>.
	/// </summary>
	/// <remarks>
	/// <para>This class is the core entity of the library. In many aspects it mimics <c>Task</c>
	/// interface and behaviour. For example, any <see cref="AsyncResult"/> instance can have any
	/// number of continuations (added either explicitly via <c>TryAddCompletionCallback</c>
	/// call or implicitly using <c>async</c>/<c>await</c> keywords). These continuations can be
	/// invoked on a captured <see cref="SynchronizationContext"/>. The class inherits <see cref="IAsyncResult"/>
	/// (just like <c>Task</c>) and can be used to implement Asynchronous Programming Model (APM).
	/// There is a number of operation state accessors that can be used exactly like corresponding
	/// properties of <c>Task</c>.
	/// </para>
	/// <para>The class implements <see cref="IDisposable"/> interface. So strictly speaking <see cref="Dispose()"/>
	/// should be called when the operation is no longed in use. In practice that is only required
	/// if <see cref="AsyncWaitHandle"/> property was used. Also keep in mind that <see cref="Dispose()"/>
	/// implementation is not thread-safe.
	/// </para>
	/// <para>Please note that while the class is designed as a lightweight and portable <c>Task</c>-like object,
	/// it's NOT a replacement for .NET <c>Task</c>. It is recommended to use <c>Task</c> in general and only switch
	/// to this class if Unity/net35 compatibility is a concern.
	/// </para>
	/// </remarks>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/">How to implement the IAsyncResult design pattern</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming">Task-based Asynchronous Pattern (TAP)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Task</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1">TaskCompletionSource</seealso>
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
		private const int _resetMask = 0x70000000;

		private static readonly object _continuationCompletionSentinel = new object();
		private static AsyncResult _completedOperation;

		private readonly object _asyncState;

		private EventWaitHandle _waitHandle;
		private AggregateException _exception;

		private volatile object _continuation;
		private volatile int _flags;

		#endregion

		#region interface

		/// <summary>
		/// Gets a value indicating whether the operation instance is disposed.
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
			: this((int)status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, object asyncState)
			: this((int)status)
		{
			_asyncState = asyncState;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: this((int)status)
		{
			_asyncState = asyncState;
			_continuation = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class that is faulted. For internal use only.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		internal AsyncResult(Exception exception, object asyncState)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			if (exception is AggregateException ae)
			{
				_exception = ae;
			}
			else
			{
				_exception = new AggregateException(exception);
			}

			if (_exception.InnerException is OperationCanceledException)
			{
				_flags = StatusCanceled | _flagCompletedSynchronously;
			}
			else
			{
				_flags = StatusFaulted | _flagCompletedSynchronously;
			}

			_continuation = _continuationCompletionSentinel;
			_asyncState = asyncState;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class that is faulted. For internal use only.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is <see langword="null"/>.</exception>
		internal AsyncResult(IEnumerable<Exception> exceptions, object asyncState)
		{
			if (exceptions == null)
			{
				throw new ArgumentNullException(nameof(exceptions));
			}

			_exception = new AggregateException(exceptions);

			if (_exception.InnerException is OperationCanceledException)
			{
				_flags = StatusCanceled | _flagCompletedSynchronously;
			}
			else
			{
				_flags = StatusFaulted | _flagCompletedSynchronously;
			}

			_continuation = _continuationCompletionSentinel;
			_asyncState = asyncState;
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

			if (TryReserveCompletion())
			{
				_exception = new AggregateException(new OperationCanceledException());
				SetCompleted(StatusCanceled, completedSynchronously);
				return true;
			}
			else if (!IsCompleted)
			{
				AsyncExtensions.SpinUntilCompleted(this);
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> (or <see cref="AsyncOperationStatus.Canceled"/>
		/// if the exception is <see cref="OperationCanceledException"/>) state.
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
					_exception = new AggregateException(exception);
					SetCompleted(StatusCanceled, completedSynchronously);
				}
				else
				{
					if (exception is AggregateException ae)
					{
						_exception = ae;
					}
					else
					{
						_exception = new AggregateException(exception);
					}

					if (_exception.InnerException is OperationCanceledException)
					{
						SetCompleted(StatusCanceled, completedSynchronously);
					}
					else
					{
						SetCompleted(StatusFaulted, completedSynchronously);
					}
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
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(exceptions));
				}

				list.Add(e);
			}

			if (list.Count == 0)
			{
				throw new ArgumentException(Constants.ErrorListIsEmpty, nameof(exceptions));
			}

			if (TryReserveCompletion())
			{
				_exception = new AggregateException(list);

				if (_exception.InnerException is OperationCanceledException)
				{
					SetCompleted(StatusCanceled, completedSynchronously);
				}
				else
				{
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
				return true;
			}
			else if (!IsCompleted)
			{
				AsyncExtensions.SpinUntilCompleted(this);
			}

			return false;
		}

		/// <summary>
		/// Throws exception if the operation has failed or canceled.
		/// </summary>
		protected internal void ThrowIfNonSuccess(bool throwAggregate)
		{
			var status = _flags & _statusMask;

			if (throwAggregate)
			{
				if (status == StatusFaulted)
				{
					if (_exception != null)
					{
						throw _exception;
					}
					else
					{
						// Should never get here. Exception should never be null in faulted state.
						throw new AggregateException();
					}
				}
				else if (status == StatusCanceled)
				{
					if (_exception != null)
					{
						throw _exception;
					}
					else
					{
						throw new AggregateException(new OperationCanceledException());
					}
				}
			}
			else
			{
				if (status == StatusFaulted)
				{
					if (!TryThrowException(_exception))
					{
						// Should never get here. Exception should never be null in faulted state.
						throw new Exception();
					}
				}
				else if (status == StatusCanceled)
				{
					if (!TryThrowException(_exception))
					{
						throw new OperationCanceledException();
					}
				}
			}
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
		/// Called when the operation state has changed. Default implementation does nothing.
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
		/// Called when the operation is started (<see cref="Status"/> is set to <see cref="AsyncOperationStatus.Running"/>). Default implementation does nothing.
		/// </summary>
		/// <seealso cref="OnCompleted"/>
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
		/// <seealso cref="OnStarted"/>
		/// <seealso cref="Status"/>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCompleted(bool)"/>
		/// <seealso cref="TrySetException(System.Exception, bool)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{System.Exception}, bool)"/>
		protected virtual void OnCompleted()
		{
			try
			{
				var continuation = Interlocked.Exchange(ref _continuation, _continuationCompletionSentinel);

				if (continuation != null)
				{
					if (continuation is IEnumerable continuationList)
					{
						lock (continuationList)
						{
							foreach (var item in continuationList)
							{
								InvokeContinuation(this, item);
							}
						}
					}
					else
					{
						InvokeContinuation(this, continuation);
					}
				}
			}
			finally
			{
				_waitHandle?.Set();
			}
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

		#region From*

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that is canceled.
		/// </summary>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled(object)"/>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromExceptions(IEnumerable{System.Exception})"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromCanceled()
		{
			return new AsyncResult(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that is canceled.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled()"/>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromExceptions(IEnumerable{System.Exception})"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult FromCanceled(object asyncState)
		{
			return new AsyncResult(AsyncOperationStatus.Canceled, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that is canceled.
		/// </summary>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromExceptions{T}(IEnumerable{System.Exception})"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromCanceled<T>()
		{
			return new AsyncResult<T>(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that is canceled.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromExceptions{T}(IEnumerable{System.Exception})"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult<T> FromCanceled<T>(object asyncState)
		{
			return new AsyncResult<T>(AsyncOperationStatus.Canceled, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(System.Exception, object)"/>
		/// <seealso cref="FromExceptions(IEnumerable{System.Exception})"/>
		/// <seealso cref="FromCanceled()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromException(Exception exception)
		{
			return new AsyncResult(exception, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(System.Exception)"/>
		/// <seealso cref="FromExceptions(IEnumerable{System.Exception}, object)"/>
		/// <seealso cref="FromCanceled(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult FromException(Exception exception, object asyncState)
		{
			return new AsyncResult(exception, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with specified exceptions.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromExceptions(IEnumerable{Exception}, object)"/>
		/// <seealso cref="FromException(System.Exception)"/>
		/// <seealso cref="FromCanceled()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromExceptions(IEnumerable<Exception> exceptions)
		{
			return new AsyncResult(exceptions, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with specified exceptions.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromExceptions(IEnumerable{Exception})"/>
		/// <seealso cref="FromException(System.Exception, object)"/>
		/// <seealso cref="FromCanceled(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult FromExceptions(IEnumerable<Exception> exceptions, object asyncState)
		{
			return new AsyncResult(exceptions, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException{T}(System.Exception, object)"/>
		/// <seealso cref="FromExceptions{T}(IEnumerable{System.Exception})"/>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromException<T>(Exception exception)
		{
			return new AsyncResult<T>(exception, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException{T}(System.Exception)"/>
		/// <seealso cref="FromExceptions{T}(IEnumerable{System.Exception}, object)"/>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult<T> FromException<T>(Exception exception, object asyncState)
		{
			return new AsyncResult<T>(exception, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with specified exceptions.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromExceptions{T}(IEnumerable{Exception}, object)"/>
		/// <seealso cref="FromException{T}(System.Exception)"/>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromExceptions<T>(IEnumerable<Exception> exceptions)
		{
			return new AsyncResult<T>(exceptions, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with specified exceptions.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromExceptions{T}(IEnumerable{Exception})"/>
		/// <seealso cref="FromException{T}(System.Exception, object)"/>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult<T> FromExceptions<T>(IEnumerable<Exception> exceptions, object asyncState)
		{
			return new AsyncResult<T>(exceptions, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified result.
		/// </summary>
		/// <param name="result">The result value with which to complete the operation.</param>
		/// <returns>A completed operation with the specified result value.</returns>
		/// <seealso cref="FromResult{T}(T, object)"/>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromExceptions{T}(IEnumerable{System.Exception})"/>
		public static AsyncResult<T> FromResult<T>(T result)
		{
			return new AsyncResult<T>(result, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified result.
		/// </summary>
		/// <param name="result">The result value with which to complete the operation.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A completed operation with the specified result value.</returns>
		/// <seealso cref="FromResult{T}(T)"/>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromExceptions{T}(IEnumerable{System.Exception})"/>
		public static AsyncResult<T> FromResult<T>(T result, object asyncState)
		{
			return new AsyncResult<T>(result, asyncState);
		}

		#endregion

		#region Delay

		/// <summary>
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="millisecondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(TimeSpan)"/>
		public static AsyncResult Delay(int millisecondsDelay)
		{
			if (millisecondsDelay < Timeout.Infinite)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), millisecondsDelay, Constants.ErrorValueIsLessThanZero);
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
		/// Creates an operation that completes after a specified time interval.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned operation, or <c>TimeSpan.FromMilliseconds(-1)</c> to wait indefinitely.</param>
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

		#endregion

		#region Retry

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, TimeSpan, int)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, int millisecondsRetryDelay, int maxRetryCount = 0)
		{
			if (opFactory == null)
			{
				throw new ArgumentNullException(nameof(opFactory));
			}

			if (millisecondsRetryDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsRetryDelay), millisecondsRetryDelay, Constants.ErrorValueIsLessThanZero);
			}

			if (maxRetryCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, Constants.ErrorValueIsLessThanZero);
			}

			return new RetryResult<object>(opFactory, millisecondsRetryDelay, maxRetryCount);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, int, int)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, TimeSpan retryDelay, int maxRetryCount = 0)
		{
			var millisecondsDelay = (long)retryDelay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelay));
			}

			return Retry(opFactory, (int)millisecondsDelay, maxRetryCount);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, TimeSpan, int)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, int millisecondsRetryDelay, int maxRetryCount = 0)
		{
			if (opFactory == null)
			{
				throw new ArgumentNullException(nameof(opFactory));
			}

			if (millisecondsRetryDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsRetryDelay), millisecondsRetryDelay, Constants.ErrorValueIsLessThanZero);
			}

			if (maxRetryCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, Constants.ErrorValueIsLessThanZero);
			}

			return new RetryResult<TResult>(opFactory, millisecondsRetryDelay, maxRetryCount);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, int, int)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, TimeSpan retryDelay, int maxRetryCount = 0)
		{
			var millisecondsDelay = (long)retryDelay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelay));
			}

			return Retry(opFactory, (int)millisecondsDelay, maxRetryCount);
		}

		#endregion

		#region WhenAll

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll{TResult}(IEnumerable{IAsyncOperation{TResult}})"/>
		/// <seealso cref="WhenAll(IAsyncOperation[])"/>
		public static AsyncResult WhenAll(IEnumerable<IAsyncOperation> ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<IAsyncOperation>();

			foreach (var op in ops)
			{
				if (op == null)
				{
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(ops));
				}

				opList.Add(op);
			}

			if (opList.Count == 0)
			{
				return CompletedOperation;
			}

			return new WhenAllResult<VoidResult>(opList.ToArray());
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll(IEnumerable{IAsyncOperation})"/>
		/// <seealso cref="WhenAll{TResult}(IAsyncOperation{TResult}[])"/>
		public static AsyncResult<T[]> WhenAll<T>(IEnumerable<IAsyncOperation<T>> ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<IAsyncOperation<T>>();

			foreach (var op in ops)
			{
				if (op == null)
				{
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(ops));
				}

				opList.Add(op);
			}

			if (opList.Count == 0)
			{
				return FromResult(new T[0]);
			}

			return new WhenAllResult<T>(opList.ToArray());
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll{T}(IAsyncOperation{T}[])"/>
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

			var opArray = new IAsyncOperation[ops.Length];

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(ops));
				}

				opArray[i] = ops[i];
			}

			return new WhenAllResult<VoidResult>(opArray);
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll(IAsyncOperation[])"/>
		/// <seealso cref="WhenAll{T}(IEnumerable{IAsyncOperation{T}})"/>
		public static AsyncResult<T[]> WhenAll<T>(params IAsyncOperation<T>[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				return FromResult(new T[0]);
			}

			var opArray = new IAsyncOperation<T>[ops.Length];

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(ops));
				}

				opArray[i] = ops[i];
			}

			return new WhenAllResult<T>(opArray);
		}

		#endregion

		#region WhenAny

		/// <summary>
		/// Creates an operation that will complete when any of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of any of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAny{T}(T[])"/>
		public static AsyncResult<T> WhenAny<T>(IEnumerable<T> ops) where T : IAsyncOperation
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<T>();

			foreach (var op in ops)
			{
				if (op == null)
				{
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(ops));
				}

				opList.Add(op);
			}

			if (opList.Count == 0)
			{
				throw new ArgumentException(Constants.ErrorListIsEmpty, nameof(ops));
			}

			return new WhenAnyResult<T>(opList.ToArray());
		}

		/// <summary>
		/// Creates an operation that will complete when any of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of any of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAny{T}(IEnumerable{T})"/>
		public static AsyncResult<T> WhenAny<T>(params T[] ops) where T : IAsyncOperation
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				throw new ArgumentException(Constants.ErrorListIsEmpty, nameof(ops));
			}

			var opArray = new T[ops.Length];

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Constants.ErrorListElementIsNull, nameof(ops));
				}

				opArray[i] = ops[i];
			}

			return new WhenAnyResult<T>(opArray);
		}

		#endregion

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
					OnStatusChanged((AsyncOperationStatus)status);
					OnCompleted();
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
		/// Copies state of the specified operation.
		/// </summary>
		internal void CopyCompletionState(IAsyncOperation patternOp, bool completedSynchronously)
		{
			if (!TryCopyCompletionState(patternOp, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attemts to copy state of the specified operation.
		/// </summary>
		internal bool TryCopyCompletionState(IAsyncOperation patternOp, bool completedSynchronously)
		{
			if (patternOp.IsCompletedSuccessfully)
			{
				return TrySetCompleted(completedSynchronously);
			}
			else if (patternOp.IsFaulted || patternOp.IsCanceled)
			{
				return TrySetException(patternOp.Exception, completedSynchronously);
			}

			return false;
		}

		/// <summary>
		/// Adds a completion callback for <c>await</c> implementation.
		/// </summary>
		internal void SetContinuationForAwait(Action continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (!TryAddContinuation(continuation, syncContext))
			{
				continuation();
			}
		}

		/// <summary>
		/// Rethrows the specified <see cref="AggregateException"/>.
		/// </summary>
		internal static bool TryThrowException(AggregateException e)
		{
			if (e != null)
			{
				var inner = e.InnerException ?? e;
#if !NET35
				ExceptionDispatchInfo.Capture(inner).Throw();
#else
				throw inner;
#endif
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public AsyncOperationStatus Status => (AsyncOperationStatus)(_flags & _statusMask);

		/// <inheritdoc/>
		public AggregateException Exception => (_flags & _flagCompleted) != 0 ? _exception : null;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => (_flags & _statusMask) == StatusRanToCompletion;

		/// <inheritdoc/>
		public bool IsFaulted => (_flags & _statusMask) == StatusFaulted;

		/// <inheritdoc/>
		public bool IsCanceled => (_flags & _statusMask) == StatusCanceled;

		#endregion

		#region IAsyncOperationEvents

		/// <inheritdoc/>
		public event AsyncOperationCallback Completed
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
					value(this);
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

		/// <inheritdoc/>
		public bool TryAddContinuation(IAsyncContinuation continuation)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			return TryAddContinuation(continuation, null);
		}

		/// <inheritdoc/>
		public bool RemoveContinuation(IAsyncContinuation continuation)
		{
			ThrowIfDisposed();

			if (continuation != null)
			{
				return TryRemoveContinuation(continuation);
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

				if (_waitHandle == null)
				{
					var done = IsCompleted;
					var mre = new ManualResetEvent(done);

					if (Interlocked.CompareExchange(ref _waitHandle, mre, null) != null)
					{
						// Another thread created this object's event; dispose the event we just created.
#if NET35
						mre.Close();
#else
						mre.Dispose();
#endif
					}
					else if (!done && IsCompleted)
					{
						// We published the event as unset, but the operation has subsequently completed;
						// set the event state properly so that callers do not deadlock.
						_waitHandle.Set();
					}
				}

				return _waitHandle;
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
		/// Also, <see cref="Dispose()"/> may only be called on an <see cref="AsyncResult"/> that is in one of
		/// the final states: <see cref="AsyncOperationStatus.RanToCompletion"/>, <see cref="AsyncOperationStatus.Faulted"/> or
		/// <see cref="AsyncOperationStatus.Canceled"/>.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if the operation is not completed.</exception>
		/// <seealso cref="Dispose(bool)"/>
		public void Dispose()
		{
			if (!IsCompleted)
			{
				throw new InvalidOperationException(Constants.ErrorOperationIsNotCompleted);
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

		/// <summary>
		/// Gets a string representing the operation state. For debugger only.
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class. For internal use only.
		/// </summary>
		private AsyncResult(int flags)
		{
			if (flags == StatusFaulted)
			{
				_exception = new AggregateException();
			}
			else if (flags == StatusCanceled)
			{
				_exception = new AggregateException(new OperationCanceledException());
			}

			if (flags > StatusRunning)
			{
				_continuation = _continuationCompletionSentinel;
				_flags = flags | _flagCompletedSynchronously;
			}
			else
			{
				_flags = flags;
			}
		}

		/// <summary>
		/// Attempts to register a continuation object. For internal use only.
		/// </summary>
		/// <param name="continuation">The continuation object to add.</param>
		/// <param name="syncContext">A <see cref="SynchronizationContext"/> instance to execute continuation on.</param>
		/// <returns>Returns <see langword="true"/> if the continuation was added; <see langword="false"/> otherwise.</returns>
		private bool TryAddContinuation(object continuation, SynchronizationContext syncContext)
		{
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				continuation = new AsyncContinuation(syncContext, continuation);
			}

			return TryAddContinuation(continuation);
		}

		/// <summary>
		/// Attempts to register a continuation object. For internal use only.
		/// </summary>
		/// <param name="valueToAdd">The continuation object to add.</param>
		/// <returns>Returns <see langword="true"/> if the continuation was added; <see langword="false"/> otherwise.</returns>
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

		/// <summary>
		/// Attempts to remove the specified continuation. For internal use only.
		/// </summary>
		/// <param name="valueToRemove">The continuation object to remove.</param>
		/// <returns>Returns <see langword="true"/> if the continuation was removed; <see langword="false"/> otherwise.</returns>
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

		/// <summary>
		/// Invokes the specified continuation instance.
		/// </summary>
		private static void InvokeContinuation(IAsyncOperation op, object continuation)
		{
			if (continuation is IAsyncContinuation c)
			{
				c.Invoke(op);
			}
			else
			{
				AsyncContinuation.InvokeDelegate(op, continuation);
			}
		}

		#endregion
	}
}
