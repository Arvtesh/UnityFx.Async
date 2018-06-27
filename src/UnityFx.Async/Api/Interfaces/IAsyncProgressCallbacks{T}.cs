// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Manages progress callbacks of <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncProgressCallbacks<in T> where T : class
	{
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
		/// <seealso cref="AddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(T)"/>
		/// <seealso cref="TryAddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(T)"/>
		void AddProgressCallback(T callback);

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
		/// <seealso cref="AddProgressCallback(T)"/>
		/// <seealso cref="AddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(T)"/>
		bool TryAddProgressCallback(T callback);

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
		/// <seealso cref="AddProgressCallback(T)"/>
		/// <seealso cref="TryAddProgressCallback(T)"/>
		/// <seealso cref="TryAddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="RemoveProgressCallback(T)"/>
		void AddProgressCallback(T callback, SynchronizationContext syncContext);

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
		/// <seealso cref="AddProgressCallback(T)"/>
		/// <seealso cref="AddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(T)"/>
		/// <seealso cref="RemoveProgressCallback(T)"/>
		bool TryAddProgressCallback(T callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes an existing progress callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddProgressCallback(T)"/>
		/// <seealso cref="AddProgressCallback(T, SynchronizationContext)"/>
		/// <seealso cref="TryAddProgressCallback(T)"/>
		/// <seealso cref="TryAddProgressCallback(T, SynchronizationContext)"/>
		bool RemoveProgressCallback(T callback);
	}
}
