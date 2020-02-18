// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	internal class WhenAnyResult<T> : AsyncResult<T> where T : IAsyncOperation
	{
		#region data

		private readonly IList<T> _ops;

		#endregion

		#region interface

		public WhenAnyResult(IList<T> ops)
			: base(AsyncOperationStatus.Running)
		{
			_ops = ops;

			for (var i = 0; i < ops.Count; i++)
			{
				ops[i].AddCompletionCallback(this, null);
			}
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			for (var i = 0; i < _ops.Count; i++)
			{
				_ops[i].Cancel();
			}
		}

		#endregion

		#region IAsyncContinuation

		public override void Invoke(IAsyncOperation op)
		{
			TrySetResult((T)op);
		}

		#endregion
	}
}
