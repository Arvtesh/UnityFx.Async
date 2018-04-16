// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class WhenAnyResult<T> : AsyncResult<T>, IAsyncContinuation where T : IAsyncOperation
	{
		#region data

		private readonly T[] _ops;

		#endregion

		#region interface

		public WhenAnyResult(T[] ops)
			: base(AsyncOperationStatus.Running)
		{
			_ops = ops;

			foreach (var op in ops)
			{
				if (!op.TryAddContinuation(this))
				{
					TrySetResult(op, true);
					break;
				}
			}
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			TrySetResult((T)op, false);
		}

		#endregion
	}
}
