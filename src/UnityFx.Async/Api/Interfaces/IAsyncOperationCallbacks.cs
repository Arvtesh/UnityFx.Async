// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Defines basic callback management tools for <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public interface IAsyncOperationCallbacks
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
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed <paramref name="callback"/> is invoked
		/// on the <paramref name="syncContext"/> specified. Throwing an exception from the callback causes unspecified behaviour. The method is not intended
		/// to be used by user code. Do not use if not sure.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation has completed. Can be one of <see cref="Action"/>, <see cref="Action{T}"/>
		/// (with <see cref="IAsyncOperation"/> argument type), <see cref="AsyncCallback"/>, <see cref="IAsyncContinuation"/> or <see cref="AsyncCompletedEventHandler"/>.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(object, SynchronizationContext)"/>
		/// <seealso cref="RemoveCallback(object)"/>
		void AddCompletionCallback(object callback, SynchronizationContext syncContext);

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is completed <paramref name="callback"/> is invoked
		/// on the <paramref name="syncContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour. The method is not intended
		/// to be used by user code. Do not use if not sure.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(object, SynchronizationContext)"/>
		/// <seealso cref="RemoveCallback(object)"/>
		void AddProgressCallback(object callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing completion/progress callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddCompletionCallback(object, SynchronizationContext)"/>
		/// <seealso cref="AddProgressCallback(object, SynchronizationContext)"/>
		bool RemoveCallback(object callback);
	}
}
