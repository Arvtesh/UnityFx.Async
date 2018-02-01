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

#if !NET35
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

#if !NET35
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

#if !NET35
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

#if !NET35
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

#if !NET35
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

#if !NET35
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

#if !NET35
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
		/// <seealso cref="WhenAny(IAsyncResult[])"/>
		/// <seealso cref="WhenAll{T}(IAsyncOperation{T}[])"/>
		public IAsyncOperation WhenAll(params IAsyncResult[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll(ops);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny(IAsyncResult[], CancellationToken)"/>
		/// <seealso cref="WhenAll(IAsyncResult[])"/>
		public IAsyncOperation WhenAll(IAsyncResult[] ops, CancellationToken cancellationToken)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll(ops, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation{T}"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny{T}(IAsyncOperation{T}[])"/>
		/// <seealso cref="WhenAll(IAsyncResult[])"/>
		public IAsyncOperation<T[]> WhenAll<T>(params IAsyncOperation<T>[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll<T>(ops);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation{T}"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAny{T}(IAsyncOperation{T}[], CancellationToken)"/>
		/// <seealso cref="WhenAll{T}(IAsyncOperation{T}[])"/>
		public IAsyncOperation<T[]> WhenAll<T>(IAsyncOperation<T>[] ops, CancellationToken cancellationToken)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAll<T>(ops, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[])"/>
		/// <seealso cref="WhenAny{T}(IAsyncOperation{T}[])"/>
		public IAsyncOperation WhenAny(params IAsyncResult[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny(ops);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll(IAsyncResult[], CancellationToken)"/>
		/// <seealso cref="WhenAny(IAsyncResult[])"/>
		public IAsyncOperation WhenAny(IAsyncResult[] ops, CancellationToken cancellationToken)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny(ops, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll{T}(IAsyncOperation{T}[])"/>
		/// <seealso cref="WhenAny(IAsyncResult[])"/>
		public IAsyncOperation<T> WhenAny<T>(params IAsyncOperation<T>[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny<T>(ops);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a new <see cref="IAsyncOperation"/> instance that finishes when all of the specified operations finish.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="ops"/> is <c>null</c>.</exception>
		/// <seealso cref="WhenAll{T}(IAsyncOperation{T}[], CancellationToken)"/>
		/// <seealso cref="WhenAny{T}(IAsyncOperation{T}[])"/>
		public IAsyncOperation<T> WhenAny<T>(IAsyncOperation<T>[] ops, CancellationToken cancellationToken)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var result = new AsyncOperationWhenAny<T>(ops, cancellationToken);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation ContinueWhen<T>(T op, Func<T, IAsyncResult> continuationFactory) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncResult, object>(op, continuationFactory, AsyncContinuationOptions.None);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation ContinueWhen<T>(T op, Func<T, IAsyncResult> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncResult, object>(op, continuationFactory, cancellationToken, AsyncContinuationOptions.None);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation ContinueWhen<T>(T op, Func<T, IAsyncResult> continuationFactory, AsyncContinuationOptions options) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncResult, object>(op, continuationFactory, options);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation ContinueWhen<T>(T op, Func<T, IAsyncResult> continuationFactory, CancellationToken cancellationToken, AsyncContinuationOptions options) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncResult, object>(op, continuationFactory, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, IAsyncOperation<TResult>> continuationFactory) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncOperation<TResult>, TResult>(op, continuationFactory, AsyncContinuationOptions.None);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, IAsyncOperation<TResult>> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncOperation<TResult>, TResult>(op, continuationFactory, cancellationToken, AsyncContinuationOptions.None);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, IAsyncOperation<TResult>> continuationFactory, AsyncContinuationOptions options) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncOperation<TResult>, TResult>(op, continuationFactory, options);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, IAsyncOperation<TResult>> continuationFactory, CancellationToken cancellationToken, AsyncContinuationOptions options) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, IAsyncOperation<TResult>, TResult>(op, continuationFactory, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, AsyncOperation> continuationFactory) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, AsyncOperation, TResult>(op, continuationFactory, AsyncContinuationOptions.None);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, AsyncOperation, TResult>(op, continuationFactory, cancellationToken, AsyncContinuationOptions.None);
			StartCoroutine(result);
			return result;
		}
#endif

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, AsyncOperation> continuationFactory, AsyncContinuationOptions options) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, AsyncOperation, TResult>(op, continuationFactory, options);
			StartCoroutine(result);
			return result;
		}

#if !NET35
		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		public IAsyncOperation<TResult> ContinueWhen<T, TResult>(T op, Func<T, AsyncOperation> continuationFactory, CancellationToken cancellationToken, AsyncContinuationOptions options) where T : class, IAsyncOperation where TResult : UnityEngine.Object
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncContinuation<T, AsyncOperation, TResult>(op, continuationFactory, cancellationToken, options);
			StartCoroutine(result);
			return result;
		}
#endif

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
