// Copyright (c) 2018-2020 Alexander Bogarsukov.
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
	/// <typeparam name="TResult">Type of the operation result value.</typeparam>
	/// <threadsafety static="true" instance="true"/>
	/// <seealso cref="AsyncCompletionSource{T}"/>
	/// <seealso cref="AsyncResult"/>
#if !NET35
	[AsyncMethodBuilder(typeof(CompilerServices.AsyncResultMethodBuilder<>))]
#endif
	public class AsyncResult<TResult> : AsyncResult, IAsyncOperation<TResult>
	{
		#region data

		private TResult _result;

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
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncResult(AsyncCreationOptions options)
			: base(options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncCreationOptions options, object asyncState)
			: base(options, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: base(options, asyncCallback, asyncState)
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
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncOperationStatus status, object asyncState)
			: base(status, asyncState)
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
		/// <param name="status">Status value of the operation.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCreationOptions options)
			: base(status, options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="status">Status value of the operation.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCreationOptions options, object asyncState)
			: base(status, options, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		/// <param name="status">Status value of the operation.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: base(status, options, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class. For internal use only.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		internal AsyncResult(Exception exception, object asyncState)
			: base(exception, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class. For internal use only.
		/// </summary>
		/// <param name="result">Result value.</param>
		/// <param name="asyncState">User-defined data to assosiate with the operation.</param>
		internal AsyncResult(TResult result, object asyncState)
			: base(AsyncOperationStatus.RanToCompletion, asyncState)
		{
			_result = result;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetResult(TResult, bool)"/>
		protected internal bool TrySetResult(TResult result)
		{
			return TrySetResult(result, false);
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The operation result.</param>
		/// <param name="completedSynchronously">Value of the <see cref="IAsyncResult.CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetResult(TResult)"/>
		protected internal bool TrySetResult(TResult result, bool completedSynchronously)
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

		/// <summary>
		/// Copies state of the specified operation.
		/// </summary>
		internal new void CopyCompletionState(IAsyncOperation patternOp, bool completedSynchronously)
		{
			if (!TryCopyCompletionState(patternOp, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

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
		internal new bool TryCopyCompletionState(IAsyncOperation patternOp, bool completedSynchronously)
		{
			if (patternOp.IsCompletedSuccessfully)
			{
				if (patternOp is IAsyncOperation<TResult> op)
				{
					return TrySetResult(op.Result, completedSynchronously);
				}
				else
				{
					return TrySetCompleted(completedSynchronously);
				}
			}
			else if (patternOp.IsCompleted)
			{
				return TrySetException(patternOp.Exception, completedSynchronously);
			}

			return false;
		}

		/// <summary>
		/// Attemts to copy state of the specified operation.
		/// </summary>
		internal bool TryCopyCompletionState(IAsyncOperation<TResult> patternOp, bool completedSynchronously)
		{
			if (patternOp.IsCompletedSuccessfully)
			{
				return TrySetResult(patternOp.Result, completedSynchronously);
			}
			else if (patternOp.IsCompleted)
			{
				return TrySetException(patternOp.Exception, completedSynchronously);
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <summary>
		/// Gets the operation result value.
		/// </summary>
		/// <value>Result of the operation.</value>
		/// <exception cref="InvalidOperationException">Thrown if the property is accessed before operation is completed.</exception>
		public TResult Result
		{
			get
			{
				if (!IsCompleted)
				{
					throw new InvalidOperationException(Messages.FormatError_OperationResultIsNotAvailable());
				}

				ThrowIfNonSuccess();
				return _result;
			}
		}

		#endregion

		#region IObservable

#if !NET35

		/// <summary>
		/// Notifies the provider that an observer is to receive notifications.
		/// </summary>
		/// <param name="observer">The object that is to receive notifications.</param>
		/// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
		public IDisposable Subscribe(IObserver<TResult> observer)
		{
			ThrowIfDisposed();

			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			var result = new ObservableSubscription<TResult>(this, observer);
			AddCompletionCallback(result, null);
			return result;
		}

#endif

		#endregion
	}
}
