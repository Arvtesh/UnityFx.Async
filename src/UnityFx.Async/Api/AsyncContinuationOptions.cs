// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies the behavior of an asynchronous operation continuation.
	/// </summary>
	/// <seealso cref="AsyncCallbackOptions"/>
	[Flags]
	public enum AsyncContinuationOptions
	{
		/// <summary>
		/// When no continuation options are specified, default behavior should be used when executing a continuation.
		/// I.e. continuation is executed on the same thread that scheduled it.
		/// </summary>
		None = 0,

		/// <summary>
		/// Specifies that the continuation should not be scheduled if its antecedent ran to completion.
		/// </summary>
		NotOnRanToCompletion = 1,

		/// <summary>
		/// Specifies that the continuation should not be scheduled if its antecedent threw an unhandled exception.
		/// </summary>
		NotOnFaulted = 2,

		/// <summary>
		/// Specifies that the continuation should not be scheduled if its antecedent was canceled.
		/// </summary>
		NotOnCanceled = 4,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent ran to completion.
		/// </summary>
		OnlyOnRanToCompletion = NotOnFaulted | NotOnCanceled,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent threw an unhandled exception.
		/// </summary>
		OnlyOnFaulted = NotOnRanToCompletion | NotOnCanceled,

		/// <summary>
		/// Specifies that the continuation should be scheduled only if its antecedent was canceled.
		/// </summary>
		OnlyOnCanceled = NotOnRanToCompletion | NotOnFaulted,

		/// <summary>
		/// Specifies that the continuation should be executed synchronously. With this option specified, the continuation runs on
		/// the same thread that causes the antecedent operation to transition into its final state.
		/// </summary>
		ExecuteSynchronously = 8,

		/// <summary>
		/// Specifies that the continuation should be executed on the default thread. Please see
		/// <see cref="AsyncResult.DefaultSynchronizationContext"/> for more information.
		/// </summary>
		ExecuteOnDefaultContext = 16,
	}
}
