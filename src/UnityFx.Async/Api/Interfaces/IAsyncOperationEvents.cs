// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// References a method to be called when a corresponding operation completes.
	/// </summary>
	/// <param name="op">The asynchronous operation.</param>
	/// <seealso cref="IAsyncOperationEvents"/>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="IAsyncContinuation"/>
	public delegate void AsyncOperationCallback(IAsyncOperation op);

	/// <summary>
	/// Manages events and callbacks of <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncOperationEvents
	{
		/// <summary>
		/// Raised when the operation has completed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the callback registered
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveContinuation(AsyncOperationCallback)"/>
		event AsyncCompletedEventHandler Completed;

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed the <paramref name="action"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback)"/>
		/// <seealso cref="RemoveContinuation(AsyncOperationCallback)"/>
		void AddContinuation(AsyncOperationCallback action);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddContinuation(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveContinuation(AsyncOperationCallback)"/>
		bool TryAddContinuation(AsyncOperationCallback action);

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed <paramref name="action"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveContinuation(AsyncOperationCallback)"/>
		void AddContinuation(AsyncOperationCallback action, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddContinuation(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback)"/>
		/// <seealso cref="RemoveContinuation(AsyncOperationCallback)"/>
		bool TryAddContinuation(AsyncOperationCallback action, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddContinuation(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddContinuation(AsyncOperationCallback)"/>
		bool RemoveContinuation(AsyncOperationCallback action);

		/// <summary>
		/// Adds a continuation to be executed after the operation has completed. If the operation is completed <paramref name="continuation"/> is invoked synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddContinuation(IAsyncContinuation)"/>
		/// <seealso cref="AddContinuation(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveContinuation(IAsyncContinuation)"/>
		void AddContinuation(IAsyncContinuation continuation);

		/// <summary>
		/// Attempts to add a continuation to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The cotinuation to be executed when the operation has completed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddContinuation(IAsyncContinuation)"/>
		/// <seealso cref="TryAddContinuation(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveContinuation(IAsyncContinuation)"/>
		bool TryAddContinuation(IAsyncContinuation continuation);

		/// <summary>
		/// Adds a continuation to be executed after the operation has completed. If the operation is completed <paramref name="continuation"/> is invoked synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="TryAddContinuation(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="AddContinuation(IAsyncContinuation)"/>
		/// <seealso cref="RemoveContinuation(IAsyncContinuation)"/>
		void AddContinuation(IAsyncContinuation continuation, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a continuation to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The cotinuation to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddContinuation(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddContinuation(IAsyncContinuation)"/>
		/// <seealso cref="RemoveContinuation(IAsyncContinuation)"/>
		bool TryAddContinuation(IAsyncContinuation continuation, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing continuation.
		/// </summary>
		/// <param name="continuation">The continuation to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="continuation"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TryAddContinuation(IAsyncContinuation)"/>
		bool RemoveContinuation(IAsyncContinuation continuation);
	}
}
