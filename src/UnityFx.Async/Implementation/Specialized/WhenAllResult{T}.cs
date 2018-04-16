// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityFx.Async
{
	internal class WhenAllResult<T> : AsyncResult<T[]>, IAsyncContinuation
	{
		#region data

		private readonly IAsyncOperation[] _ops;

		private int _count;
		private bool _completedSynchronously;

		#endregion

		#region interface

		public WhenAllResult(IAsyncOperation[] ops)
			: base(AsyncOperationStatus.Running)
		{
			_ops = ops;
			_count = ops.Length;
			_completedSynchronously = true;

			foreach (var op in ops)
			{
				if (!op.TryAddContinuation(this))
				{
					Invoke(op);
				}
			}

			_completedSynchronously = false;
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			foreach (var op in _ops)
			{
				if (op is IAsyncCancellable c)
				{
					c.Cancel();
				}
			}
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation asyncOp)
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
