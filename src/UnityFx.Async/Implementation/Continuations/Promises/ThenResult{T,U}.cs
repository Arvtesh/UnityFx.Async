// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal class ThenResult<T, U> : ContinuationResult<U>, IAsyncContinuation
	{
		#region data

		private readonly object _successCallback;
		private readonly Action<Exception> _errorCallback;

		private IAsyncOperation _continuation;

		#endregion

		#region interface

		public ThenResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
			: base(op)
		{
			_successCallback = successCallback;
			_errorCallback = errorCallback;

			// NOTE: Cannot move this to base class because this call might trigger virtual Invoke
			if (!op.TryAddContinuation(this))
			{
				InvokeInternal(op, true);
			}
		}

		protected virtual void InvokeSuccessCallback(IAsyncOperation op, bool completedSynchronously, object continuation)
		{
			switch (continuation)
			{
				case Action a:
					a.Invoke();
					TrySetCompleted(completedSynchronously);
					break;

				case Action<T> a1:
					a1.Invoke((op as IAsyncOperation<T>).Result);
					TrySetCompleted(completedSynchronously);
					break;

				case Func<IAsyncOperation<U>> f3:
					_continuation = f3();
					_continuation.AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<IAsyncOperation> f1:
					_continuation = f1();
					_continuation.AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation<U>> f4:
					_continuation = f4((op as IAsyncOperation<T>).Result);
					_continuation.AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation> f2:
					_continuation = f2((op as IAsyncOperation<T>).Result);
					_continuation.AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				default:
					TrySetCanceled(completedSynchronously);
					break;
			}
		}

		#endregion

		#region ContinuationResult

		protected sealed override void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully)
			{
				if (IsCancellationRequested)
				{
					TrySetCanceled(completedSynchronously);
				}
				else
				{
					InvokeSuccessCallback(op, completedSynchronously, _successCallback);
				}
			}
			else
			{
				InvokeErrorCallback(op, completedSynchronously);
			}
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			base.OnCancel();

			if (_continuation is IAsyncCancellable c)
			{
				c.Cancel();
			}
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			InvokeInternal(op, false);
		}

		#endregion

		#region implementation

		private void InvokeInternal(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully || _errorCallback != null)
			{
				InvokeOnSyncContext(op, completedSynchronously);
			}
			else
			{
				TrySetException(op.Exception, completedSynchronously);
			}
		}

		private void InvokeErrorCallback(IAsyncOperation op, bool completedSynchronously)
		{
			_errorCallback?.Invoke(op.Exception.InnerException);
			TrySetException(op.Exception, completedSynchronously);
		}

		#endregion
	}
}
