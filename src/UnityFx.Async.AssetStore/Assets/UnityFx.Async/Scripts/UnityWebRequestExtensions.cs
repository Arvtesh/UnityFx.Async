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
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult<T> ToAsync<T>(this UnityWebRequest request) where T : class
		{
			return CreateAsyncInternal<T>(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult<AssetBundle> ToAsyncAssetBundle(this UnityWebRequest request)
		{
			return CreateAsyncInternal<AssetBundle>(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult<Texture2D> ToAsyncTexture(this UnityWebRequest request)
		{
			return CreateAsyncInternal<Texture2D>(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult<AudioClip> ToAsyncAudioClip(this UnityWebRequest request)
		{
			return CreateAsyncInternal<AudioClip>(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult<MovieTexture> ToAsyncMovieTexture(this UnityWebRequest request)
		{
			return CreateAsyncInternal<MovieTexture>(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult<byte[]> ToAsyncByteArray(this UnityWebRequest request)
		{
			return CreateAsyncInternal<byte[]>(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		/// <returns>Returns a <see cref="IAsyncOperation{T}"/> instance that will complete when the source operation have completed.</returns>
		public static AsyncResult<string> ToAsyncString(this UnityWebRequest request)
		{
			return CreateAsyncInternal<string>(request);
		}

		#endregion

		#region implementation

		private class WebRequestResult<T> : AsyncResult<T> where T : class
		{
			private readonly UnityWebRequest _request;

			public WebRequestResult(UnityWebRequest request)
				: base(request.isModifiable ? AsyncOperationStatus.Created : AsyncOperationStatus.Running, null, request)
			{
				_request = request;
			}

			public void SetCompleted(bool completedSynchronously)
			{
				if (_request.isHttpError || _request.isNetworkError)
				{
					TrySetException(new UnityWebRequestException(_request.error, _request.responseCode), completedSynchronously);
				}
				else if (_request.downloadHandler != null)
				{
					TrySetResult(GetResult(), completedSynchronously);
				}
				else
				{
					TrySetCompleted(completedSynchronously);
				}
			}

			protected override void OnStarted()
			{
				base.OnStarted();

#if UNITY_2017_2_OR_NEWER
				_request.SendWebRequest().completed += op => SetCompleted(false);
#else
				_request.Send();
				AddAsyncToUpdateList(_request, () => SetCompleted(false));
#endif
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_request.Dispose();
				}

				base.Dispose(disposing);
			}

			public override string ToString()
			{
				var result = "UnityWebRequest (" + _request.responseCode.ToString();
				var errorStr = _request.error;

				if (IsFaulted && !string.IsNullOrEmpty(errorStr))
				{
					result += ", " + errorStr;
				}

				result += ')';
				return result;
			}

			private T GetResult()
			{
				if (_request.downloadHandler is DownloadHandlerBuffer)
				{
					return ((DownloadHandlerBuffer)_request.downloadHandler).data as T;
				}
				else if (_request.downloadHandler is DownloadHandlerAssetBundle)
				{
					return ((DownloadHandlerAssetBundle)_request.downloadHandler).assetBundle as T;
				}
				else if (_request.downloadHandler is DownloadHandlerTexture)
				{
					return ((DownloadHandlerTexture)_request.downloadHandler).texture as T;
				}
				else if (_request.downloadHandler is DownloadHandlerAudioClip)
				{
					return ((DownloadHandlerAudioClip)_request.downloadHandler).audioClip as T;
				}
				else if (_request.downloadHandler is DownloadHandlerMovieTexture)
				{
					return ((DownloadHandlerMovieTexture)_request.downloadHandler).movieTexture as T;
				}

				return _request.downloadHandler.text as T;
			}
		}

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

		private static void AddAsyncToUpdateList(UnityWebRequest op, Action cb)
		{
			if (_asyncUpdater == null)
			{
				_asyncUpdater = AsyncUtility.GetRootGo().AddComponent<RequestStatusUpdater>();
			}

			_asyncUpdater.AddAsync(op, cb);
		}

		private static AsyncResult<T> CreateAsyncInternal<T>(UnityWebRequest request) where T : class
		{
			var result = new WebRequestResult<T>(request);

			if (request.isDone)
			{
				result.SetCompleted(true);
			}
			else if (!request.isModifiable)
			{
				AddAsyncToUpdateList(request, () => result.SetCompleted(false));
			}

			return result;
		}

		#endregion
	}
}
