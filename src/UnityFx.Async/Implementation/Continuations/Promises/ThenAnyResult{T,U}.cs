// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityFx.Async.Promises
{
	internal sealed class ThenAnyResult<T, U> : ThenResult<T, U>
	{
		#region data

		private WhenAnyResult<IAsyncOperation> _op2;

		#endregion

		#region interface

		public ThenAnyResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
			: base(op, successCallback, errorCallback)
		{
		}

		#endregion

		#region ThenResult

		protected override void InvokeSuccessCallback(IAsyncOperation op, bool completedSynchronously, object continuation)
		{
			switch (continuation)
			{
				case Func<IEnumerable<IAsyncOperation<U>>> f1:
					_op2 = new WhenAnyResult<IAsyncOperation>(f1().ToArray());
					break;

				case Func<IEnumerable<IAsyncOperation>> f2:
					_op2 = new WhenAnyResult<IAsyncOperation>(f2().ToArray());
					break;

				case Func<T, IEnumerable<IAsyncOperation<U>>> f3:
					_op2 = new WhenAnyResult<IAsyncOperation>(f3((op as IAsyncOperation<T>).Result).ToArray());
					break;

				case Func<T, IEnumerable<IAsyncOperation>> f4:
					_op2 = new WhenAnyResult<IAsyncOperation>(f4((op as IAsyncOperation<T>).Result).ToArray());
					break;
			}

			if (_op2 != null)
			{
				_op2.AddCompletionCallback(
					op2 =>
					{
						if (IsCancellationRequested)
						{
							TrySetCanceled(false);
						}
						else if (op2.IsCompletedSuccessfully)
						{
							var op3 = (op2 as IAsyncOperation<IAsyncOperation>).Result;

							if (op3 is IAsyncOperation<U> op4)
							{
								TrySetResult(op4.Result, false);
							}
							else
							{
								TrySetCompleted(false);
							}
						}
						else
						{
							TrySetException(op2.Exception, false);
						}
					},
					null);
			}
			else
			{
				TrySetCanceled(completedSynchronously);
			}
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			base.OnCancel();
			_op2?.Cancel();
		}

		#endregion

		#region implementation
		#endregion
	}
}
