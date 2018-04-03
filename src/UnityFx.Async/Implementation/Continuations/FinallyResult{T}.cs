// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class FinallyResult<T> : ContinuationResult<T>, IAsyncContinuation
	{
		#region data

		private readonly Action _continuation;

		#endregion

		#region interface

		public FinallyResult(IAsyncOperation op, Action action)
		{
			_continuation = action;

			// NOTE: Cannot move this to base class because this call might trigger virtual Invoke
			if (!op.TryAddContinuation(this))
			{
				InvokeOnSyncContext(op, true);
			}
		}

		#endregion

		#region PromiseResult

		protected override void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously)
		{
			_continuation();
			TrySetCompleted(completedSynchronously);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			InvokeOnSyncContext(op, false);
		}

		#endregion
	}
}
