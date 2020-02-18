// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A provider of update notifications.
	/// </summary>
	/// <seealso cref="IAsyncUpdatable"/>
#if NET35
	public interface IAsyncUpdateSource
#else
	public interface IAsyncUpdateSource : IObservable<float>
#endif
	{
		/// <summary>
		/// Adds a new update listener.
		/// </summary>
		/// <param name="updateCallback">An update callback.</param>
		/// <exception cref="ArgumentNullException">Thrown is <paramref name="updateCallback"/> is <see langword="null"/>.</exception>
		void AddListener(Action<float> updateCallback);

		/// <summary>
		/// Removes an existing listener.
		/// </summary>
		/// <param name="updateCallback">An update listener. Can be <see langword="null"/>.</param>
		void RemoveListener(Action<float> updateCallback);

		/// <summary>
		/// Adds a new update listener.
		/// </summary>
		/// <param name="updatable">An update listener.</param>
		/// <exception cref="ArgumentNullException">Thrown is <paramref name="updatable"/> is <see langword="null"/>.</exception>
		void AddListener(IAsyncUpdatable updatable);

		/// <summary>
		/// Removes an existing listener.
		/// </summary>
		/// <param name="updatable">An update listener. Can be <see langword="null"/>.</param>
		void RemoveListener(IAsyncUpdatable updatable);
	}
}
