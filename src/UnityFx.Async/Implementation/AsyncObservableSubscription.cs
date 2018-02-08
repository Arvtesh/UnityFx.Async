// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
#if !NET35

	/// <summary>
	/// A disposable subscription for <see cref="AsyncObservable{T}"/>.
	/// </summary>
	internal class AsyncObservableSubscription : IDisposable
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly Action _completionCallback;

		#endregion

		#region interface

		public AsyncObservableSubscription(IAsyncOperation op, Action d)
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
