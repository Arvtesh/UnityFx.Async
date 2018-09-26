// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.CompilerServices
{
#if !NET35

	/// <summary>
	/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation{TResult}"/>.
	/// This type is intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public struct AsyncAwaitable<TResult>
	{
		private readonly AsyncAwaiter<TResult> _awaiter;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncAwaitable{TResult}"/> struct.
		/// </summary>
		public AsyncAwaitable(IAsyncOperation<TResult> op, AsyncCallbackOptions options)
		{
			_awaiter = new AsyncAwaiter<TResult>(op, options);
		}

		/// <summary>
		/// Returns the awaiter.
		/// </summary>
		public AsyncAwaiter<TResult> GetAwaiter() => _awaiter;
	}

#endif
}
