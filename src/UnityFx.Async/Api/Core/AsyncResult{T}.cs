// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <seealso cref="IAsyncResult"/>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="AsyncResult"/>
#if NET35
	public class AsyncResult<T> : AsyncResult, IAsyncCompletionSource<T>, IAsyncOperation<T>
#else
	public class AsyncResult<T> : AsyncResult, IAsyncCompletionSource<T>, IAsyncOperation<T>, IObservable<T>
#endif
	{
		#region data

		private T _result;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		public AsyncResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncCallback asyncCallback, object asyncState)
			: base(asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="status">Status value of the operation.</param>
		public AsyncResult(AsyncOperationStatus status)
			: base(status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="e">The exception to complete the operation with.</param>
		internal AsyncResult(Exception e)
			: base(e)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="result">Result value.</param>
		public AsyncResult(T result)
			: base(AsyncOperationStatus.RanToCompletion)
		{
			_result = result;
		}

		#endregion

		#region IAsyncOperationController

		/// <inheritdoc/>
		public void SetResult(T result) => SetResult(result, false);

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetResult(T)"/>
		public void SetResult(T result, bool completedSynchronously)
		{
			if (!TrySetResult(result, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetResult(T result) => TrySetResult(result, false);

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetResult(T)"/>
		public bool TrySetResult(T result, bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusRanToCompletion, completedSynchronously))
			{
				_result = result;
				OnCompleted();
				return true;
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public T Result
		{
			get
			{
				if (!IsCompletedSuccessfully)
				{
					throw new InvalidOperationException("The operation result is not available.");
				}

				return _result;
			}
		}

		#endregion

		#region IObservable

#if !NET35

		/// <inheritdoc/>
		public IDisposable Subscribe(IObserver<T> observer)
		{
			ThrowIfDisposed();

			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			AsyncOperationCallback completionCallback = op =>
			{
				if (IsCompletedSuccessfully)
				{
					observer.OnNext(_result);
					observer.OnCompleted();
				}
				else if (IsFaulted)
				{
					observer.OnError(Exception);
				}
				else
				{
					observer.OnCompleted();
				}
			};

			if (TryAddCompletionCallback(completionCallback, null))
			{
				return new AsyncObservableSubscription(this, completionCallback);
			}
			else
			{
				completionCallback(this);
				return EmptyDisposable.Instance;
			}
		}

#endif

		#endregion
	}
}
