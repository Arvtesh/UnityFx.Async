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
		/// Attempts to cancel the operation. When this method returns the operation can still be uncompleted.
		/// </summary>
		void Cancel();
	}
}
