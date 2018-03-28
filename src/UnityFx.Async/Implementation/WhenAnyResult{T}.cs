// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class WhenAnyResult<T> : AsyncResult<T> where T : IAsyncOperation
	{
		#region data

		private readonly AsyncOperationCallback _completionAction;
		private readonly T[] _ops;

		#endregion

		#region interface

		public WhenAnyResult(T[] ops)
			: base(AsyncOperationStatus.Running)
		{
			_completionAction = OnOperationCompleted;
			_ops = ops;

			foreach (var op in ops)
			{
				if (!op.TryAddCompletionCallback(_completionAction, AsyncContinuationOptions.None, null))
				{
					TrySetResult(op, true);
					break;
				}
			}
		}

		public void Cancel()
		{
			TrySetCanceled(false);
		}

		#endregion

		#region implementation

		private void OnOperationCompleted(IAsyncOperation op)
		{
			TrySetResult((T)op, false);
		}

		#endregion
	}
}
