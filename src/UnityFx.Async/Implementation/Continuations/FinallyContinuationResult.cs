// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class FinallyContinuationResult : AsyncResult, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly Action _continuation;
		private IAsyncOperation _op;

		#endregion

		#region interface

		public FinallyContinuationResult(Action action)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_continuation = action;
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				try
				{
					_continuation();
					TrySetCompleted(op.CompletedSynchronously);
				}
				catch (Exception e)
				{
					TrySetException(e, op.CompletedSynchronously);
				}
			}
			else
			{
				_op = op;

				if (_postCallback == null)
				{
					_postCallback = args =>
					{
						var c = args as FinallyContinuationResult;

						try
						{
							c._continuation();
							c.TrySetCompleted(false);
						}
						catch (Exception e)
						{
							TrySetException(e, false);
						}
					};
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		#endregion
	}
}
