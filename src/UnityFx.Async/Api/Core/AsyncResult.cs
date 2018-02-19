// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
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

		private readonly object _asyncState;

		private static IAsyncOperation _completedOperation;
		private static object _continuationCompletionSentinel = new object();

		private EventWaitHandle _waitHandle;
		private Exception _exception;

		private volatile object _continuation;
		private volatile int _flags;

		#endregion

		#region interface

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
				_exception = new Exception();
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
			_exception = e ?? throw new ArgumentNullException(nameof(e));
			_flags = StatusFaulted | _flagCompletedSynchronously;
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition has failed.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetRunning"/>
		public void Start()
		{
			if (!TrySetRunning())
			{
				throw new InvalidOperationException();
			}
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
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="e">An exception that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="e"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetException(Exception e, bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			if (TryReserveCompletion())
			{
				var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

				_exception = e;
				SetCompleted(status, completedSynchronously);
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

			if (TryReserveCompletion())
			{
				var list = new List<Exception>(exceptions);

				if (list.Count == 0)
				{
					throw new ArgumentException("At least one exception is needed.", nameof(exceptions));
				}

				if (list.Count == 1)
				{
					_exception = list[0];
				}
				else
				{
					_exception = new AggregateException(list);
				}

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
		protected virtual void OnStatusChanged(AsyncOperationStatus status)
		{
		}

		/// <summary>
		/// Called when the operation is started (status is set to <see cref="AsyncOperationStatus.Running"/>).
		/// </summary>
		/// <seealso cref="Status"/>
		protected virtual void OnStarted()
		{
		}

		/// <summary>
		/// Called when the operation is completed.
		/// </summary>
		/// <seealso cref="Status"/>
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
		public static IAsyncOperation CompletedOperation
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
		/// <returns>The canceled operation.</returns>
		/// <seealso cref="FromCanceled{T}"/>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static IAsyncOperation FromCanceled()
		{
			return new AsyncResult(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> that is canceled.
		/// </summary>
		/// <returns>The canceled operation.</returns>
		/// <seealso cref="FromCanceled"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static IAsyncOperation<T> FromCanceled<T>()
		{
			return new AsyncResult<T>(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		/// <returns>The faulted operation.</returns>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromCanceled"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static IAsyncOperation FromException(Exception e)
		{
			return new AsyncResult(e);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		/// <returns>The faulted operation.</returns>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromCanceled{T}"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static IAsyncOperation<T> FromException<T>(Exception e)
		{
			return new AsyncResult<T>(e);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> that has completed with a specified result.
		/// </summary>
		/// <param name="result">The result value with which to complete the operation.</param>
		/// <returns>The completed operation.</returns>
		/// <seealso cref="FromCanceled{T}"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		public static IAsyncOperation<T> FromResult<T>(T result)
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
		public static IAsyncOperation Delay(int millisecondsDelay)
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
		public static IAsyncOperation Delay(TimeSpan delay)
		{
			var millisecondsDelay = (long)delay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			return Delay((int)millisecondsDelay);
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
		internal void SetContinuationForAwait(Action action)
		{
			ThrowIfDisposed();

			if (!TryAddContinuation(action, SynchronizationContext.Current))
			{
				action();
			}
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public AsyncOperationStatus Status => (AsyncOperationStatus)(_flags & _statusMask);

		/// <inheritdoc/>
		public Exception Exception => _exception;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => (_flags & _statusMask) == StatusRanToCompletion;

		/// <inheritdoc/>
		public bool IsFaulted => (_flags & _statusMask) == StatusFaulted;

		/// <inheritdoc/>
		public bool IsCanceled => (_flags & _statusMask) == StatusCanceled;

		#endregion

		#region IAsyncOperationEvents

		/// <inheritdoc/>
		public event EventHandler Completed
		{
			add
			{
				if (value != null)
				{
					TryAddContinuation(value, null);
				}
			}
			remove
			{
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
					state += " (" + _exception.GetType().Name + ')';
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
