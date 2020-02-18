// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Specifies flags that control optional behavior for the creation and execution of operations.
	/// </summary>
	/// <seealso cref="AsyncResult"/>
	[Flags]
	public enum AsyncCreationOptions
	{
		/// <summary>
		/// Specifies that the default behavior should be used.
		/// </summary>
		None = 0,

		/// <summary>
		/// Forces continuations added to the current operation to be executed asynchronously.
		/// </summary>
		RunContinuationsAsynchronously = AsyncResult.OptionRunContinuationsAsynchronously,

		/// <summary>
		/// If set cancelling the operation has no effect (silently ignored).
		/// </summary>
		SuppressCancellation = AsyncResult.OptionSuppressCancellation
	}
}
