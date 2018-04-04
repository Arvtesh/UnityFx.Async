 // Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies the behavior of an asynchronous opration continuation.
	/// </summary>
	/// <seealso cref="IAsyncContinuation"/>
	[Flags]
	public enum AsyncContinuationOptions
	{
		/// <summary>
		/// When no continuation options are specified, specifies that default behavior should be used when executing a continuation.
		/// I.e. continuation is scheduled оn the same <see cref="SynchronizationContext"/> that was active when the continuation was created.
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
		ExecuteSynchronously = 8
	}

	/// <summary>
	/// A generic continuation.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncContinuation
	{
		/// <summary>
		/// Starts the continuation.
		/// </summary>
		/// <param name="op">The completed antecedent operation.</param>
		void Invoke(IAsyncOperation op);
	}
}
