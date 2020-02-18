// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityFx.Async.Promises
{
	internal sealed class ThenSequenceResult<T> : ThenResult<T, VoidResult>
	{
		#region data
		#endregion

		#region interface

		public ThenSequenceResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
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
				case Func<Func<IAsyncOperation>[]> f1:
					result = Promise.Sequence(f1());
					break;

				case Func<IEnumerable<Func<IAsyncOperation>>> f2:
					result = Promise.Sequence(f2());
					break;

				case Func<T, Func<IAsyncOperation>[]> f3:
					result = Promise.Sequence(f3(((IAsyncOperation<T>)op).Result));
					break;

				case Func<T, IEnumerable<Func<IAsyncOperation>>> f4:
					result = Promise.Sequence(f4(((IAsyncOperation<T>)op).Result));
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
