// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

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
					result = WhenAll(f1());
					break;

				case Func<IEnumerable<IAsyncOperation>> f2:
					result = WhenAll(f2());
					break;

				case Func<T, IEnumerable<IAsyncOperation<U>>> f3:
					result = WhenAll(f3(((IAsyncOperation<T>)op).Result));
					break;

				case Func<T, IEnumerable<IAsyncOperation>> f4:
					result = WhenAll(f4(((IAsyncOperation<T>)op).Result));
					break;
			}

			if (result != null)
			{
				result.AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
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
