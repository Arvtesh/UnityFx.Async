// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityFx.Async.Promises
{
	internal sealed class ThenAllResult<T, U> : ThenResult<T, U[]>
	{
		#region data
		#endregion

		#region interface

		public ThenAllResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
			: base(op, successCallback, errorCallback)
		{
		}

		#endregion

		#region ThenResult

		protected override IAsyncOperation InvokeSuccessCallback(IAsyncOperation op, object continuation)
		{
			IAsyncOperation result = null;

			switch (continuation)
			{
				case Func<IEnumerable<IAsyncOperation<U>>> f1:
					result = new WhenAllResult<U>(f1().ToArray());
					break;

				case Func<IEnumerable<IAsyncOperation>> f2:
					result = new WhenAllResult<U>(f2().ToArray());
					break;

				case Func<T, IEnumerable<IAsyncOperation<U>>> f3:
					result = new WhenAllResult<U>(f3((op as IAsyncOperation<T>).Result).ToArray());
					break;

				case Func<T, IEnumerable<IAsyncOperation>> f4:
					result = new WhenAllResult<U>(f4((op as IAsyncOperation<T>).Result).ToArray());
					break;
			}

			if (result != null)
			{
				result.AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
			}
			else
			{
				TrySetCanceled();
			}

			return result;
		}

		#endregion

		#region implementation
		#endregion
	}
}
