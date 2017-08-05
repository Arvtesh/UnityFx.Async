// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Controller of a <see cref="IAsyncOperation"/> instance.
	/// </summary>
	public interface IAsyncOperationController
	{
		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		void SetCanceled();

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		bool TrySetCanceled();

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		void SetException(Exception e);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		bool TrySetException(Exception e);

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		void SetCompleted();

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		bool TrySetCompleted();

		/// <summary>
		/// Sets the operation progress value.
		/// </summary>
		void SetProgress(float progress);
	}
}
