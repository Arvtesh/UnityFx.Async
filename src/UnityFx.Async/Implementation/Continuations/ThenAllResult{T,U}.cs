// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityFx.Async
{
	internal class ThenAllResult<T, U> : ThenResult<T, U[]>
	{
		#region data

		private IAsyncOperation _op2;

		#endregion

		#region interface

		public ThenAllResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
			: base(op, successCallback, errorCallback)
		{
		}

		#endregion

		#region ThenAnyResult

		protected override void InvokeSuccessCallback(IAsyncOperation op, bool completedSynchronously, object continuation)
		{
			switch (continuation)
			{
				case Func<IEnumerable<IAsyncOperation<U>>> f1:
					_op2 = new WhenAllResult<U>(f1().ToArray());
					break;

				case Func<IEnumerable<IAsyncOperation>> f2:
					_op2 = new WhenAllResult<U>(f2().ToArray());
					break;

				case Func<T, IEnumerable<IAsyncOperation<U>>> f3:
					_op2 = new WhenAllResult<U>(f3((op as IAsyncOperation<T>).Result).ToArray());
					break;

				case Func<T, IEnumerable<IAsyncOperation>> f4:
					_op2 = new WhenAllResult<U>(f4((op as IAsyncOperation<T>).Result).ToArray());
					break;
			}

			if (_op2 != null)
			{
				_op2.AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
			}
			else
			{
				TrySetCanceled(completedSynchronously);
			}

		}

		#endregion

		#region implementation
		#endregion
	}
}
