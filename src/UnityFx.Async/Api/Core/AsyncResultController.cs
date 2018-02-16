// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// tt
	/// </summary>
	public sealed class AsyncResultController : AsyncResult, IAsyncCompletionSource
	{
		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultController"/> class.
		/// </summary>
		public AsyncResultController()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultController"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncResultController(AsyncCallback asyncCallback, object asyncState)
			: base(asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultController"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		public AsyncResultController(AsyncOperationStatus status)
			: base(status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultController"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncResultController(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: base(status, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultController"/> class that is faulted.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="e"/> is <see langword="null"/>.</exception>
		public AsyncResultController(Exception e)
			: base(e)
		{
		}

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetScheduled"/>
		/// <seealso cref="SetRunning"/>
		public void SetScheduled()
		{
			if (!TrySetScheduled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetScheduled"/>
		public new bool TrySetScheduled() => base.TrySetScheduled();

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="SetScheduled"/>
		public void SetRunning()
		{
			if (!TrySetRunning())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetRunning"/>
		public new bool TrySetRunning() => base.TrySetRunning();

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		public void SetCanceled(bool completedSynchronously)
		{
			if (!base.TrySetCanceled(completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetCanceled(bool completedSynchronously) => base.TrySetCanceled(completedSynchronously);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		public void SetException(Exception exception, bool completedSynchronously)
		{
			if (!base.TrySetException(exception, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetException(Exception exception, bool completedSynchronously) => base.TrySetException(exception, completedSynchronously);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		public void SetExceptions(IEnumerable<Exception> exceptions, bool completedSynchronously)
		{
			if (!base.TrySetExceptions(exceptions, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetExceptions(IEnumerable<Exception> exceptions, bool completedSynchronously) => base.TrySetExceptions(exceptions, completedSynchronously);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		public void SetCompleted(bool completedSynchronously)
		{
			if (!base.TrySetCompleted(completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetCompleted(bool completedSynchronously) => base.TrySetCompleted(completedSynchronously);

		#endregion

		#region IAsyncCompletionSource

		/// <inheritdoc/>
		public IAsyncOperation Operation => this;

		/// <inheritdoc/>
		public bool TrySetCanceled() => base.TrySetCanceled(false);

		/// <inheritdoc/>
		public bool TrySetCompleted() => base.TrySetCompleted(false);

		/// <inheritdoc/>
		public bool TrySetException(Exception exception) => base.TrySetException(exception, false);

		/// <inheritdoc/>
		public bool TrySetExceptions(IEnumerable<Exception> exceptions) => base.TrySetExceptions(exceptions, false);

		#endregion
	}
}
