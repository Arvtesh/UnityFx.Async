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
		/// Gets the operation status identifier.
		/// </summary>
		/// <value>Identifier of the operation status.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		AsyncOperationStatus Status { get; }

		/// <summary>
		/// Gets an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <see langword="null"/>.
		/// </summary>
		/// <value>An exception that caused the operation to end prematurely. If there are more than one exception returns an instance of <see cref="AggregateException"/>.</value>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="Status"/>
		Exception Exception { get; }

		/// <summary>
		/// Gets whether the operation completed with successfully (i.e. with <see cref="AsyncOperationStatus.RanToCompletion"/> status).
		/// </summary>
		/// <value>A value indicating whether the operation has finished successfully.</value>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		/// <seealso cref="Status"/>
		bool IsCompletedSuccessfully { get; }

		/// <summary>
		/// Gets whether the operation completed due to an unhandled exception (i.e. with <see cref="AsyncOperationStatus.Faulted"/> status).
		/// </summary>
		/// <remarks>
		/// If <see cref="IsFaulted"/> is <see langword="true"/>, the operation's <see cref="Status"/> will be equal to
		/// <see cref="AsyncOperationStatus.Faulted"/>, and its <see cref="Exception"/> property will be non-<see langword="null"/>.
		/// </remarks>
		/// <value>A value indicating whether the operation has failed.</value>
		/// <seealso cref="Exception"/>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsCanceled"/>
		/// <seealso cref="Status"/>
		bool IsFaulted { get; }

		/// <summary>
		/// Gets whether the operation completed due to being canceled (i.e. with <see cref="AsyncOperationStatus.Canceled"/> status).
		/// </summary>
		/// <value>A value indicating whether the operation was canceled.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="Status"/>
		bool IsCanceled { get; }
	}
}
