// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents an asynchronous operation with external completion control. <see cref="IAsyncCompletionSource{TResult}"/>
	/// </summary>
	/// <typeparam name="TResult">Type of the operation result value.</typeparam>
	/// <seealso cref="AsyncCompletionSource"/>
	public sealed class AsyncCompletionSource<TResult> : AsyncResult<TResult>, IAsyncCompletionSource<TResult>
	{
		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource{T}"/> class.
		/// </summary>
		public AsyncCompletionSource()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource{T}"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(object asyncState)
			: base(default(AsyncCallback), asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource{T}"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCallback asyncCallback, object asyncState)
			: base(asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource{T}"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		public AsyncCompletionSource(AsyncOperationStatus status)
			: base(status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource{T}"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, object asyncState)
			: base(status, null, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource{T}"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: base(status, asyncCallback, asyncState)
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
			if (!base.TrySetScheduled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the underlying operation into the <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the underlying operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetScheduled"/>
		/// <seealso cref="TrySetRunning"/>
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
			if (!base.TrySetRunning())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the underlying operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the underlying operation is disposed.</exception>
		/// <seealso cref="SetRunning"/>
		/// <seealso cref="TrySetScheduled"/>
		public new bool TrySetRunning() => base.TrySetRunning();

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCanceled()"/>
		public void SetCanceled()
		{
			if (!base.TrySetCanceled(false))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCanceled()"/>
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
		/// <seealso cref="SetCanceled(bool)"/>
		/// <seealso cref="TrySetCanceled()"/>
		public new bool TrySetCanceled(bool completedSynchronously) => base.TrySetCanceled(completedSynchronously);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetException(Exception, bool)"/>
		/// <seealso cref="TrySetException(Exception)"/>
		public void SetException(Exception exception)
		{
			if (!base.TrySetException(exception, false))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetException(Exception, bool)"/>
		/// <seealso cref="TrySetException(Exception)"/>
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
		/// <seealso cref="SetException(Exception, bool)"/>
		/// <seealso cref="TrySetException(Exception)"/>
		public new bool TrySetException(Exception exception, bool completedSynchronously) => base.TrySetException(exception, completedSynchronously);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception}, bool)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception})"/>
		public void SetExceptions(IEnumerable<Exception> exceptions)
		{
			if (!base.TrySetExceptions(exceptions, false))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception}, bool)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception})"/>
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
		/// <seealso cref="SetExceptions(IEnumerable{Exception}, bool)"/>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception})"/>
		public new bool TrySetExceptions(IEnumerable<Exception> exceptions, bool completedSynchronously) => base.TrySetExceptions(exceptions, completedSynchronously);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetResult(TResult, bool)"/>
		/// <seealso cref="TrySetResult(TResult)"/>
		public void SetResult(TResult result)
		{
			if (!base.TrySetResult(result, false))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetResult(TResult, bool)"/>
		/// <seealso cref="TrySetResult(TResult)"/>
		public void SetResult(TResult result, bool completedSynchronously)
		{
			if (!base.TrySetResult(result, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetResult(TResult, bool)"/>
		/// <seealso cref="TrySetResult(TResult)"/>
		public new bool TrySetResult(TResult result, bool completedSynchronously) => base.TrySetResult(result, completedSynchronously);

		/// <summary>
		/// Copies state of the specified operation.
		/// </summary>
		internal void CopyCompletionState(IAsyncOperation<TResult> patternOp, bool completedSynchronously)
		{
			if (!TryCopyCompletionState(patternOp, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attemts to copy state of the specified operation.
		/// </summary>
		internal bool TryCopyCompletionState(IAsyncOperation<TResult> patternOp, bool completedSynchronously)
		{
			if (patternOp.IsCompletedSuccessfully)
			{
				return base.TrySetResult(patternOp.Result, completedSynchronously);
			}
			else if (patternOp.IsFaulted || patternOp.IsCanceled)
			{
				return base.TrySetException(patternOp.Exception, completedSynchronously);
			}

			return false;
		}

		#endregion

		#region IAsyncCompletionSource

		/// <inheritdoc/>
		public IAsyncOperation<TResult> Operation => this;

		/// <inheritdoc/>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="SetCanceled(bool)"/>
		public bool TrySetCanceled() => base.TrySetCanceled(false);

		/// <inheritdoc/>
		/// <seealso cref="TrySetException(Exception, bool)"/>
		/// <seealso cref="SetException(Exception, bool)"/>
		public bool TrySetException(Exception exception) => base.TrySetException(exception, false);

		/// <inheritdoc/>
		/// <seealso cref="TrySetExceptions(IEnumerable{Exception}, bool)"/>
		/// <seealso cref="SetExceptions(IEnumerable{Exception}, bool)"/>
		public bool TrySetExceptions(IEnumerable<Exception> exceptions) => base.TrySetExceptions(exceptions, false);

		/// <inheritdoc/>
		/// <seealso cref="TrySetResult(TResult, bool)"/>
		/// <seealso cref="SetResult(TResult, bool)"/>
		public bool TrySetResult(TResult result) => base.TrySetResult(result, false);

		#endregion
	}
}
