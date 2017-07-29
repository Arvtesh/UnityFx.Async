// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A continuation specialization for <see cref="IAsyncResult"/>.
	/// </summary>
	internal class AsyncResultContinuation<T, U, V> : AsyncContinuation<T, U, V>
		where T : class, IAsyncOperation
		where U : class, IAsyncResult
	{
		#region data
		#endregion

		#region interface

		public AsyncResultContinuation(T op, Func<T, U> continuationFactory)
			: base(op, continuationFactory)
		{
		}

		#endregion

		#region AsyncContinuationBase

		protected override void OnUpdateContinuation(U continuation)
		{
			if (continuation.IsCompleted)
			{
				if (continuation is IAsyncOperation<V> op)
				{
					SetResult(op.Result);
				}
				else
				{
					SetCompleted();
				}
			}
			else if (continuation is IAsyncOperation op)
			{
				SetContinuationProgress(op.Progress);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
