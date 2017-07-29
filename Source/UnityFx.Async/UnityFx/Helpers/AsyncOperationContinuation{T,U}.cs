// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A continuation specialization for <see cref="AsyncResult"/>.
	/// </summary>
	internal class AsyncOperationContinuation<T, U> : AsyncContinuation<T, U, UnityEngine.Object>
		where T : class, IAsyncOperation
		where U : AsyncOperation
	{
		#region data
		#endregion

		#region interface

		public AsyncOperationContinuation(T op, Func<T, U> continuationFactory)
			: base(op, continuationFactory)
		{
		}

		#endregion

		#region AsyncContinuation

		protected override void OnUpdateContinuation(U continuation)
		{
			if (continuation.isDone)
			{
				SetResult(AsyncResult.GetOperationResult(continuation) as UnityEngine.Object);
			}
			else
			{
				SetContinuationProgress(continuation.progress);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
