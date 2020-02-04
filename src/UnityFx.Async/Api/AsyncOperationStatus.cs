// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace UnityFx.Async
{
	/// <summary>
	/// Enumerates possible status values for <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskstatus">TaskStatus</seealso>
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
}
