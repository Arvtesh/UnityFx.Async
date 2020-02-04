// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;

namespace UnityFx.Async.Extensions
{
#if !NET35

	/// <summary>
	/// Extension methods for <see cref="IObservable{T}"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class IObservableExtensions
	{
		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> instance that can be used to track the source observable.
		/// </summary>
		/// <typeparam name="T">Type of the operation result.</typeparam>
		/// <param name="observable">The source observable.</param>
		/// <returns>Returns an <see cref="IAsyncOperation{T}"/> instance that can be used to track the observable.</returns>
		public static IAsyncOperation<T> ToAsync<T>(this IObservable<T> observable)
		{
			return new FromObservableResult<T>(observable);
		}
	}

#endif
}
