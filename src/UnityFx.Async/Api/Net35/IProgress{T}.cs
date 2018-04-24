// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
#if NET35

	/// <summary>
	/// Defines a provider for progress updates.
	/// </summary>
	/// <typeparam name="T">The type of progress update value.</typeparam>
	public interface IProgress<in T>
	{
		/// <summary>
		/// Reports a progress update.
		/// </summary>
		/// <param name="value">The value of the updated progress.</param>
		void Report(T value);
	}

#endif
}
