// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents the producer side of a <see cref="IAsyncOperation{TResult}"/> unbound to a delegate, providing access to the consumer side through the <see cref="Operation"/> property.
	/// </summary>
	/// <typeparam name="TResult">Type of the operation result value.</typeparam>
	/// <seealso cref="IAsyncOperation{TResult}"/>
	public interface IAsyncCompletionSource<TResult>
	{
		/// <summary>
		/// Gets the operation being controller by the source.
		/// </summary>
		/// <value>The underlying operation instance.</value>
		IAsyncOperation<TResult> Operation { get; }

		/// <summary>
		/// Attempts to set the operation progress value in range [0, 1].
		/// </summary>
		/// <param name="progress">The operation progress in range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="progress"/> is not in range [0, 1].</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetResult(TResult)"/>
		bool TrySetProgress(float progress);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception)"/>
		/// <seealso cref="TrySetResult(TResult)"/>
		bool TrySetCanceled();

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCanceled"/>
		/// <seealso cref="TrySetResult(TResult)"/>
		bool TrySetException(Exception exception);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCanceled"/>
		/// <seealso cref="TrySetException(Exception)"/>
		bool TrySetResult(TResult result);
	}
}
