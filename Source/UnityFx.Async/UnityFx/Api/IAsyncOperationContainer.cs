// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A container of asynchronous operations.
	/// </summary>
	internal interface IAsyncOperationContainer : IAsyncOperation
	{
		/// <summary>
		/// Returns the total number of operations in the container. Read only.
		/// </summary>
		int Size { get; }
	}
}
