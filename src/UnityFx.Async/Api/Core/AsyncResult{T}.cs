// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

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

		#region async/await

#if UNITYFX_SUPPORT_TAP

		/// <summary>
		/// Provides an object that waits for the completion of <see cref="AsyncResult"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public new struct AsyncAwaiter : INotifyCompletion
		{
			private readonly AsyncResult<T> _op;
			private readonly bool _continueOnCapturedContext;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter"/> struct.
			/// </summary>
			public AsyncAwaiter(AsyncResult<T> op, bool continueOnCapturedContext)
			{
				_op = op;
				_continueOnCapturedContext = continueOnCapturedContext;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.IsCompleted;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public T GetResult()
			{
				_op.ThrowIfNonSuccess(false);
				return _op.Result;
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				var syncContext = _continueOnCapturedContext ? SynchronizationContext.Current : null;
				_op.SetContinuationForAwait(continuation, syncContext);
			}
		}

		/// <summary>
		/// Provides an awaitable object that allows for configured awaits on <see cref="AsyncResult{T}"/>. This type is intended for compiler use only.
		/// </summary>
		public new struct ConfiguredAsyncAwaitable
		{
			private readonly AsyncAwaiter _awaiter;

			/// <summary>
			/// Initializes a new instance of the <see cref="ConfiguredAsyncAwaitable"/> struct.
			/// </summary>
			public ConfiguredAsyncAwaitable(AsyncResult<T> op, bool continueOnCapturedContext)
			{
				_awaiter = new AsyncAwaiter(op, continueOnCapturedContext);
			}

			/// <summary>
			/// Returns the awaiter.
			/// </summary>
			public AsyncAwaiter GetAwaiter()
			{
				return _awaiter;
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		public new AsyncAwaiter GetAwaiter()
		{
			return new AsyncAwaiter(this, true);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public new ConfiguredAsyncAwaitable ConfigureAwait(bool continueOnCapturedContext)
		{
			return new ConfiguredAsyncAwaitable(this, continueOnCapturedContext);
		}

#endif

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public T Result
		{
			get
			{
				if (!IsCompleted)
				{
					throw new InvalidOperationException(Constants.ErrorResultNotAvailable);
				}

				ThrowIfNonSuccess(true);
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
