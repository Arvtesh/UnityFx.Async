// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Helper class for <see cref="IAsyncCompletionSource"/> implmentations.
	/// </summary>
	/// <seealso cref="AsyncCompletionSource{T}"/>
	/// <seealso cref="AsyncResult"/>
	public abstract class AsyncCompletionSource : IAsyncCompletionSource
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
		public void SetCompleted()
		{
			if (!TrySetCompleted())
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public void SetException(Exception e)
		{
			if (!TrySetException(e))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public abstract bool TrySetCanceled();

		/// <inheritdoc/>
		public abstract bool TrySetCompleted();

		/// <inheritdoc/>
		public abstract bool TrySetException(Exception e);

		#endregion
	}
}
