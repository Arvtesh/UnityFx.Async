// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

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
		public abstract bool TrySetCanceled();

		/// <inheritdoc/>
		public abstract bool TrySetCompleted();

		/// <inheritdoc/>
		public abstract bool TrySetException(Exception exception);

		/// <inheritdoc/>
		public abstract bool TrySetExceptions(IEnumerable<Exception> exceptions);

		#endregion
	}
}
