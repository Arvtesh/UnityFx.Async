// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Controller of a <see cref="IAsyncOperation{T}"/> instance.
	/// </summary>
	public interface IAsyncOperationController<in T> : IAsyncOperationController
	{
		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.Success"/> state.
		/// </summary>
		void SetResult(T result);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.Success"/> state.
		/// </summary>
		bool TrySetResult(T result);
	}
}
