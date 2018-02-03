// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Enumerates possible status values for <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public enum AsyncOperationStatus
	{
		/// <summary>
		/// The operation is initialized but has not yet been scheduled for execution.
		/// </summary>
		Created = AsyncResult.StatusCreated,

		/// <summary>
		/// The operation has been scheduled for execution but has not yet begun executing.
		/// </summary>
		Scheduled = AsyncResult.StatusScheduled,

		/// <summary>
		/// The operation is running but has not yet completed.
		/// </summary>
		Running = AsyncResult.StatusRunning,

		/// <summary>
		/// The operation completed execution successfully.
		/// </summary>
		RanToCompletion = AsyncResult.StatusRanToCompletion,

		/// <summary>
		/// The operation has been canceled.
		/// </summary>
		Canceled = AsyncResult.StatusCanceled,

		/// <summary>
		/// The operation completed due to an unhandled exception.
		/// </summary>
		Faulted = AsyncResult.StatusFaulted
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
	public interface IAsyncOperation : IAsyncOperationEvents, IAsyncResult, IDisposable
	{
		/// <summary>
		/// Returns the operation status identifier. Read only.
		/// </summary>
		/// <value>The operation status identifier.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		AsyncOperationStatus Status { get; }

		/// <summary>
		/// Returns an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <see langword="null"/>. Read only.
		/// </summary>
		/// <value>An exception that caused the operation to end prematurely.</value>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="Status"/>
		Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has completed successfully, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has finished successfully.</value>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		/// <seealso cref="Status"/>
		bool IsCompletedSuccessfully { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has failed for any reason, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has failed.</value>
		/// <seealso cref="Exception"/>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsCanceled"/>
		/// <seealso cref="Status"/>
		bool IsFaulted { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the operation has been canceled by user, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the operation has been canceled by user.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="Status"/>
		bool IsCanceled { get; }
	}
}
