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
	/// A lightweight <c>net35</c>-compatible asynchronous operation for <c>Unity3d</c>.
	/// </summary>
	/// <remarks>
	/// <para>This class is the core entity of the library. In many aspects it mimics <c>Task</c>
	/// interface and behaviour. For example, any <see cref="AsyncResult"/> instance can have any
	/// number of continuations (added either explicitly via <c>TryAddCompletionCallback</c>
	/// call or implicitly using <c>async</c>/<c>await</c> keywords). These continuations can be
	/// invoked on a captured <see cref="SynchronizationContext"/>. The class inherits <see cref="IAsyncResult"/>
	/// (just like <c>Task</c>) and can be used to implement Asynchronous Programming Model (APM).
	/// There are operation state accessors that can be used exactly like corresponding properties of <c>Task</c>.
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
	/// <seealso href="http://www.what-could-possibly-go-wrong.com/promises-for-game-development/">Promises for game development</seealso>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/">How to implement the IAsyncResult design pattern</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming">Task-based Asynchronous Pattern (TAP)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Task</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1">TaskCompletionSource</seealso>
	/// <seealso cref="AsyncCompletionSource"/>
	/// <seealso cref="AsyncResult{T}"/>
	/// <seealso cref="IAsyncResult"/>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public partial class AsyncResult : IAsyncOperation, IAsyncCancellable, IEnumerator
	{
		#region data

		private const int _flagCompletionReserved = 0x00010000;
		private const int _flagCompleted = 0x00020000;
		private const int _flagSynchronous = 0x00040000;
		private const int _flagCancellationRequested = 0x00100000;
		private const int _flagCompletedSynchronously = _flagCompleted | _flagCompletionReserved | _flagSynchronous;
		private const int _flagDisposed = 0x01000000;
		private const int _flagDoNotDispose = 0x10000000;
		private const int _flagRunContinuationsAsynchronously = 0x20000000;
		private const int _statusMask = 0x0000000f;
		private const int _resetMask = 0x70000000;

		private static readonly object _continuationCompletionSentinel = new object();
		private readonly object _asyncState;

		private AggregateException _exception;

#if UNITYFX_NOT_THREAD_SAFE

		private object _continuation;
		private int _flags;

#else

		private EventWaitHandle _waitHandle;
		private volatile object _continuation;
		private volatile int _flags;

#endif

		#endregion

		#region interface

		/// <summary>
		/// Gets a value indicating whether the operation instance is disposed.
		/// </summary>
		/// <value>The disposed flag.</value>
		protected bool IsDisposed => (_flags & _flagDisposed) != 0;

		/// <summary>
		/// Gets a value indicating whether the operation cancellation was requested.
		/// </summary>
		/// <value>The cancellation request flag.</value>
		protected bool IsCancellationRequested => (_flags & _flagCancellationRequested) != 0;

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
		/// <returns>Returns <see langword="true"/> if the operation status was changed to <see cref="AsyncOperationStatus.Running"/>; <see langword="false"/> otherwise.</returns>
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
		/// Called when the operation cancellation has been requested. Default implementation throws <see cref="NotSupportedException"/>.
		/// </summary>
		/// <seealso cref="Cancel"/>
		protected virtual void OnCancel()
		{
			throw new NotSupportedException();
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
#if UNITYFX_NOT_THREAD_SAFE

			var continuation = _continuation;

			if (continuation != null)
			{
				if (continuation is IEnumerable continuationList)
				{
					foreach (var item in continuationList)
					{
						InvokeContinuation(this, item);
					}
				}
				else
				{
					InvokeContinuation(this, continuation);
				}
			}

#else

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
			finally
			{
				_waitHandle?.Set();
			}

#endif
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

#if !UNITYFX_NOT_THREAD_SAFE

				if (_waitHandle != null)
				{
#if NET35
					_waitHandle.Close();
#else
					_waitHandle.Dispose();
#endif
					_waitHandle = null;
				}

#endif
			}
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

#if UNITYFX_NOT_THREAD_SAFE

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

			_flags = (flags & ~_statusMask) | newStatus;
			OnStatusChanged((AsyncOperationStatus)newStatus);
			return true;

#else

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

#endif
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

#if UNITYFX_NOT_THREAD_SAFE

			var flags = _flags;

			if ((flags & (_flagCompletionReserved | _flagCompleted)) != 0)
			{
				return false;
			}

			_flags = (flags & ~_statusMask) | status;
			OnStatusChanged((AsyncOperationStatus)status);
			OnCompleted();
			return true;

#else

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

#endif
		}

		/// <summary>
		/// Initiates operation completion. Should only be used in pair with <see cref="SetCompleted(int, bool)"/>.
		/// </summary>
		internal bool TryReserveCompletion()
		{
#if UNITYFX_NOT_THREAD_SAFE

			var flags = _flags;

			if ((flags & (_flagCompletionReserved | _flagCompleted)) != 0)
			{
				return false;
			}

			_flags = flags | _flagCompletionReserved;
			return true;
#else

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

#endif
		}

		/// <summary>
		/// Attempts to add a new flag value.
		/// </summary>
		internal bool TrySetFlag(int newFlag)
		{
#if UNITYFX_NOT_THREAD_SAFE

			var flags = _flags;

			if ((flags & (newFlag | _flagCompletionReserved | _flagCompleted)) != 0)
			{
				return false;
			}

			_flags = flags | newFlag;
			return true;
#else

			do
			{
				var flags = _flags;

				if ((flags & (newFlag | _flagCompletionReserved | _flagCompleted)) != 0)
				{
					return false;
				}

				if (Interlocked.CompareExchange(ref _flags, flags | newFlag, flags) == flags)
				{
					return true;
				}
			}
			while (true);

#endif
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

			// Set completed status. After this line IsCompleted will return true.
#if UNITYFX_NOT_THREAD_SAFE
			_flags = oldFlags | newFlags;
#else
			Interlocked.Exchange(ref _flags, oldFlags | newFlags);
#endif

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

			if (!TryAddContinuationInternal(continuation, syncContext))
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

#if UNITYFX_NOT_THREAD_SAFE

				if (!TryAddContinuationInternal(value))
				{
					value(this);
				}

#else

				if (!TryAddContinuationInternal(value, SynchronizationContext.Current))
				{
					value(this);
				}

#endif
			}
			remove
			{
				ThrowIfDisposed();

				if (value != null)
				{
					TryRemoveContinuationInternal(value);
				}
			}
		}

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(AsyncOperationCallback action)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

#if UNITYFX_NOT_THREAD_SAFE

			return TryAddContinuationInternal(action);

#else

			return TryAddContinuationInternal(action, SynchronizationContext.Current);

#endif
		}

		/// <inheritdoc/>
		public bool TryAddCompletionCallback(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddContinuationInternal(action, syncContext);
		}

		/// <inheritdoc/>
		public bool RemoveCompletionCallback(AsyncOperationCallback action)
		{
			ThrowIfDisposed();

			if (action != null)
			{
				return TryRemoveContinuationInternal(action);
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

#if UNITYFX_NOT_THREAD_SAFE

			return TryAddContinuationInternal(continuation);

#else

			return TryAddContinuationInternal(continuation, null);

#endif
		}

		/// <inheritdoc/>
		public bool RemoveContinuation(IAsyncContinuation continuation)
		{
			ThrowIfDisposed();

			if (continuation != null)
			{
				return TryRemoveContinuationInternal(continuation);
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
#if UNITYFX_NOT_THREAD_SAFE

				throw new NotSupportedException();

#else

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

#endif
			}
		}

		/// <inheritdoc/>
		public object AsyncState => _asyncState;

		/// <inheritdoc/>
		public bool CompletedSynchronously => (_flags & _flagSynchronous) != 0;

		/// <inheritdoc/>
		public bool IsCompleted => (_flags & _flagCompleted) != 0;

		#endregion

		#region IAsyncCancellable

		/// <inheritdoc/>
		public void Cancel()
		{
			ThrowIfDisposed();

			if (TrySetFlag(_flagCancellationRequested))
			{
				OnCancel();
			}
		}

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
		private bool TryAddContinuationInternal(object continuation, SynchronizationContext syncContext)
		{
			var runContinuationsAsynchronously = (_flags & _flagRunContinuationsAsynchronously) != 0;

			if ((syncContext != null && syncContext.GetType() != typeof(SynchronizationContext)) || runContinuationsAsynchronously)
			{
				continuation = new AsyncContinuation(syncContext, continuation, runContinuationsAsynchronously);
			}

			return TryAddContinuationInternal(continuation);
		}

#if UNITYFX_NOT_THREAD_SAFE

		/// <summary>
		/// Attempts to register a continuation object. For internal use only.
		/// </summary>
		/// <param name="valueToAdd">The continuation object to add.</param>
		/// <returns>Returns <see langword="true"/> if the continuation was added; <see langword="false"/> otherwise.</returns>
		private bool TryAddContinuationInternal(object valueToAdd)
		{
			var oldValue = _continuation;

			// Quick return if the operation is completed.
			if (oldValue != _continuationCompletionSentinel)
			{
				// If no continuation is stored yet, try to store it as _continuation.
				if (oldValue == null)
				{
					_continuation = valueToAdd;
				}

				// Logic for the case where we were previously storing a single continuation.
				if (oldValue is IList list)
				{
					list.Add(valueToAdd);
				}
				else
				{
					_continuation = new List<object>() { oldValue, valueToAdd };
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to remove the specified continuation. For internal use only.
		/// </summary>
		/// <param name="valueToRemove">The continuation object to remove.</param>
		/// <returns>Returns <see langword="true"/> if the continuation was removed; <see langword="false"/> otherwise.</returns>
		private bool TryRemoveContinuationInternal(object valueToRemove)
		{
			var value = _continuation;

			if (value != _continuationCompletionSentinel)
			{
				if (value is IList list)
				{
					list.Remove(valueToRemove);
				}
				else
				{
					_continuation = null;
				}

				return true;
			}

			return false;
		}

#else

		/// <summary>
		/// Attempts to register a continuation object. For internal use only.
		/// </summary>
		/// <param name="valueToAdd">The continuation object to add.</param>
		/// <returns>Returns <see langword="true"/> if the continuation was added; <see langword="false"/> otherwise.</returns>
		private bool TryAddContinuationInternal(object valueToAdd)
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
		private bool TryRemoveContinuationInternal(object valueToRemove)
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

#endif

		/// <summary>
		/// Invokes the specified continuation instance.
		/// </summary>
		private void InvokeContinuation(object continuation)
		{
			if (continuation is IAsyncContinuation c)
			{
				c.Invoke(this);
			}
			else
			{
				AsyncContinuation.InvokeDelegate(this, continuation);
			}
		}

		#endregion
	}
}
