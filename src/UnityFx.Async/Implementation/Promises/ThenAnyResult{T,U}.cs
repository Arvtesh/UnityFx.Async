// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async.Promises
{
	internal sealed class ThenAnyResult<T, U> : ThenResult<T, U>
	{
		#region data
		#endregion

		#region interface

		public ThenAnyResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
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
					result = WhenAny(f1());
					break;

				case Func<IEnumerable<IAsyncOperation>> f2:
					result = WhenAny(f2());
					break;

				case Func<T, IEnumerable<IAsyncOperation<U>>> f3:
					result = WhenAny(f3(((IAsyncOperation<T>)op).Result));
					break;

				case Func<T, IEnumerable<IAsyncOperation>> f4:
					result = WhenAny(f4(((IAsyncOperation<T>)op).Result));
					break;
			}

			if (result != null)
			{
				result.AddCompletionCallback(
					new Action<IAsyncOperation>(op2 =>
					{
						if (IsCancellationRequested)
						{
							TrySetCanceled();
						}
						else if (op2.IsCompletedSuccessfully)
						{
							var op3 = (op2 as IAsyncOperation<IAsyncOperation>).Result;

							if (op3 is IAsyncOperation<U> op4)
							{
								TrySetResult(op4.Result);
							}
							else
							{
								TrySetCompleted();
							}
						}
						else
						{
							TrySetException(op2.Exception);
						}
					}),
					null);
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
