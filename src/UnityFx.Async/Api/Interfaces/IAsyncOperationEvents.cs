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
	public interface IAsyncOperationEvents :
#if NET35
		IAsyncCompletionCallbacks<Action<IAsyncOperation>>,
		IAsyncCompletionCallbacks<IAsyncContinuation>,
		IAsyncProgressCallbacks<Action<float>>
#else
		IAsyncCompletionCallbacks<Action<IAsyncOperation>>,
		IAsyncCompletionCallbacks<IAsyncContinuation>,
		IAsyncProgressCallbacks<Action<float>>,
		IAsyncProgressCallbacks<IProgress<float>>
#endif
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
		/// Schedules a <paramref name="continuation"/> to be started (or cancelled in case of the operatino failure) after the operation has completed.
		/// If the operation is already completed the <paramref name="continuation"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The continuation to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="Schedule(IAsyncSchedulable, SynchronizationContext)"/>
		void Schedule(IAsyncSchedulable continuation);

		/// <summary>
		/// Schedules a <paramref name="continuation"/> to be started (or cancelled in case of the operatino failure) after the operation has completed.
		/// If the operation is completed <paramref name="continuation"/> is invoked on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The continuation to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="Schedule(IAsyncSchedulable)"/>
		void Schedule(IAsyncSchedulable continuation, SynchronizationContext syncContext);
	}
}
