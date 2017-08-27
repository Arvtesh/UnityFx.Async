// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/> wrapper of <see cref="AsyncResult"/>.
	/// </summary>
	internal class UnityWebRequestWrapper<T> : AsyncResult<T> where T : UnityEngine.Object
	{
		#region data

		private readonly UnityWebRequest _request;
		private readonly AsyncOperation _op;
		private readonly Func<UnityWebRequest, T> _resultProcessor;

		#endregion

		#region interface

		public UnityWebRequestWrapper(UnityWebRequest request)
			: base(request)
		{
			_request = request;
			_op = request.Send();
		}

		public UnityWebRequestWrapper(UnityWebRequest request, Func<UnityWebRequest, T> resultProcessor)
			: base(request)
		{
			_request = request;
			_resultProcessor = resultProcessor;
			_op = request.Send();
		}

#if NET46
		public UnityWebRequestWrapper(UnityWebRequest request, CancellationToken cancellationToken)
			: base(request, cancellationToken)
		{
			_request = request;
			_op = request.Send();
		}

		public UnityWebRequestWrapper(UnityWebRequest request, Func<UnityWebRequest, T> resultProcessor, CancellationToken cancellationToken)
			: base(request, cancellationToken)
		{
			_request = request;
			_resultProcessor = resultProcessor;
			_op = request.Send();
		}
#endif

		protected override void OnUpdate()
		{
			if (_op.isDone)
			{
				if (_request.isNetworkError || _request.isHttpError)
				{
					throw new WebException(_request.error);
				}
				else
				{
					SetResult();
				}
			}
			else
			{
				SetProgress(_op.progress);
			}
		}

		protected override void OnCompleted()
		{
			_request.Dispose();
		}

		#endregion

		#region implementation

		private void SetResult()
		{
			if (_resultProcessor != null)
			{
				SetResult(_resultProcessor(_request));
			}
			else if (typeof(T) == typeof(string))
			{
				SetResult(_request.downloadHandler.text as T);
			}
			else if (typeof(T) == typeof(byte[]))
			{
				SetResult(_request.downloadHandler.data as T);
			}
			else if (typeof(T) == typeof(AssetBundle))
			{
				if (_request.downloadHandler is DownloadHandlerAssetBundle h)
				{
					SetResult(h.assetBundle as T);
				}
				else
				{
					throw new InvalidCastException("Download handler for the request is expected to be DownloadHandlerAssetBundle");
				}
			}
			else if (typeof(T) == typeof(Texture))
			{
				if (_request.downloadHandler is DownloadHandlerTexture h)
				{
					SetResult(h.texture as T);
				}
				else
				{
					throw new InvalidCastException("Download handler for the request is expected to be DownloadHandlerTexture");
				}
			}
			else if (typeof(T) == typeof(MovieTexture))
			{
				if (_request.downloadHandler is DownloadHandlerMovieTexture h)
				{
					SetResult(h.movieTexture as T);
				}
				else
				{
					throw new InvalidCastException("Download handler for the request is expected to be DownloadHandlerMovieTexture");
				}
			}
			else if (typeof(T) == typeof(AudioClip))
			{
				if (_request.downloadHandler is DownloadHandlerAudioClip h)
				{
					SetResult(h.audioClip as T);
				}
				else
				{
					throw new InvalidCastException("Download handler for the request is expected to be DownloadHandlerAudioClip");
				}
			}
			else
			{
				SetCompleted();
			}
		}

		#endregion
	}
}
