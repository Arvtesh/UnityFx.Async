// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Controls completion of a <see cref="IAsyncOperation{T}"/> instance.
	/// </summary>
	/// <seealso cref="IAsyncOperation{T}"/>
	public interface IAsyncCompletionSource<in T> : IAsyncCompletionSource
	{
		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">A synchronous completion flag. Set to <see langword="false"/> if not sure.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetResult(T, bool)"/>
		void SetResult(T result, bool completedSynchronously);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">A synchronous completion flag. Set to <see langword="false"/> if not sure.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetResult(T, bool)"/>
		bool TrySetResult(T result, bool completedSynchronously);
	}
}
