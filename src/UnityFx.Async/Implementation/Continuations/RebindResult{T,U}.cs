// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class RebindResult<T, U> : ContinuationResult<U>, IAsyncContinuation
	{
		#region data

		private readonly object _continuation;

		#endregion

		#region interface

		public RebindResult(IAsyncOperation op, object action)
		{
			_continuation = action;

			// NOTE: Cannot move this to base class because this call might trigger virtual Invoke
			if (!op.TryAddContinuation(this))
			{
				InvokeInternal(op, true);
			}
		}

		#endregion

		#region PromiseResult

		protected override void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously)
		{
			switch (_continuation)
			{
				case Func<U> f1:
					TrySetResult(f1(), completedSynchronously);
					break;

				case Func<T, U> f2:
					TrySetResult(f2((op as IAsyncOperation<T>).Result), completedSynchronously);
					break;

				default:
					TrySetCanceled(completedSynchronously);
					break;
			}
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			InvokeOnSyncContext(op, false);
		}

		#endregion

		#region implementation

		private void InvokeInternal(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully)
			{
				InvokeOnSyncContext(op, completedSynchronously);
			}
			else
			{
				TrySetException(op.Exception, completedSynchronously);
			}
		}

		#endregion
	}
}
