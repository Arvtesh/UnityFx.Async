// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
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
	public class AsyncResult : IAsyncOperation, IAsyncOperationController, IEnumerator
	{
		#region data

		private const int _statusCompletedFlag = 0x00100000;
		private const int _statusSynchronousFlag = 0x00200000;
		private const int _statusDisposedFlag = 0x00400000;
		private const int _statusMask = 0x0000000f;

		private readonly AsyncCallback _asyncCallback;
		private readonly object _asyncState;

		private EventWaitHandle _waitHandle;
		private Exception _exception;
		private int _status;
		private Action _continuation;

		#endregion

		#region interface

		/// <summary>
		/// Returns <see langword="true"/> if the operation is disposed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		protected bool IsDisposed => (_status & _statusDisposedFlag) != 0;

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
		/// Throws exception if the operation has failed.
		/// </summary>
		protected internal void ThrowIfFaulted()
		{
			if (IsFaulted)
			{
				if (_exception != null)
				{
#if !NET35
					ExceptionDispatchInfo.Capture(_exception).Throw();
#else
					throw _exception;
#endif
				}
				else if (IsCanceled)
				{
					throw new OperationCanceledException(GetOperationName());
				}
				else
				{
					throw new Exception(GetOperationName());
				}
			}
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> if this operation has been disposed.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if ((_status & _statusDisposedFlag) != 0)
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
			if (disposing)
			{
				_status |= _statusDisposedFlag;

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
				status |= _statusCompletedFlag;

				if (completedSynchronously)
				{
					status |= _statusSynchronousFlag;
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
				throw new NullReferenceException(nameof(continuation));
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
		public AsyncOperationStatus Status => (AsyncOperationStatus)(_status & _statusMask);

		/// <inheritdoc/>
		public Exception Exception => _exception;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => (_status & _statusMask) == StatusRanToCompletion;

		/// <inheritdoc/>
		public bool IsFaulted => (_status & _statusMask) == StatusFaulted;

		/// <inheritdoc/>
		public bool IsCanceled => (_status & _statusMask) == StatusCanceled;

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
		public bool CompletedSynchronously => (_status & _statusSynchronousFlag) != 0;

		/// <inheritdoc/>
		public bool IsCompleted => (_status & _statusCompletedFlag) != 0;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		public object Current => null;

		/// <inheritdoc/>
		public bool MoveNext() => _status == StatusRunning;

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

		private bool TrySetStatusInternal(int newStatus)
		{
			var status = _status;

			if ((status & _statusCompletedFlag) == 0)
			{
				var status0 = status & _statusMask;
				var status1 = newStatus & _statusMask;

				if (status0 < status1)
				{
					if (status0 == StatusCreated)
					{
						return Interlocked.CompareExchange(ref _status, newStatus, StatusCreated) == StatusCreated;
					}
					else if (status0 == StatusScheduled)
					{
						return Interlocked.CompareExchange(ref _status, newStatus, StatusScheduled) == StatusScheduled;
					}
					else
					{
						return Interlocked.CompareExchange(ref _status, newStatus, StatusRunning) == StatusRunning;
					}
				}
			}

			return false;
		}

		#endregion
	}
}
