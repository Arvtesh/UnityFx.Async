// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Controls completion of a <see cref="IAsyncOperation"/> instance.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncCompletionSource
	{
		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetCanceled"/>
		void SetCanceled();

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetCanceled"/>
		bool TrySetCanceled();

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="e">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="e"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetException(Exception)"/>
		void SetException(Exception e);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="e">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="e"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetException(Exception)"/>
		bool TrySetException(Exception e);

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetCompleted"/>
		void SetCompleted();

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetCompleted"/>
		bool TrySetCompleted();
	}
}
