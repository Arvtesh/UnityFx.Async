// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class UnwrapResult<T> : AsyncResult<T>
	{
		#region data

		private enum State
		{
			WaitingForOuterOperation,
			WaitingForInnerOperation,
			Done
		}

		private IAsyncOperation _op;
		private State _state;

		#endregion

		#region interface

		public UnwrapResult(IAsyncOperation outerOp)
			: base(AsyncOperationStatus.Running)
		{
			outerOp.AddCompletionCallback(this);
			_op = outerOp;
		}

		#endregion

		#region AsyncResult

		protected override float GetProgress()
		{
			if (_state == State.WaitingForOuterOperation)
			{
				return _op.Progress * 0.5f;
			}
			else if (_state == State.WaitingForInnerOperation)
			{
				return 0.5f + _op.Progress * 0.5f;
			}

			return 1;
		}

		#endregion

		#region IAsyncContinuation

		public override void Invoke(IAsyncOperation op)
		{
			if (_state == State.WaitingForOuterOperation)
			{
				_state = State.WaitingForInnerOperation;

				if (op.IsCompletedSuccessfully)
				{
					switch (op)
					{
						case IAsyncOperation<IAsyncOperation<T>> innerOp1:
							ProcessInnerOperation(innerOp1.Result);
							break;

						case IAsyncOperation<IAsyncOperation> innerOp2:
							ProcessInnerOperation(innerOp2.Result);
							break;

						default:
							ProcessInnerOperation(null);
							break;
					}
				}
				else
				{
					TrySetException(op.Exception);
				}
			}
			else if (_state == State.WaitingForInnerOperation)
			{
				if (op.IsCompletedSuccessfully)
				{
					if (op is IAsyncOperation<T> innerOp)
					{
						TrySetResult(innerOp.Result);
					}
					else
					{
						TrySetCompleted();
					}
				}
				else
				{
					TrySetException(op.Exception);
				}

				_state = State.Done;
			}
			else
			{
				// Should not get here.
			}
		}

		#endregion

		#region implementation

		private void ProcessInnerOperation(IAsyncOperation innerOp)
		{
			if (innerOp == null)
			{
				TrySetCanceled();
			}
			else
			{
				innerOp.AddCompletionCallback(this);
				_op = innerOp;
			}
		}

		#endregion
	}
}
