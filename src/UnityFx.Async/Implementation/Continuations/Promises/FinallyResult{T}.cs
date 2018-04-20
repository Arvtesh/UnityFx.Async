// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal sealed class FinallyResult<T> : ContinuationResult<T>, IAsyncContinuation
	{
		#region data

		private readonly object _continuation;

		#endregion

		#region interface

		public FinallyResult(IAsyncOperation op, object action)
			: base(op)
		{
			_continuation = action;

			// NOTE: Cannot move this to base class because this call might trigger virtual Invoke
			if (!op.TryAddContinuation(this))
			{
				InvokeOnSyncContext(op, true);
			}
		}

		#endregion

		#region ContinuationResult

		protected override void InvokeInline(IAsyncOperation op, bool completedSynchronously)
		{
			switch (_continuation)
			{
				case Action a:
					a.Invoke();
					TrySetCompleted(completedSynchronously);
					break;

				case Func<IAsyncOperation<T>> f1:
					f1().AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<IAsyncOperation> f2:
					f2().AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
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
	}
}
