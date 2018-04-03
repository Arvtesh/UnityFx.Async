// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal abstract class ContinuationResult<T> : AsyncResult<T>
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private IAsyncOperation _op;

		#endregion

		#region interface

		protected ContinuationResult()
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
		}

		protected ContinuationResult(bool captureSynchronizationContext)
			: base(AsyncOperationStatus.Running)
		{
			if (captureSynchronizationContext)
			{
				_syncContext = SynchronizationContext.Current;
			}
		}

		protected void InvokeOnSyncContext(IAsyncOperation op, bool completedSynchronously)
		{
			if (completedSynchronously || _syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				try
				{
					InvokeUnsafe(op, completedSynchronously);
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
						var c = args as ContinuationResult<T>;

						try
						{
							c.InvokeUnsafe(c._op, false);
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

		protected abstract void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously);

		#endregion
	}
}
