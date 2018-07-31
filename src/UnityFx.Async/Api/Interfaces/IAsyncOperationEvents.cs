// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Manages events and callbacks of <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncOperationEvents
	{
		/// <summary>
		/// Raised when the operation progress is changed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the event handler
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="Completed"/>
		event ProgressChangedEventHandler ProgressChanged;

		/// <summary>
		/// Raised when the operation is completed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the event handler
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="ProgressChanged"/>
		event AsyncCompletedEventHandler Completed;

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(Action{IAsyncOperation})"/>
		void AddCompletionCallback(Action<IAsyncOperation> callback);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(Action{IAsyncOperation})"/>
		bool TryAddCompletionCallback(Action<IAsyncOperation> callback);

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed <paramref name="callback"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(Action{IAsyncOperation})"/>
		void AddCompletionCallback(Action<IAsyncOperation> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="RemoveCompletionCallback(Action{IAsyncOperation})"/>
		bool TryAddCompletionCallback(Action<IAsyncOperation> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="AddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation})"/>
		/// <seealso cref="TryAddCompletionCallback(Action{IAsyncOperation}, SynchronizationContext)"/>
		bool RemoveCompletionCallback(Action<IAsyncOperation> callback);

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		void AddCompletionCallback(IAsyncContinuation callback);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		bool TryAddCompletionCallback(IAsyncContinuation callback);

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed <paramref name="callback"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		void AddCompletionCallback(IAsyncContinuation callback, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="RemoveCompletionCallback(IAsyncContinuation)"/>
		bool TryAddCompletionCallback(IAsyncContinuation callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation)"/>
		/// <seealso cref="TryAddCompletionCallback(IAsyncContinuation, SynchronizationContext)"/>
		bool RemoveCompletionCallback(IAsyncContinuation callback);

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(Action{float})"/>
		/// <seealso cref="TryAddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(Action{float})"/>
		void AddProgressCallback(Action<float> callback);

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(Action{float})"/>
		/// <seealso cref="AddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(Action{float})"/>
		bool TryAddProgressCallback(Action<float> callback);

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is completed <paramref name="callback"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(Action{float})"/>
		/// <seealso cref="TryAddProgressCallback(Action{float})"/>
		/// <seealso cref="TryAddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(Action{float})"/>
		void AddProgressCallback(Action<float> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the callback to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(Action{float})"/>
		/// <seealso cref="AddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(Action{float})"/>
		/// <seealso cref="RemoveProgressCallback(Action{float})"/>
		bool TryAddProgressCallback(Action<float> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing progress callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddProgressCallback(Action{float})"/>
		/// <seealso cref="AddProgressCallback(Action{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(Action{float})"/>
		/// <seealso cref="TryAddProgressCallback(Action{float}, SynchronizationContext)"/>
		bool RemoveProgressCallback(Action<float> callback);

#if !NET35

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		void AddProgressCallback(IProgress<float> callback);

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float})"/>
		/// <seealso cref="AddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		bool TryAddProgressCallback(IProgress<float> callback);

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is completed <paramref name="callback"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float})"/>
		/// <seealso cref="TryAddProgressCallback(IProgress{float}, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(IProgress{float})"/>
		void AddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext);

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the callback to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
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
