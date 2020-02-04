// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A generic non-delegate continuation.
	/// </summary>
	/// <remarks>
	/// This interface is a great helper for combining functionality and reducing number of allocations.
	/// It is especially useful for implementing custom continuation operations.
	/// </remarks>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="AsyncContinuationOptions"/>
	public interface IAsyncContinuation
	{
		/// <summary>
		/// Invokes the continuation.
		/// </summary>
		/// <param name="op">The completed antecedent operation.</param>
		void Invoke(IAsyncOperation op);
	}
}
