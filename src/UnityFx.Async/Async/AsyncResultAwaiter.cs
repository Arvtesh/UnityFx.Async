// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
	/// </summary>
	/// <seealso cref="AsyncResult"/>
	public struct AsyncResultAwaiter : IAsyncAwaiter
	{
		#region data

		private readonly IAsyncResult _op;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultAwaiter"/> struct.
		/// </summary>
		public AsyncResultAwaiter(IAsyncResult op)
		{
			_op = op;
		}

		#endregion

		#region IAwaiter

		/// <summary>
		/// Returns <c>true</c> if the source awaitable is completed; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsCompleted => _op.IsCompleted;

		/// <summary>
		/// Returns the source result value.
		/// </summary>
		public void GetResult()
		{
			// does nothing
		}

		#endregion

		#region INotifyCompletion

		/// <summary>
		/// Schedules the continuation action that's invoked when the operation completes.
		/// </summary>
		public void OnCompleted(Action continuation)
		{
			if (_op is IAsyncContinuationController c)
			{
				c.AddCompletionCallback(continuation);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		#endregion
	}
}
