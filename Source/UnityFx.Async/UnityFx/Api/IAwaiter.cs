// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace UnityFx.Async
{
	/// <summary>
	/// An generic awaiter.
	/// </summary>
	public interface IAwaiter : INotifyCompletion
	{
		/// <summary>
		/// Returns <c>true</c> if the source awaitable is completed; <c>false</c> otherwise. Read only.
		/// </summary>
		bool IsCompleted { get; }

		/// <summary>
		/// Returns the source result value.
		/// </summary>
		void GetResult();
	}
}
