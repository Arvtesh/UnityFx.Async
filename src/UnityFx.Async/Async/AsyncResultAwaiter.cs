// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace UnityFx.Async
{
	/// <summary>
	/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public struct AsyncResultAwaiter : INotifyCompletion
	{
		#region data

		private readonly IAsyncOperation _op;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultAwaiter"/> struct.
		/// </summary>
		public AsyncResultAwaiter(IAsyncOperation op) => _op = op;

		#endregion

		#region IAwaiter

		/// <summary>
		/// Gets whether the underlying operation is completed.
		/// </summary>
		/// <value>The operation completion flag.</value>
		public bool IsCompleted => _op.IsCompleted;

		/// <summary>
		/// Returns the source result value.
		/// </summary>
		public void GetResult()
		{
		}

		#endregion

		#region INotifyCompletion

		/// <inheritdoc/>
		public void OnCompleted(Action continuation) => _op.AddOrInvokeCompletionCallback(continuation);

		#endregion
	}
}
