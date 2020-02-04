// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies callback options.
	/// </summary>
	/// <seealso cref="AsyncContinuationOptions"/>
	[Flags]
	public enum AsyncCallbackOptions
	{
		/// <summary>
		/// When no options specified, default behavior should be used when executing callbacks.
		/// I.e. a callback is executed on the same thread that scheduled it.
		/// </summary>
		None,

		/// <summary>
		/// Specifies that the callback should be executed synchronously. With this option specified, the callback runs on
		/// the same thread that causes the antecedent operation to transition into its final state.
		/// </summary>
		ExecuteSynchronously = 1,

		/// <summary>
		/// Specifies that the callback should be executed on the default thread. Please see
		/// <see cref="AsyncResult.DefaultSynchronizationContext"/> for more information.
		/// </summary>
		ExecuteOnDefaultContext = 2,
	}
}
