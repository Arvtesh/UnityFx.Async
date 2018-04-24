// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A generic non-delegate continuation.
	/// </summary>
	/// <remarks>
	/// This interface allows us to combine functionality and reduce allocations. It especially useful for implementing custom
	/// continuation operations.
	/// </remarks>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="AsyncOperationCallback"/>
	/// <seealso cref="AsyncContinuationOptions"/>
	public interface IAsyncContinuation
	{
		/// <summary>
		/// Starts the continuation.
		/// </summary>
		/// <param name="op">The completed antecedent operation.</param>
		/// <param name="inline">Inline call flag: <see langword="true"/> means the continuation was called without actually being
		/// added to the continuation list (the operation was already completed at the time).</param>
		void Invoke(IAsyncOperation op, bool inline);
	}
}
