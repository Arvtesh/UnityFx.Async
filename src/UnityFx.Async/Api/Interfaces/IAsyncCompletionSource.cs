// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents the producer side of a <see cref="IAsyncOperation"/> unbound to a delegate, providing access to the consumer side through the <see cref="Operation"/> property.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public interface IAsyncCompletionSource
	{
		/// <summary>
		/// Gets the operation being controller by the source.
		/// </summary>
		/// <value>The underlying operation instance.</value>
		IAsyncOperation Operation { get; }

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception})"/>
		/// <seealso cref="TrySetCompleted"/>
		bool TrySetCanceled();

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception})"/>
		/// <seealso cref="TrySetCanceled"/>
		/// <seealso cref="TrySetCompleted"/>
		bool TrySetException(Exception exception);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception)"/>
		/// <seealso cref="TrySetCanceled"/>
		/// <seealso cref="TrySetCompleted"/>
		bool TrySetExceptions(IEnumerable<Exception> exceptions);

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCanceled"/>
		/// <seealso cref="TrySetException(Exception)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception})"/>
		bool TrySetCompleted();
	}
}
