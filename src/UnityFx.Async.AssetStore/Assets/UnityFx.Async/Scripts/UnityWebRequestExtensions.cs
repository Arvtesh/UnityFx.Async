// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Async
{
	/// <summary>
	/// Extensions for <see cref="UnityWebRequest"/> class.
	/// </summary>
	public static class UnityWebRequestExtensions
	{
		#region data

		private static RequestStatusUpdater _asyncUpdater;

		#endregion

		#region interface

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult ToAsync(this UnityWebRequest request)
		{
			return WebRequestResult<object>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<T> ToAsync<T>(this UnityWebRequest request) where T : class
		{
			return WebRequestResult<T>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<AssetBundle> ToAsyncAssetBundle(this UnityWebRequest request)
		{
			return WebRequestResult<AssetBundle>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<Texture2D> ToAsyncTexture(this UnityWebRequest request)
		{
			return WebRequestResult<Texture2D>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<AudioClip> ToAsyncAudioClip(this UnityWebRequest request)
		{
			return WebRequestResult<AudioClip>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<MovieTexture> ToAsyncMovieTexture(this UnityWebRequest request)
		{
			return WebRequestResult<MovieTexture>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<byte[]> ToAsyncByteArray(this UnityWebRequest request)
		{
			return WebRequestResult<byte[]>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		/// <returns>Returns a <see cref="IAsyncOperation{T}"/> instance that will complete when the source operation have completed.</returns>
		public static WebRequestResult<string> ToAsyncString(this UnityWebRequest request)
		{
			return WebRequestResult<string>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Register a completion callback for the specified <see cref="UnityWebRequest"/> instance.
		/// </summary>
		/// <param name="request">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="request"/> has completed.</param>
		public static void RegisterCompletionCallback(this UnityWebRequest request, Action completionCallback)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (completionCallback == null)
			{
				throw new ArgumentNullException("completionCallback");
			}

			if (_asyncUpdater == null)
			{
				_asyncUpdater = AsyncUtility.GetRootGo().AddComponent<RequestStatusUpdater>();
			}

			_asyncUpdater.AddAsync(request, completionCallback);
		}

		#endregion

		#region implementation

		private class RequestStatusUpdater : MonoBehaviour
		{
			private readonly Dictionary<UnityWebRequest, Action> _ops = new Dictionary<UnityWebRequest, Action>();
			private readonly List<UnityWebRequest> _opsToRemove = new List<UnityWebRequest>();

			public void AddAsync(UnityWebRequest op, Action cb)
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
