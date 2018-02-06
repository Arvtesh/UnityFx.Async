// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

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
		/// <seealso cref="AddCompletionCallback(Action)"/>
		/// <seealso cref="AddOrInvokeCompletionCallback(Action)"/>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		event EventHandler Completed;

		/// <summary>
		/// Adds a completion callback to be executed after the operation has finished.
		/// </summary>
		/// <param name="action">The callback to be executed when the operation has finished.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddOrInvokeCompletionCallback(Action)"/>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		/// <seealso cref="Completed"/>
		void AddCompletionCallback(Action action);

		/// <summary>
		/// Adds a completion callback to be executed after the operation has finished. If the operation is already
		/// in completed state just invokes the <paramref name="action"/>.
		/// </summary>
		/// <param name="action">The callback to be executed when the operation has finished.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddCompletionCallback(Action)"/>
		/// <seealso cref="RemoveCompletionCallback(Action)"/>
		/// <seealso cref="Completed"/>
		void AddOrInvokeCompletionCallback(Action action);

		/// <summary>
		/// Removes existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddOrInvokeCompletionCallback(Action)"/>
		/// <seealso cref="Completed"/>
		void RemoveCompletionCallback(Action action);
	}
}
