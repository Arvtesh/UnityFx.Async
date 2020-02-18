// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async.CompilerServices
{
#if !NET35

	/// <summary>
	/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation"/>.
	/// This type is intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public struct AsyncAwaitable
	{
		private readonly AsyncAwaiter _awaiter;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncAwaitable"/> struct.
		/// </summary>
		public AsyncAwaitable(IAsyncOperation op, SynchronizationContext syncContext)
		{
			_awaiter = new AsyncAwaiter(op, syncContext);
		}

		/// <summary>
		/// Returns the awaiter.
		/// </summary>
		public AsyncAwaiter GetAwaiter() => _awaiter;
	}

#endif
}
