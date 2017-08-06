// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/>.
	/// </summary>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	/// <seealso cref="IAsyncResult"/>
	[DebuggerDisplay("Status = {Status}, Progress={Progress}")]
	public class AsyncResult : IAsyncOperationController, IAsyncContinuationContainer, IAsyncOperation, IEnumerator
	{
		#region data

		private const string _errorOpStatus = "Invalid operation status.";
		private const string _errorOpCompleted = "The operation is already completed.";
		private const string _errorOpFaulted = "The operation result is not available.";

		private static IAsyncOperation _completed;
		private static IAsyncOperation _canceled;
		private static AsyncFactory _factory;

		private readonly bool _completedSynchronously;
		private readonly object _asyncState;

#if !UNITYFX_NET35
		private CancellationToken _cancellationToken;
#endif
		private EventWaitHandle _waitHandle;
		private Exception _exception;
		private object _current;

		/*
		 * Continuation(s) to run when the operation is finished.
		 */
		private Action _continuation;

		/*
		 * Operation progress in range [0,1]. Values < 0 mean the object is disposed.
		 */
		private float _progress;

		/*
		 * Operation status. The field type is integer (not enum) because it is used with interlocked methods.
		 *
		 * Possible values (should match AsyncOperationStatus constants):
		 * -1: Initialized (initialized but has not yet been scheduled);
		 *  0: Running;
		 *  1: Success (completed without exceptions);
		 *  2: Faulted (completed with exceptions);
		 *  3: Canceled.
		 *
		 * Do not modify this field outside class constructor manually; use TrySetStatus instead.
		 */
		private volatile int _status;

		#endregion

		#region interface

		internal const int StatusInitialized = -1;
		internal const int StatusRunning = 0;
		internal const int StatusCompleted = 1;
		internal const int StatusFaulted = 2;
		internal const int StatusCanceled = 3;

		/// <summary>
		/// Number of milliseconds to wait until the next <see cref="IAsyncResult.IsCompleted"/> check.
		/// </summary>
		public const int WaitSleepTimeout = 32;

		/// <summary>
		/// Returns <c>true</c> if the operation is disposed; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsDisposed => _progress < 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		public AsyncResult()
			: this(null, StatusInitialized)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(object asyncState)
			: this(asyncState, StatusInitialized)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="status">Initial operation status.</param>
		public AsyncResult(object asyncState, AsyncOperationStatus status)
			: this(asyncState, (int)status)
		{
		}

#if !UNITYFX_NET35
		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public AsyncResult(object asyncState, CancellationToken cancellationToken)
			: this(asyncState, cancellationToken.IsCancellationRequested ? StatusCanceled : StatusInitialized)
		{
			_cancellationToken = cancellationToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <param name="status">Initial operation status.</param>
		public AsyncResult(object asyncState, CancellationToken cancellationToken, AsyncOperationStatus status)
			: this(asyncState, cancellationToken.IsCancellationRequested ? StatusCanceled : (int)status)
		{
			_cancellationToken = cancellationToken;
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="e">Exception instance.</param>
		public AsyncResult(object asyncState, Exception e)
		{
			_completedSynchronously = true;
			_asyncState = asyncState;
			_status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;
			_exception = e;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <param name="status">Initial operation status.</param>
		internal AsyncResult(object asyncState, int status)
		{
			if (status < StatusInitialized || status > StatusCanceled)
			{
				throw new ArgumentException(_errorOpStatus, nameof(status));
			}

			_completedSynchronously = status > StatusRunning;
			_asyncState = asyncState;
			_status = status;
		}

		/// <summary>
		/// Sets the value of <see cref="Current"/> property. Use with care.
		/// </summary>
		protected void SetCurrent(object current)
		{
			_current = current;
		}

		/// <summary>
		/// Finished the operation (if it is not already finished). Do not use this method unless absolutely needed.
		/// </summary>
		/// <seealso cref="FireCompleted"/>
		protected bool TrySetStatus(int newStatus)
		{
			if (_status < StatusCompleted)
			{
				if (Interlocked.CompareExchange(ref _status, newStatus, StatusInitialized) == StatusInitialized ||
					Interlocked.CompareExchange(ref _status, newStatus, StatusRunning) == StatusRunning)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Triggers completed event (if any).
		/// </summary>
		protected void FireCompleted()
		{
			try
			{
				OnCompleted();
			}
			finally
			{
				_waitHandle?.Set();

				// TODO: synchronization needed
				_continuation?.Invoke();
				_continuation = null;
			}
		}

		/// <summary>
		/// Returns an exception for the operation instance. Never returns <c>null</c>.
		/// </summary>
		protected Exception GetExceptionSafe()
		{
			if (_exception == null)
			{
				if (_status == StatusCanceled)
				{
					return new OperationCanceledException();
				}
				else
				{
					return new Exception();
				}
			}

			return _exception;
		}

		/// <summary>
		/// Throws exception if the operation has failed.
		/// </summary>
		protected void ThrowIfFaulted()
		{
			if (_status > StatusCompleted)
			{
				throw new InvalidOperationException(_errorOpFaulted, _exception);
			}
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> if this operation has been disposed.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if (_progress < 0)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion

		#region virtual interface

		/// <summary>
		/// Updates the operation state. Called by <see cref="MoveNext"/>. Default implementation does nothing.
		/// </summary>
		/// <remarks>
		/// Do not reference public class methods and properties in this method (except <see cref="SetCompleted"/>)
		/// because their implementation may reference <see cref="MoveNext"/> and cause endless recursion.
		/// </remarks>
		/// <seealso cref="OnCompleted()"/>
		protected virtual void OnUpdate()
		{
		}

		/// <summary>
		/// Called when the operation has completed (either successfully or not). Default implementation does nothing.
		/// </summary>
		/// <seealso cref="OnUpdate()"/>
		protected virtual void OnCompleted()
		{
		}

		#endregion

		#region static interface

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that is completed successfully. Read only.
		/// </summary>
		public static IAsyncOperation Completed
		{
			get
			{
				if (_completed == null)
				{
					_completed = new AsyncResult(null, StatusCompleted);
				}

				return _completed;
			}
		}

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that is canceled. Read only.
		/// </summary>
		public static IAsyncOperation Canceled
		{
			get
			{
				if (_canceled == null)
				{
					_canceled = new AsyncResult(null, StatusCanceled);
				}

				return _canceled;
			}
		}

		/// <summary>
		/// Returns default factory for <see cref="IAsyncOperation"/> instances. Read only.
		/// </summary>
		public static AsyncFactory Factory => _factory;

#if !UNITYFX_NET35
		/// <summary>
		/// Returns a canceled <see cref="IAsyncOperation"/> instance.
		/// </summary>
		public static IAsyncOperation FromCanceled(CancellationToken cancellationToken) => new AsyncResult(null, cancellationToken, AsyncOperationStatus.Canceled);
#endif

		/// <summary>
		/// Returns a canceled <see cref="IAsyncOperation"/> instance.
		/// </summary>
		public static IAsyncOperation<T> FromCanceled<T>() => new AsyncResult<T>(null, AsyncOperationStatus.Canceled);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns a canceled <see cref="IAsyncOperation"/> instance.
		/// </summary>
		public static IAsyncOperation<T> FromCanceled<T>(CancellationToken cancellationToken) => new AsyncResult<T>(null, cancellationToken, AsyncOperationStatus.Canceled);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance completed with an exception.
		/// </summary>
		public static IAsyncOperation FromException(Exception e) => new AsyncResult(null, e);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance completed with an exception.
		/// </summary>
		public static IAsyncOperation<T> FromException<T>(Exception e) => new AsyncResult<T>(null, e);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that is completed with the specified result.
		/// </summary>
		public static IAsyncOperation<T> FromResult<T>(T result) => new AsyncResult<T>(null, result);

		/// <summary>
		/// Returns an instance of <see cref="IAsyncOperation"/> that is finished in the specified time interval.
		/// </summary>
		public static IAsyncOperation Delay(TimeSpan delay) => _factory.FromDelay(delay);

		/// <summary>
		/// Returns an instance of <see cref="IAsyncOperation"/> that is finished in the specified time interval.
		/// </summary>
		public static IAsyncOperation Delay(TimeSpan delay, MonoBehaviour b) => new AsyncFactory(b).FromDelay(delay);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an instance of <see cref="IAsyncOperation"/> that is finished in the specified time interval.
		/// </summary>
		public static IAsyncOperation Delay(TimeSpan delay, CancellationToken cancellationToken) => _factory.FromDelay(delay, cancellationToken);

		/// <summary>
		/// Returns an instance of <see cref="IAsyncOperation"/> that is finished in the specified time interval.
		/// </summary>
		public static IAsyncOperation Delay(TimeSpan delay, CancellationToken cancellationToken, MonoBehaviour b) => new AsyncFactory(b).FromDelay(delay, cancellationToken);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified operation is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromEnumerator(IEnumerator op) => _factory.FromEnumerator(op);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified operation is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromEnumerator(IEnumerator op, MonoBehaviour b) => new AsyncFactory(b).FromEnumerator(op);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified operation is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromEnumerator(IEnumerator op, CancellationToken cancellationToken) => _factory.FromEnumerator(op, cancellationToken);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified operation is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromEnumerator(IEnumerator op, CancellationToken cancellationToken, MonoBehaviour b) => new AsyncFactory(b).FromEnumerator(op, cancellationToken);
#endif

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="YieldInstruction"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromCoroutine(YieldInstruction op) => _factory.FromCoroutine(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="YieldInstruction"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromCoroutine(YieldInstruction op, MonoBehaviour b) => new AsyncFactory(b).FromCoroutine(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="AsyncOperation"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromAsyncOperation(AsyncOperation op) => _factory.FromAsyncOperation(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="AsyncOperation"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromAsyncOperation(AsyncOperation op, MonoBehaviour b) => new AsyncFactory(b).FromAsyncOperation(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="AsyncOperation"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation<T> FromAsyncOperation<T>(AsyncOperation op) where T : class => _factory.FromAsyncOperation<T>(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="AsyncOperation"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation<T> FromAsyncOperation<T>(AsyncOperation op, MonoBehaviour b) where T : class => new AsyncFactory(b).FromAsyncOperation<T>(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="IAsyncResult"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromAsyncResult(IAsyncResult op) => _factory.FromAsyncResult(op);

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="IAsyncResult"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromAsyncResult(IAsyncResult op, MonoBehaviour b) => new AsyncFactory(b).FromAsyncResult(op);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromUpdateCallback(Action<IAsyncOperationController> updateCallback) => _factory.FromUpdateCallback(updateCallback);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromUpdateCallback(Action<IAsyncOperationController> updateCallback, MonoBehaviour b) => new AsyncFactory(b).FromUpdateCallback(updateCallback);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromUpdateCallback(Action<IAsyncOperationController> updateCallback, CancellationToken cancellationToken) => _factory.FromUpdateCallback(updateCallback, cancellationToken);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation FromUpdateCallback(Action<IAsyncOperationController> updateCallback, CancellationToken cancellationToken, MonoBehaviour b) => new AsyncFactory(b).FromUpdateCallback(updateCallback, cancellationToken);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation<T> FromUpdateCallback<T>(Action<IAsyncOperationController<T>> updateCallback) => _factory.FromUpdateCallback(updateCallback);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation<T> FromUpdateCallback<T>(Action<IAsyncOperationController<T>> updateCallback, MonoBehaviour b) => new AsyncFactory(b).FromUpdateCallback(updateCallback);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation<T> FromUpdateCallback<T>(Action<IAsyncOperationController<T>> updateCallback, CancellationToken cancellationToken) => _factory.FromUpdateCallback(updateCallback, cancellationToken);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that wraps the specified <see cref="Action{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified action is <c>null</c>.</exception>
		/// <seealso cref="AsyncFactory"/>
		public static IAsyncOperation<T> FromUpdateCallback<T>(Action<IAsyncOperationController<T>> updateCallback, CancellationToken cancellationToken, MonoBehaviour b) => new AsyncFactory(b).FromUpdateCallback(updateCallback, cancellationToken);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAll(params IAsyncResult[] ops) => _factory.WhenAll(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAll(IAsyncResult[] ops, MonoBehaviour b) => new AsyncFactory(b).WhenAll(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAll(IAsyncResult[] ops, AsyncContinuationOptions options) => _factory.WhenAll(ops, options);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAll(IAsyncResult[] ops, AsyncContinuationOptions options, MonoBehaviour b) => new AsyncFactory(b).WhenAll(ops, options);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAll(IAsyncResult[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None) => _factory.WhenAll(ops, cancellationToken, options);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T[]> WhenAll<T>(params IAsyncOperation<T>[] ops) => _factory.WhenAll(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, MonoBehaviour b) => new AsyncFactory(b).WhenAll(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, AsyncContinuationOptions options) => _factory.WhenAll(ops, options);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, AsyncContinuationOptions options, MonoBehaviour b) => new AsyncFactory(b).WhenAll(ops, options);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when all specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None) => _factory.WhenAll(ops, cancellationToken, options);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAny(params IAsyncResult[] ops) => _factory.WhenAny(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAny(IAsyncResult[] ops, MonoBehaviour b) => new AsyncFactory(b).WhenAny(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAny(IAsyncResult[] ops, AsyncContinuationOptions options) => _factory.WhenAny(ops, options);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAny(IAsyncResult[] ops, AsyncContinuationOptions options, MonoBehaviour b) => new AsyncFactory(b).WhenAny(ops, options);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation WhenAny(IAsyncResult[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None) => _factory.WhenAny(ops, cancellationToken, options);
#endif

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T> WhenAny<T>(params IAsyncOperation<T>[] ops) => _factory.WhenAny(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, MonoBehaviour b) => new AsyncFactory(b).WhenAny(ops, AsyncContinuationOptions.None);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, AsyncContinuationOptions options) => _factory.WhenAny(ops, options);

		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, AsyncContinuationOptions options, MonoBehaviour b) => new AsyncFactory(b).WhenAny(ops, options);

#if !UNITYFX_NET35
		/// <summary>
		/// Returns an <see cref="IAsyncOperation{T}"/> instance that finishes when any of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the specified array is <c>null</c>.</exception>
		public static IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None) => _factory.WhenAny(ops, cancellationToken, options);
#endif

		/// <summary>
		/// Helper method for checking completed flag with the specified options.
		/// </summary>
		protected internal static bool IsCompletedWithOptions(IAsyncResult op, AsyncContinuationOptions options)
		{
			if (op.IsCompleted)
			{
				if (options != AsyncContinuationOptions.None && op is IAsyncOperation asyncOp)
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						return (options & AsyncContinuationOptions.OnlyOnSuccess) != 0;
					}
					else if (asyncOp.IsCanceled)
					{
						return (options & AsyncContinuationOptions.OnlyOnCanceled) != 0;
					}
					else
					{
						return (options & AsyncContinuationOptions.OnlyOnFaulted) != 0;
					}
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Helper method that returns a result of the <see cref="AsyncOperation"/> instance passed (if any).
		/// </summary>
		protected internal static object GetOperationResult(AsyncOperation op)
		{
			if (op is ResourceRequest rr)
			{
				return rr.asset;
			}

			if (op is AssetBundleRequest abr)
			{
				return abr.asset;
			}

			if (op is AssetBundleCreateRequest abcr)
			{
				return abcr.assetBundle;
			}

			return null;
		}

		#endregion

		#region IAsyncContinuationContainer

		/// <inheritdoc/>
		public void AddContinuation(Action continuation)
		{
			// TODO: make sure other threads do not interfere
			_continuation += continuation;
		}

		/// <inheritdoc/>
		public void RemoveContinuation(Action continuation)
		{
			// TODO: make sure other threads do not interfere
			_continuation -= continuation;
		}

		#endregion

		#region IAsyncOperationController

		/// <inheritdoc/>
		public void SetProgress(float progress)
		{
			ThrowIfDisposed();

			if (progress < 0)
			{
				_progress = 0;
			}
			else if (progress > 1)
			{
				_progress = 1;
			}
			else
			{
				_progress = progress;
			}
		}

		/// <inheritdoc/>
		public void SetCanceled()
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCanceled))
			{
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetCanceled()
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCanceled))
			{
				FireCompleted();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void SetException(Exception e)
		{
			ThrowIfDisposed();

			var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetException(Exception e)
		{
			ThrowIfDisposed();

			var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				FireCompleted();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void SetCompleted()
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCompleted))
			{
				FireCompleted();
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public bool TrySetCompleted()
		{
			ThrowIfDisposed();

			if (TrySetStatus(StatusCompleted))
			{
				FireCompleted();
				return true;
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public float Progress => _status == StatusInitialized ? 0 : _status > StatusRunning ? 1 : _progress < 0 ? 0 : _progress;

		/// <inheritdoc/>
		public AsyncOperationStatus Status => (AsyncOperationStatus)_status;

		/// <inheritdoc/>
		public Exception Exception => _exception;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => _status == StatusCompleted;

		/// <inheritdoc/>
		public bool IsFaulted => _status > StatusCompleted;

		/// <inheritdoc/>
		public bool IsCanceled => _status == StatusCanceled;

		#endregion

		#region IAsyncResult

		/// <inheritdoc/>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				ThrowIfDisposed();

				if (_waitHandle == null)
				{
					var done = IsCompleted;
					var mre = new ManualResetEvent(done);

					if (Interlocked.CompareExchange(ref _waitHandle, mre, null) != null)
					{
						// Another thread created this object's event; dispose the event we just created.
						mre.Close();
					}
					else if (!done && IsCompleted)
					{
						// We published the event as unset, but the operation has subsequently completed;
						// set the event state properly so that callers do not deadlock.
						_waitHandle.Set();
					}
				}

				return _waitHandle;
			}
		}

		/// <inheritdoc/>
		public object AsyncState => _asyncState;

		/// <inheritdoc/>
		public bool CompletedSynchronously => _completedSynchronously;

		/// <inheritdoc/>
		public bool IsCompleted => _status > StatusRunning;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		public object Current => _current;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (_status > StatusRunning)
			{
				// The operation has completed.
				return false;
			}
			else
			{
				// If this is the first time MoveNext() is called, switch status to Running.
				if (_status == StatusInitialized)
				{
					TrySetStatus(StatusRunning);
				}

				// The operation is pending.
				try
				{
#if UNITYFX_NET35
					OnUpdate();
#else
					if (_cancellationToken.IsCancellationRequested)
					{
						TrySetCanceled();
					}
					else
					{
						OnUpdate();
					}
#endif
				}
				catch (Exception e)
				{
					TrySetException(e);
				}

				return _status == StatusRunning;
			}
		}

		/// <inheritdoc/>
		public void Reset()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			// NOTE: thread safety is not required
			if (this == _completed || this == _canceled)
			{
				// Do not dispose the _completed and _canceled instances because they might be reused.
			}
			else if (_progress >= 0)
			{
				try
				{
					TrySetCanceled();
				}
				finally
				{
					_progress = -1;
					_waitHandle?.Close();

					GC.SuppressFinalize(this);
				}
			}
		}

		#endregion

		#region Object

		/// <inheritdoc/>
		public override string ToString()
		{
			if (_progress < 0)
			{
				return "{Disposed}";
			}
			else if (_exception != null)
			{
				return $"{{Status={Status}, Progress={Progress.ToString(NumberFormatInfo.InvariantInfo)}, Exception={_exception.Message}}}";
			}
			else
			{
				return $"{{Status={Status}, Progress={Progress.ToString(NumberFormatInfo.InvariantInfo)}}}";
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
