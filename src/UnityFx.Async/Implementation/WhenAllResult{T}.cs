// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityFx.Async
{
	internal class WhenAllResult<T> : AsyncResult<T[]>
	{
		#region data

		private readonly AsyncOperationCallback _completionAction;
		private readonly IAsyncOperation[] _ops;

		private int _count;
		private bool _completedSynchronously;

		#endregion

		#region interface

		public WhenAllResult(IAsyncOperation[] ops)
			: base(AsyncOperationStatus.Running)
		{
			_completionAction = OnOperationCompleted;
			_ops = ops;
			_count = ops.Length;
			_completedSynchronously = true;

			foreach (var op in ops)
			{
				if (!op.TryAddCompletionCallback(_completionAction, null))
				{
					OnOperationCompleted(op);
				}
			}

			_completedSynchronously = false;
		}

		public void Cancel()
		{
			TrySetCanceled(false);
		}

		#endregion

		#region implementation

		private void OnOperationCompleted(IAsyncOperation asyncOp)
		{
			if (IsCompleted)
			{
				return;
			}

			if (Interlocked.Decrement(ref _count) == 0)
			{
				List<Exception> exceptions = null;
				IAsyncOperation canceledOp = null;

				foreach (var op in _ops)
				{
					if (op.IsFaulted)
					{
						if (exceptions == null)
						{
							exceptions = new List<Exception>() { op.Exception };
						}
					}
					else if (op.IsCanceled)
					{
						canceledOp = op;
					}
				}

				if (exceptions != null)
				{
					TrySetExceptions(exceptions, _completedSynchronously);
				}
				else if (canceledOp != null)
				{
					TrySetCanceled(_completedSynchronously);
				}
				else if (typeof(T) == typeof(VoidResult))
				{
					TrySetCompleted(_completedSynchronously);
				}
				else
				{
					var results = new List<T>(_ops.Length);

					foreach (var op in _ops)
					{
						if (op is IAsyncOperation<T> rop)
						{
							results.Add(rop.Result);
						}
					}

					TrySetResult(results.ToArray(), false);
				}
			}
		}

		#endregion
	}
}
