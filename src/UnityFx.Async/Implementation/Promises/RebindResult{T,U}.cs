// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal sealed class RebindResult<T, U> : AsyncResult<U>
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly object _continuation;

		#endregion

		#region interface

		public RebindResult(IAsyncOperation op, object action)
			: base(AsyncOperationStatus.Running)
		{
			_op = op;
			_continuation = action;

			op.AddCompletionCallback(this);
		}

		#endregion

		#region AsyncResult

		protected override float GetProgress()
		{
			return _op.Progress;
		}

		protected override void OnCancel()
		{
			_op.Cancel();
		}

		#endregion

		#region IAsyncContinuation

		public override void Invoke(IAsyncOperation op)
		{
			try
			{
				if (op.IsCompletedSuccessfully)
				{
					switch (_continuation)
					{
						case Func<U> f1:
							TrySetResult(f1());
							break;

						case Func<T, U> f2:
							TrySetResult(f2(((IAsyncOperation<T>)op).Result));
							break;

						default:
							TrySetCanceled();
							break;
					}
				}
				else
				{
					TrySetException(op.Exception);
				}
			}
			catch (Exception e)
			{
				TrySetException(e);
			}
		}

		#endregion
	}
}
