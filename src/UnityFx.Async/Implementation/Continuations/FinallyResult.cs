// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class FinallyResult : AsyncResult, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly Action _continuation;
		private IAsyncOperation _op;

		#endregion

		#region interface

		public FinallyResult(Action action)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_continuation = action;
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				try
				{
					_continuation();
					TrySetCompleted(completedSynchronously);
				}
				catch (Exception e)
				{
					TrySetException(e, completedSynchronously);
				}
			}
			else
			{
				_op = op;

				if (_postCallback == null)
				{
					_postCallback = args =>
					{
						var c = args as FinallyResult;

						try
						{
							c._continuation();
							c.TrySetCompleted(false);
						}
						catch (Exception e)
						{
							c.TrySetException(e, false);
						}
					};
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		#endregion
	}
}
