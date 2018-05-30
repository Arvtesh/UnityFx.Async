﻿// Copyright (c) Alexander Bogarsukov.
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
		/// Raised when the operation progress has changed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the event handler
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="Completed"/>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(AsyncOperationCallback)"/>
		event ProgressChangedEventHandler ProgressChanged;

		/// <summary>
		/// Raised when the operation has completed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the event handler
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="ProgressChanged"/>
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
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
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		void AddCompletionCallback(AsyncOperationCallback action);

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
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		bool TryAddCompletionCallback(AsyncOperationCallback action);

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
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		void AddCompletionCallback(AsyncOperationCallback action, SynchronizationContext syncContext);

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
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="RemoveCompletionCallback(AsyncOperationCallback)"/>
		bool TryAddCompletionCallback(AsyncOperationCallback action, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddCompletionCallback(AsyncOperationCallback, SynchronizationContext)"/>
		bool RemoveCompletionCallback(AsyncOperationCallback action);

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
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		void AddCompletionCallback(IAsyncContinuation continuation);

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
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		bool TryAddCompletionCallback(IAsyncContinuation continuation);

		/// <summary>
		/// Adds a continuation to be executed after the operation has completed. If the operation is completed <paramref name="continuation"/>
		/// is invoked on the <paramref name="syncContext"/> specified.
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
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		void AddCompletionCallback(IAsyncContinuation continuation, SynchronizationContext syncContext);

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
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		bool TryAddCompletionCallback(IAsyncContinuation continuation, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing continuation.
		/// </summary>
		/// <param name="continuation">The continuation to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="continuation"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		bool RemoveCompletionCallback(IAsyncContinuation continuation);

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed the <paramref name="action"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(AsyncOperationCallback)"/>
		void AddProgressCallback(AsyncOperationCallback action);

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(AsyncOperationCallback)"/>
		bool TryAddProgressCallback(AsyncOperationCallback action);

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is completed <paramref name="action"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(AsyncOperationCallback)"/>
		void AddProgressCallback(AsyncOperationCallback action, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the callback to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="RemoveProgressCallback(AsyncOperationCallback)"/>
		bool TryAddProgressCallback(AsyncOperationCallback action, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing progress callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="AddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback)"/>
		/// <seealso cref="TryAddProgressCallback(AsyncOperationCallback, SynchronizationContext)"/>
		bool RemoveProgressCallback(AsyncOperationCallback action);

#if !NET35

		/// <summary>
		/// Adds a callback to be executed each time progress value changes. If the operation is completed <paramref name="callback"/> is invoked synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		void AddProgressCallback(IProgress<float> callback);

		/// <summary>
		/// Adds a callback to be executed each time progress value changes. If the operation is completed <paramref name="callback"/>
		/// is invoked on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		void AddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a progress callback to be executed each time progress value changes. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float})"/>
		/// <seealso cref="AddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		bool TryAddProgressCallback(IProgress<float> callback);

		/// <summary>
		/// Attempts to add a progress callback to be executed each time progress value changes. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the callback to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float})"/>
		/// <seealso cref="AddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float})"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		bool TryAddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing progress callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddProgressCallback(IProgress{float})"/>
		/// <seealso cref="AddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		bool RemoveProgressCallback(IProgress<float> callback);

#endif
	}
}
