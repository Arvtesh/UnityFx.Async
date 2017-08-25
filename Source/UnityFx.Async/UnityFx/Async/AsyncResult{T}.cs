// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <seealso cref="IAsyncResult"/>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="AsyncResult"/>
	public class AsyncResult<T> : AsyncResult, IAsyncOperationController<T>, IAsyncOperation<T>
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
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncResult.AsyncState"/>.</param>
		public AsyncResult(object asyncState)
			: base(asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncResult.AsyncState"/>.</param>
		/// <param name="status">Initial operation status.</param>
		public AsyncResult(object asyncState, AsyncOperationStatus status)
			: base(asyncState, (int)status)
		{
		}

#if NET46
		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncResult.AsyncState"/>.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public AsyncResult(object asyncState, CancellationToken cancellationToken)
			: base(asyncState, cancellationToken)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncResult.AsyncState"/>.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <param name="status">Initial operation status.</param>
		public AsyncResult(object asyncState, CancellationToken cancellationToken, AsyncOperationStatus status)
			: base(asyncState, cancellationToken, status)
		{
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncResult.AsyncState"/>.</param>
		/// <param name="e">An exception instance.</param>
		public AsyncResult(object asyncState, Exception e)
			: base(asyncState, e)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncResult.AsyncState"/>.</param>
		/// <param name="result">Operation result.</param>
		public AsyncResult(object asyncState, T result)
			: base(asyncState, StatusCompleted)
		{
			_result = result;
		}

		#endregion

		#region IAsyncOperationController

		/// <inheritdoc/>
		public void SetResult(T result)
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCompleted))
			{
				_result = result;
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetResult(T result)
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCompleted))
			{
				_result = result;
				FireCompleted();
				return true;
			}

			return false;
		}

		#endregion

		#region IAsyncResult

		/// <summary>
		/// Returns the result value of this operation. Accessing the property blocks the calling thread until the operation is complete. Read only.
		/// </summary>
		public T Result
		{
			get
			{
				ThrowIfDisposed();
				AsyncExtensions.Wait(this);
				ThrowIfFaulted();
				return _result;
			}
		}

		#endregion

		#region IObservable

#if NET46

		/// <inheritdoc/>
		public IDisposable Subscribe(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			return new DisposableSubscription(this, () =>
			{
				if (IsCompletedSuccessfully)
				{
					observer.OnNext(_result);
					observer.OnCompleted();
				}
				else
				{
					observer.OnError(GetExceptionSafe());
				}
			});
		}

		private class DisposableSubscription : IDisposable
		{
			private readonly IAsyncContinuationContainer _op;
			private readonly Action _action;

			public DisposableSubscription(IAsyncContinuationContainer op, Action action)
			{
				_op = op;
				_op.AddContinuation(action);
				_action = action;
			}

			public void Dispose()
			{
				_op.RemoveContinuation(_action);
			}
		}

#endif

		#endregion
	}
}
