// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Defines an updatable entity.
	/// </summary>
	/// <seealso cref="IAsyncUpdateSource"/>
	public interface IAsyncUpdatable
	{
		/// <summary>
		/// Updates the object state. Called by <see cref="IAsyncUpdateSource"/>.
		/// </summary>
		/// <param name="frameTime">Time since last call in seconds.</param>
		void Update(float frameTime);
	}
}
