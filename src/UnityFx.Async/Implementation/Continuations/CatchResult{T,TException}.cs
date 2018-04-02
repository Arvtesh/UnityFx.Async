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

		public CatchResult(IAsyncOperation op, Action<TException> errorCallback)
			: base(op)
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

		public override void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (op.IsCompletedSuccessfully || !(op.Exception.InnerException is TException))
			{
				TrySetCompleted(completedSynchronously);
			}
			else
			{
				base.Invoke(op, completedSynchronously);
			}
		}

		#endregion
	}
}
