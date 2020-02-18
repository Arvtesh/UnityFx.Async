// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Generic interface of a <see cref="AsyncResult"/> callback storage.
	/// </summary>
	internal interface IAsyncCallbackCollection
	{
		/// <summary>
		/// Adds a new completion callback.
		/// </summary>
		/// <param name="callback">The callback instance to add.</param>
		/// <param name="syncContext">A synchronization context to invoke callback on or <see langword="null"/>.</param>
		/// <seealso cref="AddProgressCallback(object, SynchronizationContext)"/>
		void AddCompletionCallback(object callback, SynchronizationContext syncContext);

		/// <summary>
		/// Adds a new progress callback.
		/// </summary>
		/// <param name="callback">The callback instance to add.</param>
		/// <param name="syncContext">A synchronization context to invoke callback on or <see langword="null"/>.</param>
		/// <seealso cref="AddCompletionCallback(object, SynchronizationContext)"/>
		void AddProgressCallback(object callback, SynchronizationContext syncContext);

		/// <summary>
		/// Removes the specified completion/progress callback.
		/// </summary>
		/// <param name="callback">The calback instance to remove.</param>
		/// <returns>Returns <see langword="true"/> if the callback was actually present in the collection; otherwise <see langword="false"/>.</returns>
		/// <seealso cref="AddCompletionCallback(object, SynchronizationContext)"/>
		/// <seealso cref="AddProgressCallback(object, SynchronizationContext)"/>
		bool Remove(object callback);

		/// <summary>
		/// Invokes all callbacks stored in the collection.
		/// </summary>
		/// <param name="invokeAsync">If <see langword="true"/> the callback is posted to the synchronization context even if it matches the calling thread one.</param>
		/// <seealso cref="InvokeProgressCallbacks"/>
		void Invoke(bool invokeAsync);

		/// <summary>
		/// Invokes progress callbacks only.
		/// </summary>
		/// <seealso cref="Invoke(bool)"/>
		void InvokeProgressCallbacks();
	}
}
