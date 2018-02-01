// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	/// <seealso cref="IAsyncResult"/>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AsyncResult : IAsyncOperation, IAsyncOperationController, IEnumerator
	{
		#region data

		private const int _flagCompleted = 0x00100000;
		private const int _flagSynchronous = 0x00200000;
		private const int _flagCompletedSynchronously = _flagCompleted | _flagSynchronous;
		private const int _flagDisposed = 0x00400000;
		private const int _flagDoNotDispose = 0x10000000;
		private const int _statusMask = 0x0000000f;

		private readonly AsyncCallback _asyncCallback;
		private readonly object _asyncState;

		private static IAsyncOperation _completedOperation;

		private EventWaitHandle _waitHandle;
		private Exception _exception;
		private int _flags;
		private Action _continuation;

		#endregion

		#region interface

		/// <summary>
		/// Returns <see langword="true"/> if the operation is disposed; <see langword="false"/> otherwise. Read only.
		/// </summary>
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
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;
		}

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <seealso cref="SetRunning"/>
		public void SetScheduled()
		{
			if (!TrySetStatusInternal(StatusScheduled))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <seealso cref="SetScheduled"/>
		public void SetRunning()
		{
			if (!TrySetStatusInternal(StatusRunning))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> if this operation has been disposed.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if ((_flags & _flagDisposed) != 0)
			{
				throw new ObjectDisposedException(GetOperationName());
			}
		}

		/// <summary>
		/// Returns the operation name.
		/// </summary>
		/// <returns>Name of the operation.</returns>
		protected string GetOperationName()
		{
			return GetType().Name;
		}

		#endregion

		#region virtual interface

		/// <summary>
		/// Called when the operation state has changed.
		/// </summary>
		protected virtual void OnStatusChanged()
		{
		}

		/// <summary>
		/// Called when the operation is completed.
		/// </summary>
		protected virtual void OnCompleted()
		{
			_waitHandle?.Set();
			_asyncCallback?.Invoke(this);
			_continuation?.Invoke();
		}

		/// <summary>
		/// Releases unmanaged resources used by the object.
		/// </summary>
		/// <param name="disposing">Should be <see langword="true"/> if the method is called from <see cref="Dispose()"/>; <see langword="false"/> otherwise.</param>
		/// <seealso cref="Dispose()"/>
		/// <seealso cref="ThrowIfDisposed"/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && (_flags & _flagDoNotDispose) == 0)
			{
				_flags |= _flagDisposed;

				if (_waitHandle != null)
				{
					_waitHandle.Close();
					_waitHandle = null;
				}
			}
		}

		#endregion

		#region static interface

		/// <summary>
		/// Returns an operation that's already been completed successfully.
		/// </summary>
		/// <remarks>
		/// May not always return the same instance.
		/// </remarks>
		public static IAsyncOperation Completed
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
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="millisecondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		public static IAsyncOperation Delay(int millisecondsDelay)
		{
			if (millisecondsDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
			}

			if (millisecondsDelay == 0)
			{
				return Completed;
			}

			if (millisecondsDelay == Timeout.Infinite)
			{
				return new AsyncResult();
			}

			return new DelayAsyncResult(millisecondsDelay);
		}

		/// <summary>
		/// Creates a task that completes after a specified time interval.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned task, or <c>TimeSpan.FromMilliseconds(-1)</c> to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="delay"/> represents a negative time interval other than <c>TimeSpan.FromMillseconds(-1)</c>.</exception>
		/// <returns>An operation that represents the time delay.</returns>
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
		/// tt
		/// </summary>
		/// <param name="waitHandle"></param>
		/// <param name="asyncResult"></param>
		/// <returns></returns>
		public static EventWaitHandle TryCreateAsyncWaitHandle(ref EventWaitHandle waitHandle, IAsyncResult asyncResult)
		{
			if (waitHandle == null)
			{
				var done = asyncResult.IsCompleted;
				var mre = new ManualResetEvent(done);

				if (Interlocked.CompareExchange(ref waitHandle, mre, null) != null)
				{
					// Another thread created this object's event; dispose the event we just created.
					mre.Close();
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

		internal bool TrySetStatus(int status, bool completedSynchronously)
		{
			if (status > StatusRunning)
			{
				status |= _flagCompleted;

				if (completedSynchronously)
				{
					status |= _flagSynchronous;
				}
			}

			return TrySetStatusInternal(status);
		}

		#endregion

		#region IAsyncContinuationContainer

		/// <inheritdoc/>
		public void AddContinuation(Action continuation)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			if (IsCompleted)
			{
				continuation.Invoke();
			}
			else
			{
				// NOTE: the code is adapted from https://stackoverflow.com/questions/3522361/add-delegate-to-event-thread-safety
				Action d1 = _continuation;
				Action d2, d3;

				do
				{
					d2 = d1;
					d3 = (Action)Delegate.Combine(d2, continuation);
					d1 = Interlocked.CompareExchange(ref _continuation, d3, d2);
				}
				while (d1 != d2);
			}
		}

		/// <inheritdoc/>
		public void RemoveContinuation(Action continuation)
		{
			ThrowIfDisposed();

			if (continuation != null)
			{
				// NOTE: the code is adapted from https://stackoverflow.com/questions/3522361/add-delegate-to-event-thread-safety
				Action d1 = _continuation;
				Action d2, d3;

				do
				{
					d2 = d1;
					d3 = (Action)Delegate.Remove(d2, continuation);
					d1 = Interlocked.CompareExchange(ref _continuation, d3, d2);
				}
				while (d1 != d2);
			}
		}

		#endregion

		#region IAsyncOperationController

		/// <inheritdoc/>
		public void SetCanceled(bool completedSynchronously)
		{
			if (!TrySetCanceled(completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetCanceled(bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCanceled, completedSynchronously))
			{
				OnCompleted();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void SetException(Exception e, bool completedSynchronously)
		{
			if (!TrySetException(e, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetException(Exception e, bool completedSynchronously)
		{
			ThrowIfDisposed();

			var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

			if (TrySetStatus(status, completedSynchronously))
			{
				_exception = e;
				OnCompleted();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void SetCompleted(bool completedSynchronously)
		{
			if (!TrySetCompleted(completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetCompleted(bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusRanToCompletion, completedSynchronously))
			{
				OnCompleted();
				return true;
			}

			return false;
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
		public object Current => null;

		/// <inheritdoc/>
		public bool MoveNext() => _flags == StatusRunning;

		/// <inheritdoc/>
		public void Reset() => throw new NotSupportedException();

		#endregion

		#region IDisposable

		/// <inheritdoc/>
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

		#region implementation

		private string DebuggerDisplay
		{
			get
			{
				var result = GetOperationName();
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

		private bool TrySetStatusInternal(int newStatus)
		{
			var status = _flags;

			if ((status & _flagCompleted) == 0)
			{
				var status0 = status & _statusMask;
				var status1 = newStatus & _statusMask;

				if (status0 < status1)
				{
					var result = false;

					if (status0 == StatusCreated)
					{
						result = Interlocked.CompareExchange(ref _flags, newStatus, StatusCreated) == StatusCreated;
					}
					else if (status0 == StatusScheduled)
					{
						result = Interlocked.CompareExchange(ref _flags, newStatus, StatusScheduled) == StatusScheduled;
					}
					else
					{
						result = Interlocked.CompareExchange(ref _flags, newStatus, StatusRunning) == StatusRunning;
					}

					if (result)
					{
						OnStatusChanged();
					}

					return result;
				}
			}

			return false;
		}

		#endregion
	}
}
