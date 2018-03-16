// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Extensions for <see cref="AsyncOperation"/> class.
	/// </summary>
	public static class AsyncOperationExtensions
	{
		#region data

		private static AsyncOperationStatusUpdater _asyncUpdater;

		#endregion

		#region interface

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static IAsyncOperation ToAsync(this AsyncOperation op)
		{
			if (op.isDone)
			{
				return AsyncResult.CompletedOperation;
			}
			else
			{
				var result = new AsyncCompletionSource(AsyncOperationStatus.Running, op);

#if UNITY_2017_2_OR_NEWER

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				op.completed += o => result.TrySetCompleted();

#else

				RegisterCompletionCallback(op, () => result.TrySetCompleted());

#endif
				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static IAsyncOperation<T> ToAsync<T>(this ResourceRequest op) where T : UnityEngine.Object
		{
			if (op.isDone)
			{
				return AsyncResult.FromResult(op.asset as T);
			}
			else
			{
				var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running, op);

#if UNITY_2017_2_OR_NEWER

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				op.completed += o => result.TrySetResult(op.asset as T);

#else

				RegisterCompletionCallback(op, () => result.TrySetResult(op.asset as T));

#endif
				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static IAsyncOperation<T> ToAsync<T>(this AssetBundleRequest op) where T : UnityEngine.Object
		{
			if (op.isDone)
			{
				return AsyncResult.FromResult(op.asset as T);
			}
			else
			{
				var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running, op);

#if UNITY_2017_2_OR_NEWER

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				op.completed += o => result.TrySetResult(op.asset as T);

#else

				RegisterCompletionCallback(op, () => result.TrySetResult(op.asset as T));

#endif
				return result;
			}
		}

		/// <summary>
		/// Register a completion callback for the specified <see cref="AsyncOperation"/> instance.
		/// </summary>
		/// <param name="op">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="op"/> has completed.</param>
		public static void RegisterCompletionCallback(this AsyncOperation op, Action completionCallback)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}

			if (completionCallback == null)
			{
				throw new ArgumentNullException("completionCallback");
			}

			if (_asyncUpdater == null)
			{
				_asyncUpdater = AsyncUtility.GetRootGo().AddComponent<AsyncOperationStatusUpdater>();
			}

			_asyncUpdater.AddAsync(op, completionCallback);
		}

		#endregion

		#region implementation

		private class AsyncOperationStatusUpdater : MonoBehaviour
		{
			private readonly Dictionary<AsyncOperation, Action> _ops = new Dictionary<AsyncOperation, Action>();
			private readonly List<AsyncOperation> _opsToRemove = new List<AsyncOperation>();

			public void AddAsync(AsyncOperation op, Action cb)
			{
				_ops.Add(op, cb);
			}

			private void Update()
			{
				if (_ops.Count > 0)
				{
					_opsToRemove.Clear();

					foreach (var item in _ops)
					{
						if (item.Key.isDone)
						{
							item.Value();
							_opsToRemove.Add(item.Key);
						}
					}

					foreach (var item in _opsToRemove)
					{
						_ops.Remove(item);
					}
				}
			}
		}

		#endregion
	}
}
