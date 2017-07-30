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
	[DebuggerDisplay("Status = {Status}, Progress={Progress}")]
	public partial class AsyncResult : IAsyncOperationController, IAsyncContinuationContainer, IAsyncOperation, IEnumerator
	{
		#region data

		private const string _errorOpCompleted = "The operation is already completed.";
		private const string _errorOpStarted = "The operation is already started.";
		private const string _errorOpFaulted = "The operation result is not available.";

		private readonly bool _completedSynchronously;
		private readonly object _asyncState;

#if !UNITYFX_NET35
		private CancellationToken _cancellationToken;
#endif
		private EventWaitHandle _waitHandle;
		private Exception _exception;
		private object _current;
		private Action _continuation;

		/*
		 * Operation progress in range [0,1]. Value -1 means that the object is disposed.
		 */
		private float _progress;

		/*
		 * Operation status. The field type is integer (not enum) because it is used with interlocked methods.
		 *
		 * Possible values (should match AsyncOperationStatus constants):
		 * -1: Initialized (initialized but has not yet been scheduled);
		 *  0: Running;
		 *  1: Success (completed without errors);
		 *  2: Faulted (completed with errors);
		 *  3: Canceled;
		 *  4: Disposed.
		 *
		 * Do not modify this field outside class constructor manually; use SetStatus instead.
		 */
		private volatile int _status;

		#endregion

		#region interface

		internal const int StatusInitialized = -1;
		internal const int StatusRunning = 0;
		internal const int StatusCompleted = 1;
		internal const int StatusFaulted = 2;
		internal const int StatusCanceled = 3;

		/// <summary>
		/// Number of milliseconds to wait until the next <see cref="IAsyncResult.IsCompleted"/> check.
		/// </summary>
		public const int WaitSleepTimeout = 32;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="status">Initial operation status.</param>
		internal AsyncResult(object asyncState, int status)
		{
			Debug.Assert(status >= -1 && status <= 3, "Invalid status value");

			_completedSynchronously = status > StatusRunning;
			_asyncState = asyncState;
			_status = status;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="status">Initial operation status.</param>
		public AsyncResult(object asyncState, AsyncOperationStatus status = AsyncOperationStatus.Running)
			: this(asyncState, (int)status)
		{
		}

#if !UNITYFX_NET35
		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <param name="status">Initial operation status.</param>
		public AsyncResult(object asyncState, CancellationToken cancellationToken, AsyncOperationStatus status = AsyncOperationStatus.Running)
			: this(asyncState, (int)status)
		{
			_cancellationToken = cancellationToken;
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="e">Exception instance.</param>
		public AsyncResult(object asyncState, Exception e)
		{
			_completedSynchronously = true;
			_asyncState = asyncState;
			_status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;
		}

		/// <summary>
		/// Sets the value of <see cref="Current"/> property. Use with care.
		/// </summary>
		protected void SetCurrent(object current)
		{
			_current = current;
		}

		/// <summary>
		/// Finished the operation (if it is not already finished). Do not use this method unless absolutely needed.
		/// </summary>
		/// <seealso cref="FireCompleted"/>
		protected bool TrySetStatus(int newStatus)
		{
			ThrowIfDisposed();

			if (_status < StatusCompleted)
			{
				if (Interlocked.CompareExchange(ref _status, newStatus, StatusInitialized) == StatusInitialized ||
					Interlocked.CompareExchange(ref _status, newStatus, StatusRunning) == StatusRunning)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Triggers completed event (if any).
		/// </summary>
		protected void FireCompleted()
		{
			try
			{
				OnCompleted();
			}
			finally
			{
				_waitHandle?.Set();

				// TODO: synchronization needed
				_continuation?.Invoke();
				_continuation = null;
			}
		}

		/// <summary>
		/// Start the operation.
		/// </summary>
		protected void SetStarted()
		{
			ThrowIfDisposed();

			if (_status >= StatusRunning)
			{
				throw new InvalidOperationException(_errorOpStarted);
			}

			_status = StatusRunning;
		}

		/// <summary>
		/// Returns an exception for the operation instance. Never returns <c>null</c>.
		/// </summary>
		protected Exception GetExceptionSafe()
		{
			if (_exception == null)
			{
				if (_status == StatusCanceled)
				{
					return new OperationCanceledException();
				}
				else
				{
					return new Exception();
				}
			}

			return _exception;
		}

		/// <summary>
		/// Throws exception if the operation has failed.
		/// </summary>
		protected void ThrowIfFaulted()
		{
			if (_progress < 0)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			if (_status > StatusCompleted)
			{
				throw new InvalidOperationException(_errorOpFaulted, _exception);
			}
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> if this operation has been disposed.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if (_progress < 0)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion

		#region virtual interface

		/// <summary>
		/// Updates the operation state. Called by <see cref="MoveNext"/>. Default implementation does nothing.
		/// </summary>
		/// <remarks>
		/// Do not reference public class methods and properties in this method (except <see cref="SetCompleted"/>)
		/// because their implementation may reference <see cref="MoveNext"/> and cause endless recursion.
		/// </remarks>
		/// <seealso cref="OnCompleted()"/>
		protected virtual void OnUpdate()
		{
		}

		/// <summary>
		/// Notifies the observer that the provider has finished sending push-based notifications.
		/// </summary>
		/// <seealso cref="OnUpdate()"/>
		protected virtual void OnCompleted()
		{
		}

		#endregion

		#region IAsyncContinuationContainer

		/// <inheritdoc/>
		public void AddContinuation(Action continuation)
		{
			// TODO: make sure other threads do not interfere
			_continuation += continuation;
		}

		/// <inheritdoc/>
		public void RemoveContinuation(Action continuation)
		{
			// TODO: make sure other threads do not interfere
			_continuation -= continuation;
		}

		#endregion

		#region IAsyncOperationController

		/// <inheritdoc/>
		public void SetProgress(float progress)
		{
			if (progress < 0)
			{
				_progress = 0;
			}
			else if (progress > 1)
			{
				_progress = 1;
			}
			else
			{
				_progress = progress;
			}
		}

		/// <inheritdoc/>
		public void SetCanceled()
		{
			if (TrySetStatus(StatusCanceled))
			{
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetCanceled()
		{
			if (TrySetStatus(StatusCanceled))
			{
				FireCompleted();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void SetException(Exception e)
		{
			var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetException(Exception e)
		{
			var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				FireCompleted();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void SetCompleted()
		{
			if (TrySetStatus(StatusCompleted))
			{
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetCompleted()
		{
			if (TrySetStatus(StatusCompleted))
			{
				FireCompleted();
				return true;
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public float Progress => _status == StatusInitialized ? 0 : _status > StatusRunning ? 1 : _progress;

		/// <inheritdoc/>
		public AsyncOperationStatus Status => (AsyncOperationStatus)_status;

		/// <inheritdoc/>
		public Exception Exception => _exception;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => _status == StatusCompleted;

		/// <inheritdoc/>
		public bool IsFaulted => _status > StatusCompleted;

		/// <inheritdoc/>
		public bool IsCanceled => _status == StatusCanceled;

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
						mre.Close();
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
		public bool CompletedSynchronously => _completedSynchronously;

		/// <inheritdoc/>
		public bool IsCompleted => _status > StatusRunning;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		public object Current => _current;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (_status > StatusRunning)
			{
				// The operation has completed.
				return false;
			}
			else if (_status == StatusRunning)
			{
				// The operation is pending.
				try
				{
#if UNITYFX_NET35
					OnUpdate();
#else
					if (_cancellationToken.IsCancellationRequested)
					{
						TrySetCanceled();
					}
					else
					{
						OnUpdate();
					}
#endif
				}
				catch (Exception e)
				{
					TrySetException(e);
				}

				return _status == StatusRunning;
			}

			// The operation is not started yet.
			return true;
		}

		/// <inheritdoc/>
		public void Reset()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			// NOTE: thread safety is not required
			if (this == _completed || this == _canceled)
			{
				// Do not dispose the _completed and _canceled instances because they might be reused.
			}
			else if (_progress >= 0)
			{
				try
				{
					TrySetCanceled();
				}
				finally
				{
					_progress = -1;
					_waitHandle?.Close();

					GC.SuppressFinalize(this);
				}
			}
		}

		#endregion

		#region Object

		/// <inheritdoc/>
		public override string ToString()
		{
			if (_progress < 0)
			{
				return "{Disposed}";
			}
			else if (_exception != null)
			{
				return $"{{Status={Status}, Progress={Progress.ToString(NumberFormatInfo.InvariantInfo)}, Exception={_exception?.Message}}}";
			}
			else
			{
				return $"{{Status={Status}, Progress={Progress.ToString(NumberFormatInfo.InvariantInfo)}}}";
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
