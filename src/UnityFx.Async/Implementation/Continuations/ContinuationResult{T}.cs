// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal abstract class ContinuationResult<T> : AsyncResult<T>, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly AsyncContinuationOptions _options;
		private IAsyncOperation _op;

		#endregion

		#region interface

		protected ContinuationResult(AsyncContinuationOptions options)
			: base(AsyncOperationStatus.Running)
		{
			if ((options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0)
			{
				_syncContext = SynchronizationContext.Current;
			}

			_options = options;
		}

		protected abstract T OnInvoke(IAsyncOperation op);

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (AsyncContinuation.CanInvoke(op, _options))
			{
				if (_syncContext == null || _syncContext == SynchronizationContext.Current)
				{
					try
					{
						TrySetResult(OnInvoke(op), op.CompletedSynchronously);
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
							var c = args as ContinuationResult<T>;

							try
							{
								c.TrySetResult(c.OnInvoke(c._op), false);
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
			else
			{
				TrySetCanceled(op.CompletedSynchronously);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
