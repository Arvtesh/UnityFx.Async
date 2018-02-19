// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;

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
				if (!op.TryAddCompletionCallback(_completionAction, null))
				{
					TrySetResult(op, true);
					break;
				}
			}
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
