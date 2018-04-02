// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal abstract class PromiseResult<T> : AsyncResult<T>, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly IAsyncOperation _op;

		#endregion

		#region interface

		protected PromiseResult(IAsyncOperation op)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_op = op;
			_op.AddContinuation(this);
		}

		protected abstract void InvokeCallbacks(IAsyncOperation op, bool completedSynchronously);

		#endregion

		#region IAsyncContinuation

		public virtual void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				try
				{
					InvokeCallbacks(op, completedSynchronously);
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
						var c = args as PromiseResult<T>;

						try
						{
							c.InvokeCallbacks(c._op, false);
						}
						catch (Exception e)
						{
							c.TrySetException(e, completedSynchronously);
						}
					};
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		#endregion
	}
}
