// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal sealed class FinallyResult<T> : AsyncResult<T>
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly object _continuation;

		#endregion

		#region interface

		public FinallyResult(IAsyncOperation op, object action)
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
				switch (_continuation)
				{
					case Action a:
						{
							a.Invoke();
							TrySetCompleted();
							break;
						}

					case Func<IAsyncOperation<T>> f1:
						{
							f1().AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
							break;
						}

					case Func<IAsyncOperation> f2:
						{
							f2().AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
							break;
						}

					default:
						{
							TrySetCanceled();
							break;
						}
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
