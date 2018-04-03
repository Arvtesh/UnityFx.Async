// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class ThenResult<T, U> : ContinuationResult<U>, IAsyncContinuation
	{
		#region data

		private readonly object _successCallback;
		private readonly Action<Exception> _errorCallback;

		#endregion

		#region interface

		public ThenResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
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
					f3().AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<IAsyncOperation> f1:
					f1().AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation<U>> f4:
					f4((op as IAsyncOperation<T>).Result).AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation> f2:
					f2((op as IAsyncOperation<T>).Result).AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				default:
					TrySetCanceled(completedSynchronously);
					break;
			}
		}

		#endregion

		#region PromiseResult

		protected sealed override void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully)
			{
				InvokeSuccessCallback(op, completedSynchronously, _successCallback);
			}
			else
			{
				InvokeErrorCallback(op, completedSynchronously);
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
