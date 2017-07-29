// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A continuation container.
	/// </summary>
	public interface IAsyncContinuationContainer
	{
		/// <summary>
		/// Adds new continuation to the operation.
		/// </summary>
		void AddContinuation(Action continuation);

		/// <summary>
		/// Removes existing continuation from the operation.
		/// </summary>
		void RemoveContinuation(Action continuation);
	}
}
