// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents the consumer side of an asynchronous operation (promise).
	/// </summary>
	/// <remarks>
	/// The interface defines a read-only consumer side of an asynchronous operation. Basically
	/// it provides the operation state information and completion/progress events. It is cancellable
	/// so a cancellation request can be issued at any time (without any guarantees though).
	/// </remarks>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Task</seealso>
	/// <seealso cref="IAsyncCompletionSource"/>
	/// <seealso cref="IAsyncOperation{TResult}"/>
	/// <seealso cref="AsyncResult"/>
	public interface IAsyncOperation : IAsyncOperationCallbacks, IAsyncCancellable, IAsyncResult, IDisposable
	{
		/// <summary>
		/// Gets a unique ID for the operation instance.
		/// </summary>
		/// <remarks>
		/// The identifiers might be assigned on demand and do not represent the order in which operations instances were created.
		/// </remarks>
		/// <value>Unique non-zero identifier of the operation instance.</value>
		int Id { get; }

		/// <summary>
		/// Gets the operation progress [0, 1].
		/// </summary>
		/// <remarks>
		/// Different operation implementations might provide different progress resolution. Users of this
		/// interface can expect 0 value until the operation is started and 1 when it is completed as minimum.
		/// </remarks>
		/// <value>Progress of the operation in range [0, 1].</value>
		/// <seealso cref="Status"/>
		float Progress { get; }

		/// <summary>
		/// Gets the operation status identifier.
		/// </summary>
		/// <value>Identifier of the operation status.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		AsyncOperationStatus Status { get; }

		/// <summary>
		/// Gets an exception that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <see langword="null"/>.
		/// </summary>
		/// <remarks>
		/// Task uses a special aggregate exception for providing generic error information. The library does not allow
		/// child operations by design and this fact makes usage of aggregate exceptions a very rare case. This is
		/// why we use <see cref="System.Exception"/> here.
		/// </remarks>
		/// <value>An exception that caused the operation to end prematurely.</value>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="Status"/>
		Exception Exception { get; }

		/// <summary>
		/// Gets a value indicating whether the operation completed successfully (i.e. with <see cref="AsyncOperationStatus.RanToCompletion"/> status).
		/// </summary>
		/// <value>A value indicating whether the operation completed successfully.</value>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		/// <seealso cref="Status"/>
		bool IsCompletedSuccessfully { get; }

		/// <summary>
		/// Gets a value indicating whether the operation completed due to an unhandled exception (i.e. with <see cref="AsyncOperationStatus.Faulted"/> status).
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
		/// Gets a value indicating whether the operation completed due to being canceled (i.e. with <see cref="AsyncOperationStatus.Canceled"/> status).
		/// </summary>
		/// <remarks>
		/// If <see cref="IsCanceled"/> is <see langword="true"/>, the operation's <see cref="Status"/> will be equal to
		/// <see cref="AsyncOperationStatus.Canceled"/>, and its <see cref="Exception"/> property will be non-<see langword="null"/>.
		/// </remarks>
		/// <value>A value indicating whether the operation was canceled.</value>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="Status"/>
		bool IsCanceled { get; }
	}
}
