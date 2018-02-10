// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
#if !NET35

	internal class AsyncObservableSubscription : IDisposable
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly AsyncOperationCallback _completionCallback;

		#endregion

		#region interface

		public AsyncObservableSubscription(IAsyncOperation op, AsyncOperationCallback d)
		{
			_op = op;
			_completionCallback = d;
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			_op.RemoveCompletionCallback(_completionCallback);
		}

		#endregion
	}

#endif
}
