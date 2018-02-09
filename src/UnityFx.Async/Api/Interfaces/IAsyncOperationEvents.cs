// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
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
		/// The event handler is invoked on a thread that initiated the operation completion (not on a thread that registered it).
		/// If the operation is already completed the event handler is not called.
		/// </remarks>
		/// <seealso cref="AddCompletionCallback(Action, bool)"/>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		event EventHandler Completed;

		/// <summary>
		/// Adds a completion callback to be executed after the operation has finished. If the operation is completed the <paramref name="action"/> is invoked synchronously.
		/// </summary>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> method attempts to marshal the continuation back to the current synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddCompletionCallback(Action, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		/// <seealso cref="Completed"/>
		void AddCompletionCallback(Action action, bool continueOnCapturedContext);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished.
		/// </summary>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action, bool)"/>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		/// <seealso cref="Completed"/>
		bool TryAddCompletionCallback(Action action, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action, bool)"/>
		/// <seealso cref="TryAddCompletionCallback(Action, SynchronizationContext)"/>
		/// <seealso cref="Completed"/>
		void RemoveCompletionCallback(Action action);
	}
}
