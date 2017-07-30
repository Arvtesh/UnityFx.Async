// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	partial class AsyncResult
	{
		#region data

		private static IAsyncOperation _completed;
		private static IAsyncOperation _canceled;
		private static AsyncFactory _factory;

		#endregion

		#region interface

		/// <summary>
		/// Returns an <see cref="IAsyncOperation"/> instance that is completed successfully. Read only.
		/// </summary>
		public static IAsyncOperation Completed
		{
			get
			{
				if (_completed == null)
				{
					_completed = new AsyncResult(null, AsyncOperationStatus.Success);
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
					_canceled = new AsyncResult(null, AsyncOperationStatus.Canceled);
				}

				return _canceled;
			}
		}

		/// <summary>
		/// Returns default factory for <see cref="IAsyncOperation"/> instances. Read only.
		/// </summary>
		public static AsyncFactory Factory => _factory;

		/// <summary>
		/// Returns a canceled <see cref="IAsyncOperation"/> instance.
		/// </summary>
		public static IAsyncOperation FromCanceled() => new AsyncResult(null, AsyncOperationStatus.Canceled);

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
		public static IAsyncOperation<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) => new AsyncResult<TResult>(null, cancellationToken, AsyncOperationStatus.Canceled);
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
		public static IAsyncOperation<TResult> FromResult<TResult>(TResult result) => new AsyncResult<TResult>(null, result);

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

		#endregion

		#region internal

		internal static bool IsCompletedWithOptions(IAsyncResult op, AsyncContinuationOptions options)
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

		internal static object GetOperationResult(object op)
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
	}
}
