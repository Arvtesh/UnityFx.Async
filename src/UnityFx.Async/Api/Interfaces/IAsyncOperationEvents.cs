// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// References a method to be called when a corresponding operation completes.
	/// </summary>
	/// <param name="op">The asynchronous operation.</param>
	/// <seealso cref="IAsyncOperationEvents"/>
	/// <seealso cref="IAsyncOperation"/>
	public delegate void AsyncOperationCallback(IAsyncOperation op);

	/// <summary>
	/// Specifies the behavior of an asynchronous opration continuation.
	/// </summary>
	[Flags]
	public enum AsyncContinuationOptions
	{
		/// <summary>
		/// When no continuation options are specified, specifies that default behavior should be used when executing a continuation.
		/// I.e. continuation is scheduled independently of the operation completion status.
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
		/// Specifies whether a <see cref="SynchronizationContext"/> should be captured when a continuation is registered.
		/// </summary>
		CaptureSynchronizationContext = 8
	}

	/// <summary>
	/// A controller for <see cref="IAsyncOperation"/> completion callbacks.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncOperationEvents
	{
		/// <summary>
		/// Raised when the operation has completed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, AsyncContinuationOptions, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		event AsyncOperationCallback Completed;

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Options for when the callback is executed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion. The argument value is ignored if <paramref name="options"/>
		/// is set to <see cref="AsyncContinuationOptions.CaptureSynchronizationContext"/>.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		bool TryAddCompletionCallback(AsyncOperationCallback action, AsyncContinuationOptions options, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if the <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, AsyncContinuationOptions, SynchronizationContext)"/>
		bool RemoveCompletionCallback(AsyncOperationCallback action);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Options for when the callback is executed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		bool TryAddCompletionCallback(Action action, AsyncContinuationOptions options);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if the <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddCompletionCallback(Action, AsyncContinuationOptions)"/>
		bool RemoveCompletionCallback(Action action);
	}
}
