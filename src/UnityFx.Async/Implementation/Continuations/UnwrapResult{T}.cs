// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class UnwrapResult<T> : AsyncResult<T>, IAsyncContinuation
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
		{
			outerOp.AddContinuation(this);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (_state == State.WaitingForOuterOperation)
			{
				if (op.IsCompletedSuccessfully)
				{
					switch (op)
					{
						case IAsyncOperation<IAsyncOperation<T>> innerOp1:
							ProcessInnerOperation(innerOp1, completedSynchronously);
							break;

						case IAsyncOperation<IAsyncOperation> innerOp2:
							ProcessInnerOperation(innerOp2, completedSynchronously);
							break;

						default:
							ProcessInnerOperation(null, completedSynchronously);
							break;
					}
				}
				else
				{
					TrySetException(op.Exception, completedSynchronously);
				}

				_state = State.WaitingForInnerOperation;
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
