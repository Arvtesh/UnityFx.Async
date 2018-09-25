// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
#if !NET35

	partial class AsyncExtensions
	{
		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> instance that can be used to track the source observable.
		/// </summary>
		/// <typeparam name="T">Type of the operation result.</typeparam>
		/// <param name="observable">The source observable.</param>
		/// <returns>Returns an <see cref="IAsyncOperation{T}"/> instance that can be used to track the observable.</returns>
		public static IAsyncOperation<T> ToAsync<T>(this IObservable<T> observable)
		{
			return new AsyncObservableResult<T>(observable);
		}
	}

#endif
}
