// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class UnwrapResult<T> : AsyncResult<T>, IAsyncContinuation
	{
		#region data

		private enum State
		{
			WaitingForOuterOperation,
			WaitingForInnerOperation,
			Done
		}

		private State _state;

		#endregion

		#region interface

		public UnwrapResult(IAsyncOperation outerOp)
			: base(AsyncOperationStatus.Running)
		{
			outerOp.AddContinuation(this);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (_state == State.WaitingForOuterOperation)
			{
				_state = State.WaitingForInnerOperation;

				if (op.IsCompletedSuccessfully)
				{
					switch (op)
					{
						case IAsyncOperation<IAsyncOperation<T>> innerOp1:
							ProcessInnerOperation(innerOp1.Result, false);
							break;

						case IAsyncOperation<IAsyncOperation> innerOp2:
							ProcessInnerOperation(innerOp2.Result, false);
							break;

						default:
							ProcessInnerOperation(null, false);
							break;
					}
				}
				else
				{
					TrySetException(op.Exception, false);
				}
			}
			else if (_state == State.WaitingForInnerOperation)
			{
				if (op.IsCompletedSuccessfully)
				{
					if (op is IAsyncOperation<T> innerOp)
					{
						TrySetResult(innerOp.Result, false);
					}
					else
					{
						TrySetCompleted(false);
					}
				}
				else
				{
					TrySetException(op.Exception, false);
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

		private void ProcessInnerOperation(IAsyncOperation innerOp, bool completedSynchronously)
		{
			if (innerOp == null)
			{
				TrySetCanceled(completedSynchronously);
			}
			else
			{
				innerOp.AddContinuation(this);
			}
		}

		#endregion
	}
}
