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
		/// <returns>Returns a <see cref="IAsyncOperation"/> instance that will complete when the source operation have completed.</returns>
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
				op.completed += o => result.TrySetCompleted();
#else
				AddAsyncToUpdateList(op, () => result.TrySetCompleted());
#endif
				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>Returns a <see cref="IAsyncOperation{T}"/> instance that will complete when the source operation have completed.</returns>
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
				op.completed += o => result.TrySetResult(op.asset as T);
#else
				AddAsyncToUpdateList(op, () => result.TrySetResult(op.asset as T));
#endif
				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>Returns a <see cref="IAsyncOperation{T}"/> instance that will complete when the source operation have completed.</returns>
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
				op.completed += o => result.TrySetResult(op.asset as T);
#else
				AddAsyncToUpdateList(op, () => result.TrySetResult(op.asset as T));
#endif
				return result;
			}
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

		private static void AddAsyncToUpdateList(AsyncOperation op, Action cb)
		{
			if (_asyncUpdater == null)
			{
				_asyncUpdater = AsyncUtility.GetRootGo().AddComponent<AsyncOperationStatusUpdater>();
			}

			_asyncUpdater.AddAsync(op, cb);
		}

		#endregion
	}
}
