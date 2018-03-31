// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class CatchContinuationResult : AsyncResult, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly Action<Exception> _errorCallback;
		private IAsyncOperation _op;

		#endregion

		#region interface

		public CatchContinuationResult(Action<Exception> errorCallback)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_errorCallback = errorCallback;
		}

		protected void InvokeErrorCallback(IAsyncOperation op)
		{
			_errorCallback?.Invoke(op.Exception.InnerException);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (op.IsCompletedSuccessfully)
			{
				TrySetCompleted(op.CompletedSynchronously);
			}
			else if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				InvokeErrorCallback(op, op.CompletedSynchronously);
			}
			else
			{
				_op = op;

				if (_postCallback == null)
				{
					_postCallback = args =>
					{
						var c = args as CatchContinuationResult;
						c.InvokeErrorCallback(c._op, false);
					};
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		#endregion

		#region implementation

		private void InvokeErrorCallback(IAsyncOperation op, bool completedSynchronously)
		{
			try
			{
				_errorCallback.Invoke(op.Exception.InnerException);
				TrySetCompleted(completedSynchronously);
			}
			catch (Exception e)
			{
				TrySetException(e, completedSynchronously);
			}
		}

		#endregion
	}
}
