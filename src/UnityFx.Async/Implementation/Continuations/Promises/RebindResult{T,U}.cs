// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async.Promises
{
	internal sealed class RebindResult<T, U> : AsyncResult<U>, IAsyncContinuation
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

			op.AddContinuation(this);
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

		public void Invoke(IAsyncOperation op, bool inline)
		{
			try
			{
				if (op.IsCompletedSuccessfully)
				{
					switch (_continuation)
					{
						case Func<U> f1:
							TrySetResult(f1(), inline);
							break;

						case Func<T, U> f2:
							TrySetResult(f2((op as IAsyncOperation<T>).Result), inline);
							break;

						default:
							TrySetCanceled(inline);
							break;
					}
				}
				else
				{
					TrySetException(op.Exception, inline);
				}
			}
			catch (Exception e)
			{
				TrySetException(e, inline);
			}
		}

		#endregion
	}
}
