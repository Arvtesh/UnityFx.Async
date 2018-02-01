// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Enumerates possible status values used by <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public enum AsyncOperationStatus
	{
		/// <summary>
		/// The operation is initialized but has not yet been scheduled for execution.
		/// </summary>
		Created = AsyncResult.StatusInitialized,

		/// <summary>
		/// The operation execution has started.
		/// </summary>
		Running = AsyncResult.StatusRunning,

		/// <summary>
		/// The operation has completed successfully.
		/// </summary>
		RanToCompletion = AsyncResult.StatusCompleted,

		/// <summary>
		/// The operation failed.
		/// </summary>
		Faulted = AsyncResult.StatusFaulted,

		/// <summary>
		/// The operation has been canceled.
		/// </summary>
		Canceled = AsyncResult.StatusCanceled,
	}

	/// <summary>
	/// A disposable <see cref="IAsyncResult"/> with status information.
	/// </summary>
	/// <remarks>
	/// The interface is designed to be as close to the TLP Task as possible. All interface methods are expected to be thread-safe.
	/// The only exception to this rule is <see cref="IDisposable.Dispose"/>.
	/// </remarks>
	/// <seealso cref="IAsyncResult"/>
	/// <seealso cref="IAsyncOperation{T}"/>
	public interface IAsyncOperation : IAsyncResult, IDisposable
	{
		/// <summary>
		/// Returns the operation progress in range [0,1]. Read only.
		/// </summary>
		float Progress { get; }

		/// <summary>
		/// Returns the operation status identifier. Read only.
		/// </summary>
		AsyncOperationStatus Status { get; }

		/// <summary>
		/// Returns an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <c>null</c>. Read only.
		/// </summary>
		Exception Exception { get; }

		/// <summary>
		/// Returns <c>true</c> if the operation has completed successfully, <c>false</c> otherwise. Read only.
		/// </summary>
		bool IsCompletedSuccessfully { get; }

		/// <summary>
		/// Returns <c>true</c> if the operation has failed for any reason, <c>false</c> otherwise. Read only.
		/// </summary>
		bool IsFaulted { get; }

		/// <summary>
		/// Returns <c>true</c> if the operation has been canceled by user, <c>false</c> otherwise. Read only.
		/// </summary>
		bool IsCanceled { get; }
	}
}
