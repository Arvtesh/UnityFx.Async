// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class ThenResult<T, U> : PromiseResult<U>, IAsyncContinuation
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
			op.AddContinuation(this);
		}

		#endregion

		#region PromiseResult

		protected override void InvokeCallbacks(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully)
			{
				if (InvokeSuccessCallback(op))
				{
					TrySetCompleted(completedSynchronously);
				}
			}
			else
			{
				InvokeErrorCallback(op);
				TrySetException(op.Exception, completedSynchronously);
			}
		}

		#endregion

		#region IAsyncContinuation

		public override void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully || _errorCallback != null)
			{
				base.Invoke(op, completedSynchronously);
			}
			else
			{
				TrySetException(op.Exception, completedSynchronously);
			}
		}

		#endregion

		#region implementation

		private bool InvokeSuccessCallback(IAsyncOperation op)
		{
			var result = false;

			switch (_successCallback)
			{
				case Action a:
					a.Invoke();
					result = true;
					break;

				case Action<T> a1:
					a1.Invoke((op as IAsyncOperation<T>).Result);
					result = true;
					break;

				case Func<IAsyncOperation<U>> f3:
					f3().AddCompletionCallback(op2 => TryCopyCompletionState(op2 as IAsyncOperation<U>, false), null);
					break;

				case Func<IAsyncOperation> f1:
					f1().AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation<U>> f4:
					f4((op as IAsyncOperation<T>).Result).AddCompletionCallback(op2 => TryCopyCompletionState(op2 as IAsyncOperation<U>, false), null);
					break;

				case Func<T, IAsyncOperation> f2:
					f2((op as IAsyncOperation<T>).Result).AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				default:
					// Should not get here.
					throw new InvalidOperationException();
			}

			return result;
		}

		private void InvokeErrorCallback(IAsyncOperation op)
		{
			_errorCallback?.Invoke(op.Exception.InnerException);
		}

		#endregion
	}
}
