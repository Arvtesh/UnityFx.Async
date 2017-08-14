// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Async
{
	/// <summary>
	/// Provides support for creating and scheduling asynchronous operations.
	/// </summary>
	public struct AsyncFactory
	{
		#region data

		private readonly object _scheduler;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncFactory"/> struct.
		/// </summary>
		public AsyncFactory(AsyncScheduler scheduler)
		{
			_scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncFactory"/> struct.
		/// </summary>
		public AsyncFactory(MonoBehaviour b)
		{
			_scheduler = b ?? throw new ArgumentNullException(nameof(b));
		}

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied time interval.
		/// </summary>
		public IAsyncOperation FromDelay(TimeSpan delay)
		{
			var result = new AsyncDelay(delay);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied time interval.
		/// </summary>
		public IAsyncOperation FromDelay(TimeSpan delay, CancellationToken cancellationToken)
		{
			var result = new AsyncDelay(delay, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="Coroutine"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="FromEnumerator(IEnumerator)"/>
		/// <seealso cref="FromAsyncOperation(AsyncOperation)"/>
		public IAsyncOperation FromCoroutine(YieldInstruction op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			IEnumerator CoroutineEnum(YieldInstruction c)
			{
				yield return c;
			}

			var result = new AsyncEnumeratorWrapper(CoroutineEnum(op));
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="FromCoroutine(YieldInstruction)"/>
		/// <seealso cref="FromAsyncOperation(AsyncOperation)"/>
		public IAsyncOperation FromEnumerator(IEnumerator op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			var result = new AsyncEnumeratorWrapper(op);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> from the supplied <see cref="IEnumerator"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="FromCoroutine(YieldInstruction)"/>
		/// <seealso cref="FromAsyncOperation(AsyncOperation)"/>
		public IAsyncOperation FromEnumerator(IEnumerator op, CancellationToken cancellationToken)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			var result = new AsyncEnumeratorWrapper(op, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="AsyncOperation"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="FromCoroutine(YieldInstruction)"/>
		/// <seealso cref="FromEnumerator(IEnumerator)"/>
		public IAsyncOperation FromAsyncOperation(AsyncOperation op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			var result = new AsyncOperationWrapper<object>(op);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="AsyncOperation"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="FromCoroutine(YieldInstruction)"/>
		/// <seealso cref="FromEnumerator(IEnumerator)"/>
		public IAsyncOperation<T> FromAsyncOperation<T>(AsyncOperation op) where T : class
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			var result = new AsyncOperationWrapper<T>(op);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is <c>null</c>.</exception>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest)"/>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest, Func{UnityWebRequest, T})"/>
		/// <seealso cref="FromAsyncOperation(AsyncOperation)"/>
		public IAsyncOperation FromWebRequest(UnityWebRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var result = new UnityWebRequestWrapper<UnityEngine.Object>(request);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is <c>null</c>.</exception>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest)"/>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest, Func{UnityWebRequest, T})"/>
		/// <seealso cref="FromAsyncOperation(AsyncOperation)"/>
		public IAsyncOperation FromWebRequest(UnityWebRequest request, CancellationToken cancellationToken)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var result = new UnityWebRequestWrapper<UnityEngine.Object>(request, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation{T}"/> for the supplied <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is <c>null</c>.</exception>
		/// <seealso cref="FromWebRequest(UnityWebRequest)"/>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest, Func{UnityWebRequest, T})"/>
		/// <seealso cref="FromAsyncOperation{T}(AsyncOperation)"/>
		public IAsyncOperation<T> FromWebRequest<T>(UnityWebRequest request) where T : UnityEngine.Object
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var result = new UnityWebRequestWrapper<T>(request);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation{T}"/> for the supplied <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is <c>null</c>.</exception>
		/// <seealso cref="FromWebRequest(UnityWebRequest)"/>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest, Func{UnityWebRequest, T})"/>
		/// <seealso cref="FromAsyncOperation{T}(AsyncOperation)"/>
		public IAsyncOperation<T> FromWebRequest<T>(UnityWebRequest request, CancellationToken cancellationToken) where T : UnityEngine.Object
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var result = new UnityWebRequestWrapper<T>(request, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation{T}"/> for the supplied <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is <c>null</c>.</exception>
		/// <seealso cref="FromWebRequest(UnityWebRequest)"/>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest)"/>
		/// <seealso cref="FromAsyncOperation{T}(AsyncOperation)"/>
		public IAsyncOperation<T> FromWebRequest<T>(UnityWebRequest request, Func<UnityWebRequest, T> resultProcessor) where T : UnityEngine.Object
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if (resultProcessor == null)
			{
				throw new ArgumentNullException(nameof(resultProcessor));
			}

			var result = new UnityWebRequestWrapper<T>(request, resultProcessor);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation{T}"/> for the supplied <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> is <c>null</c>.</exception>
		/// <seealso cref="FromWebRequest(UnityWebRequest)"/>
		/// <seealso cref="FromWebRequest{T}(UnityWebRequest)"/>
		/// <seealso cref="FromAsyncOperation{T}(AsyncOperation)"/>
		public IAsyncOperation<T> FromWebRequest<T>(UnityWebRequest request, Func<UnityWebRequest, T> resultProcessor, CancellationToken cancellationToken) where T : UnityEngine.Object
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if (resultProcessor == null)
			{
				throw new ArgumentNullException(nameof(resultProcessor));
			}

			var result = new UnityWebRequestWrapper<T>(request, resultProcessor, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates an instance of <see cref="IAsyncOperation"/> for the supplied <see cref="IAsyncResult"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		/// <seealso cref="FromCoroutine(YieldInstruction)"/>
		/// <seealso cref="FromEnumerator(IEnumerator)"/>
		public IAsyncOperation FromAsyncResult(IAsyncResult op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			var result = new AsyncResultWrapper(op);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Starts a new operation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		/// <seealso cref="FromUpdateCallback{T}(Action{IAsyncOperationController{T}})"/>
		public IAsyncOperation FromUpdateCallback(Action<IAsyncOperationController> updateCallback)
		{
			if (updateCallback == null)
			{
				throw new ArgumentNullException(nameof(updateCallback));
			}

			var result = new AsyncUpdateCallback(updateCallback);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Starts a new operation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		/// <seealso cref="FromUpdateCallback{T}(Action{IAsyncOperationController{T}})"/>
		public IAsyncOperation FromUpdateCallback(Action<IAsyncOperationController> updateCallback, CancellationToken cancellationToken)
		{
			if (updateCallback == null)
			{
				throw new ArgumentNullException(nameof(updateCallback));
			}

			var result = new AsyncUpdateCallback(updateCallback, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Starts a new operation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		/// <seealso cref="FromUpdateCallback(Action{IAsyncOperationController})"/>
		public IAsyncOperation<T> FromUpdateCallback<T>(Action<IAsyncOperationController<T>> updateCallback)
		{
			if (updateCallback == null)
			{
				throw new ArgumentNullException(nameof(updateCallback));
			}

			var result = new AsyncUpdateCallback<T>(updateCallback);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Starts a new operation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="updateCallback"/> is <c>null</c>.</exception>
		/// <seealso cref="FromUpdateCallback(Action{IAsyncOperationController})"/>
		public IAsyncOperation<T> FromUpdateCallback<T>(Action<IAsyncOperationController<T>> updateCallback, CancellationToken cancellationToken)
		{
			if (updateCallback == null)
			{
				throw new ArgumentNullException(nameof(updateCallback));
			}

			var result = new AsyncUpdateCallback<T>(updateCallback, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation WhenAll(params IAsyncResult[] ops)
		{
			return WhenAll(ops, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation WhenAll(IAsyncResult[] ops, AsyncContinuationOptions options)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll(ops, options);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation WhenAll(IAsyncResult[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll(ops, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation{T}"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation<T[]> WhenAll<T>(params IAsyncOperation<T>[] ops)
		{
			return WhenAll(ops, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation{T}"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, AsyncContinuationOptions options)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll<T>(ops, options);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation{T}"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll<T>(ops, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation WhenAny(params IAsyncResult[] ops)
		{
			return WhenAny(ops, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation WhenAny(IAsyncResult[] ops, AsyncContinuationOptions options)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny(ops, options);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation WhenAny(IAsyncResult[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny(ops, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation<T> WhenAny<T>(params IAsyncOperation<T>[] ops)
		{
			return WhenAny(ops, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, AsyncContinuationOptions options)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny<T>(ops, options);
			StartCoroutine(result);
			return result;
		}

#if UNITYFX_NET46
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], AsyncContinuationOptions)"/>
		public IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options = AsyncContinuationOptions.None)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny<T>(ops, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation ContinueWhen(IAsyncOperation op, Func<IAsyncOperation, IAsyncResult> continuationFactory)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation, IAsyncResult, object>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation ContinueWhen<T>(IAsyncOperation<T> op, Func<IAsyncOperation<T>, IAsyncResult> continuationFactory)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation<T>, IAsyncResult, object>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<TResult>(IAsyncOperation op, Func<IAsyncOperation, IAsyncOperation<TResult>> continuationFactory)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation, IAsyncOperation<TResult>, TResult>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(IAsyncOperation<T> op, Func<IAsyncOperation<T>, IAsyncOperation<TResult>> continuationFactory)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation<T>, IAsyncOperation<TResult>, TResult>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<UnityEngine.Object> ContinueWhen(IAsyncOperation op, Func<IAsyncOperation, AsyncOperation> continuationFactory)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation, AsyncOperation, UnityEngine.Object>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<TResult>(IAsyncOperation op, Func<IAsyncOperation, AsyncOperation> continuationFactory) where TResult : UnityEngine.Object
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation, AsyncOperation, TResult>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<UnityEngine.Object> ContinueWhen<T>(IAsyncOperation<T> op, Func<IAsyncOperation<T>, AsyncOperation> continuationFactory)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation<T>, AsyncOperation, UnityEngine.Object>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(IAsyncOperation<T> op, Func<IAsyncOperation<T>, AsyncOperation> continuationFactory) where TResult : UnityEngine.Object
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<IAsyncOperation<T>, AsyncOperation, TResult>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		#endregion

		#region implementation

		private void StartCoroutine(IEnumerator op)
		{
			if (_scheduler is MonoBehaviour b)
			{
				b.StartCoroutine(op);
			}
			else if (_scheduler is AsyncScheduler s)
			{
				s.QueueCoroutine(op);
			}
			else
			{
				AsyncRunnerBehaviour.Instance.StartCoroutine(op);
			}
		}

		#endregion
	}
}
