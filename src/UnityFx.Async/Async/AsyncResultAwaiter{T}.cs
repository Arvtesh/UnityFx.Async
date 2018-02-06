// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace UnityFx.Async
{
	/// <summary>
	/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation{T}"/>
	public struct AsyncResultAwaiter<T> : INotifyCompletion
	{
		#region data

		private readonly IAsyncOperation<T> _op;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultAwaiter{T}"/> struct.
		/// </summary>
		public AsyncResultAwaiter(IAsyncOperation<T> op) => _op = op;

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
		/// <returns>Returns the underlying operation result.</returns>
		public T GetResult() => _op.Result;

		#endregion

		#region INotifyCompletion

		/// <inheritdoc/>
		public void OnCompleted(Action continuation) => _op.AddOrInvokeCompletionCallback(continuation);

		#endregion
	}
}
