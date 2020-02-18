// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A cancellable operation.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncCancellable
	{
		/// <summary>
		/// Initiates cancellation of an asynchronous operation. There is no guarantee that this call will actually cancel
		/// the operation or that the operation will be cancelled immidiately.
		/// </summary>
		void Cancel();
	}
}
