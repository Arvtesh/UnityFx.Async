// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies the synchronization context to schedule continuation on.
	/// </summary>
	/// <seealso cref="AsyncContinuationOptions"/>
	public enum AsyncContinuationContext
	{
		/// <summary>
		/// Specifies that the continuation should be executed synchronously. With this option specified,
		/// the continuation runs on the same thread that causes the antecedent operation to transition
		/// into its final state.
		/// </summary>
		None,

		/// <summary>
		/// Specifies that the continuation should be executed on the same thread that scheduled it.
		/// That is the default behaviour of all library continuations.
		/// </summary>
		Current,

		/// <summary>
		/// Specifies that the continuation should be executed on the default thread. See
		/// <see cref="AsyncResult.DefaultSynchronizationContext"/> for more information.
		/// </summary>
		Default
	}
}
