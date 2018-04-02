// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class RebindResult<T, U> : AsyncResult<U>, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly object _continuation;
		private IAsyncOperation _op;

		#endregion

		#region interface

		public RebindResult(object action)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_continuation = action;
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (op.IsCompletedSuccessfully)
			{
				if (_syncContext == null || _syncContext == SynchronizationContext.Current)
				{
					InvokeCallback(op, op.CompletedSynchronously);
				}
				else
				{
					_op = op;

					if (_postCallback == null)
					{
						_postCallback = args =>
						{
							var c = args as RebindResult<T, U>;
							c.InvokeCallback(c._op, false);
						};
					}

					_syncContext.Post(_postCallback, this);
				}
			}
			else
			{
				TrySetException(op.Exception, op.CompletedSynchronously);
			}
		}

		#endregion

		#region implementation

		private void InvokeCallback(IAsyncOperation op, bool completedSynchronously)
		{
			try
			{
				switch (_continuation)
				{
					case Func<U> f1:
						TrySetResult(f1(), completedSynchronously);
						break;

					case Func<T, U> f2:
						TrySetResult(f2((op as IAsyncOperation<T>).Result), completedSynchronously);
						break;

					default:
						TrySetCanceled(completedSynchronously);
						break;
				}
			}
			catch (Exception e)
			{
				TrySetException(e, completedSynchronously);
			}
		}

		#endregion
	}
}
