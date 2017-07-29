// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace UnityFx.Async
{
	/// <summary>
	/// An generic awaiter.
	/// </summary>
#if UNITYFX_NET35
	public interface IAwaiter
#else
	public interface IAwaiter : INotifyCompletion
#endif
	{
		/// <summary>
		/// Returns <c>true</c> if the source awaitable is completed; <c>false</c> otherwise. Read only.
		/// </summary>
		bool IsCompleted { get; }

		/// <summary>
		/// Returns the source result value.
		/// </summary>
		void GetResult();

#if UNITYFX_NET35
		/// <summary>
		/// Schedules the continuation action that's invoked when the instance completes.
		/// </summary>
		/// <param name="continuation">The action to invoke when the operation completes.</param>
		/// <exception cref="ArgumentNullException">The continuation argument is <c>null</c>.</exception>
		void OnCompleted(Action continuation);
#endif
	}
}
