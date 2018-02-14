// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Helper class for <see cref="IAsyncCompletionSource{T}"/> implmentations.
	/// </summary>
	/// <seealso cref="AsyncCompletionSource"/>
	/// <seealso cref="AsyncResult{T}"/>
	public abstract class AsyncCompletionSource<T> : IAsyncCompletionSource<T>
	{
		#region IAsyncCompletionSource

		/// <inheritdoc/>
		public void SetCanceled()
		{
			if (!TrySetCanceled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public void SetResult(T result)
		{
			if (!TrySetResult(result))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public void SetException(Exception exception)
		{
			if (!TrySetException(exception))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public void SetException(IEnumerable<Exception> exceptions)
		{
			if (!TrySetException(exceptions))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public abstract bool TrySetCanceled();

		/// <inheritdoc/>
		public abstract bool TrySetResult(T result);

		/// <inheritdoc/>
		public abstract bool TrySetException(Exception exception);

		/// <inheritdoc/>
		public abstract bool TrySetException(IEnumerable<Exception> exceptions);

		#endregion
	}
}
