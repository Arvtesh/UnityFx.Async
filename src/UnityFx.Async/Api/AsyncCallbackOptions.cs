// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies the behavior for an operation callbacks.
	/// </summary>
	public enum AsyncCallbackOptions
	{
		/// <summary>
		/// Specifies that the continuation should be executed on the same thread that scheduled it.
		/// That is the default behaviour.
		/// </summary>
		ExecuteOnCapturedContext,

		/// <summary>
		/// Specifies that the continuation should be executed on the default thread. Please see
		/// <see cref="AsyncResult.DefaultSynchronizationContext"/> for more information.
		/// </summary>
		ExecuteOnDefaultContext,

		/// <summary>
		/// Specifies that the continuation should be executed synchronously. With this option specified,
		/// the continuation runs on the same thread that causes the antecedent operation to transition
		/// into its final state.
		/// </summary>
		ExecuteSynchronously
	}
}
