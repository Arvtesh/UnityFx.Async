// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// A lightweight net35-compatible asynchronous operation that can return a value.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <seealso cref="AsyncCompletionSource{T}"/>
	/// <seealso cref="AsyncResult"/>
	/// <seealso cref="IAsyncResult"/>
#if NET35
	public class AsyncResult<T> : AsyncResult, IAsyncOperation<T>
#else
	public class AsyncResult<T> : AsyncResult, IAsyncOperation<T>, IObservable<T>
#endif
	{
		#region data

		private const string _errorResultNotAvailable = "The operation result is not available.";

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
		/// <param name="status">Status value of the operation.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: base(status, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		internal AsyncResult(Exception exception)
			: base(exception)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="exceptions">Exceptions to complete the operation with.</param>
		internal AsyncResult(IEnumerable<Exception> exceptions)
			: base(exceptions)
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

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TrySetResult(T result, bool completedSynchronously)
		{
			ThrowIfDisposed();

			if (TryReserveCompletion())
			{
				_result = result;
				SetCompleted(StatusRanToCompletion, completedSynchronously);
				return true;
			}

			return false;
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override bool OnReset()
		{
			if (base.OnReset())
			{
				_result = default(T);
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
					throw new InvalidOperationException(_errorResultNotAvailable);
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
					observer.OnError(Exception.InnerException);
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
