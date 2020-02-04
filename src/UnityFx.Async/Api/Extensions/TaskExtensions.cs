// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Async.Extensions
{
#if !NET35

	/// <summary>
	/// Extension methods for <see cref="Task"/> and <see cref="Task{TResult}"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class TaskExtensions
	{
		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The task to convert to <see cref="IAsyncOperation"/>.</param>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the <paramref name="task"/>.</returns>
		public static AsyncResult ToAsync(this Task task)
		{
			return AsyncResult.FromTask(task);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The task to convert to <see cref="IAsyncOperation"/>.</param>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the <paramref name="task"/>.</returns>
		public static AsyncResult<TResult> ToAsync<TResult>(this Task<TResult> task)
		{
			return AsyncResult.FromTask(task);
		}
	}

#endif
}
