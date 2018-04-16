// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal abstract class ContinuationResult<T> : AsyncResult<T>
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly IAsyncOperation _op;

		#endregion

		#region interface

		protected ContinuationResult(IAsyncOperation op)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_op = op;
		}

		protected ContinuationResult(IAsyncOperation op, bool captureSynchronizationContext)
			: base(AsyncOperationStatus.Running)
		{
			if (captureSynchronizationContext)
			{
				_syncContext = SynchronizationContext.Current;
			}

			_op = op;
		}

		protected void InvokeOnSyncContext(IAsyncOperation op, bool completedSynchronously)
		{
			Debug.Assert(op == _op);

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

		#region AsyncResult

		protected override void OnCancel()
		{
			if (_op is IAsyncCancellable c)
			{
				c.Cancel();
			}
		}

		#endregion
	}
}
