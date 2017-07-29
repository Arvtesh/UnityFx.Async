// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

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

#if !UNITYFX_NET35
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

#if !UNITYFX_NET35
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

#if !UNITYFX_NET35
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

#if !UNITYFX_NET35
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

#if !UNITYFX_NET35
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

#if !UNITYFX_NET35
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

#if !UNITYFX_NET35
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
		/// <seealso cref="ContinueWhen{T, U}(T, Func{T, IAsyncOperation{U}})"/>
		/// <seealso cref="ContinueWhen{T}(T, Func{T, AsyncOperation})"/>
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

			var result = new AsyncResultContinuation<T, IAsyncResult, object>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		/// <seealso cref="ContinueWhen{T}(T, Func{T, IAsyncResult})"/>
		/// <seealso cref="ContinueWhen{T}(T, Func{T, AsyncOperation})"/>
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

			var result = new AsyncResultContinuation<T, IAsyncOperation<TResult>, TResult>(op, continuationFactory);
			StartCoroutine(result);
			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncResult"/> completes.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> or <paramref name="continuationFactory"/> is <c>null</c>.</exception>
		/// <seealso cref="ContinueWhen{T}(T, Func{T, IAsyncResult})"/>
		/// <seealso cref="ContinueWhen{T, U}(T, Func{T, IAsyncOperation{U}})"/>
		public IAsyncOperation<UnityEngine.Object> ContinueWhen<T>(T op, Func<T, AsyncOperation> continuationFactory) where T : class, IAsyncOperation
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (continuationFactory == null)
			{
				throw new ArgumentNullException(nameof(continuationFactory));
			}

			var result = new AsyncOperationContinuation<T, AsyncOperation>(op, continuationFactory);
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
				s.QueueEnum(op);
			}
		}

		#endregion
	}
}
