// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A cancellable operation.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncCancellable
	{
		/// <summary>
		/// Attempts to cancel the operation.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		void Cancel();
	}
}
