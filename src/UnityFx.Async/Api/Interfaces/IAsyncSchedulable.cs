// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A schedulable operation.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncSchedulable : IAsyncCancellable
	{
		/// <summary>
		/// Starts the operation execution.
		/// </summary>
		/// <remarks>
		/// An operation may be started on once. Any attempts to schedule it a second time will result in an exception.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if the operation has already been started.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		void Start();
	}
}
