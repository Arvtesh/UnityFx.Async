// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class CatchResult<T, TException> : PromiseResult<T>, IAsyncContinuation where TException : Exception
	{
		#region data

		private readonly Action<TException> _errorCallback;

		#endregion

		#region interface

		public CatchResult(Action<TException> errorCallback)
		{
			_errorCallback = errorCallback;
		}

		#endregion

		#region PromiseResult

		protected override void InvokeCallbacks(IAsyncOperation op, bool completedSynchronously)
		{
			_errorCallback.Invoke(op.Exception.InnerException as TException);
			TrySetCompleted(completedSynchronously);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully || !(op.Exception.InnerException is TException))
			{
				TrySetCompleted(completedSynchronously);
			}
			else
			{
				InvokeOnSyncContext(op, completedSynchronously);
			}
		}

		#endregion
	}
}
