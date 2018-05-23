// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Text;
using UnityEngine;
#if UNITY_5_4_OR_NEWER || UNITY_2017 || UNITY_2018
using UnityEngine.Networking;
#elif UNITY_5_2_OR_NEWER
using UnityEngine.Experimental.Networking;
#endif

namespace UnityFx.Async
{
#if UNITY_5_2_OR_NEWER || UNITY_5_3_OR_NEWER || UNITY_2017 || UNITY_2018

	/// <summary>
	/// A wrapper for <see cref="UnityWebRequest"/> with result value.
	/// </summary>
	/// <typeparam name="T">Type of the request result.</typeparam>
	public class WebRequestResult<T> : AsyncResult<T> where T : class
	{
		#region data

		private readonly UnityWebRequest _request;
		private AsyncOperation _op;

		#endregion

		#region interface

		/// <summary>
		/// Gets the underlying <see cref="UnityWebRequest"/> instance.
		/// </summary>
		public UnityWebRequest WebRequest
		{
			get
			{
				return _request;
			}
		}

		/// <summary>
		/// Gets the request url string.
		/// </summary>
		public string Url
		{
			get
			{
				return _request.url;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebRequestResult{T}"/> class.
		/// </summary>
		/// <param name="request">Source web request.</param>
		public WebRequestResult(UnityWebRequest request)
			: base(request.isModifiable ? AsyncOperationStatus.Created : AsyncOperationStatus.Running, null, request)
		{
			_request = request;
		}

		/// <summary>
		/// Initializes the operation result value. Called when the underlying <see cref="UnityWebRequest"/> has completed withou errors.
		/// </summary>
		protected virtual T GetResult(UnityWebRequest request)
		{
			if (request.downloadHandler is DownloadHandlerBuffer)
			{
				return ((DownloadHandlerBuffer)request.downloadHandler).data as T;
			}
			else if (request.downloadHandler is DownloadHandlerAssetBundle)
			{
				return ((DownloadHandlerAssetBundle)request.downloadHandler).assetBundle as T;
			}
			else if (request.downloadHandler is DownloadHandlerTexture)
			{
				return ((DownloadHandlerTexture)request.downloadHandler).texture as T;
			}
			else if (request.downloadHandler is DownloadHandlerAudioClip)
			{
				return ((DownloadHandlerAudioClip)request.downloadHandler).audioClip as T;
			}
#if !UNITY_5
			else if (request.downloadHandler is DownloadHandlerMovieTexture)
			{
				return ((DownloadHandlerMovieTexture)request.downloadHandler).movieTexture as T;
			}
#endif
			else if (typeof(T) != typeof(object))
			{
				return request.downloadHandler.text as T;
			}

			return null;
		}

		/// <summary>
		/// Creates a wrapper for the specified <see cref="UnityWebRequest"/> instance.
		/// </summary>
		public static WebRequestResult<T> FromUnityWebRequest(UnityWebRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			var result = new WebRequestResult<T>(request);

			if (request.isDone)
			{
				result.SetCompleted(true);
			}
			else if (!request.isModifiable)
			{
				AsyncUtility.AddCompletionCallback(request, () => result.SetCompleted(false));
			}

			return result;
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override float GetProgress()
		{
			if (_op != null)
			{
				return _op.progress;
			}

			return base.GetProgress();
		}

		/// <inheritdoc/>
		protected override void OnStarted()
		{
			base.OnStarted();

#if UNITY_2017_2_OR_NEWER || UNITY_2018

			// Starting with Unity 2017.2 there is AsyncOperation.completed event
			_op = _request.SendWebRequest();
			_op.completed += op => SetCompleted(false);

#else

			_op = _request.Send();
			AsyncUtility.AddCompletionCallback(_request, () => SetCompleted(false));

#endif
		}

		/// <inheritdoc/>
		protected override void OnCancel()
		{
			if (TrySetCanceled(false))
			{
				_request.Abort();
			}
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_request.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Object

		/// <inheritdoc/>
		public override string ToString()
		{
			var result = new StringBuilder();
			var errorStr = _request.error;

			result.Append(_request.GetType().Name);
			result.Append(" (");
			result.Append(_request.url);

			if (IsFaulted)
			{
				if (string.IsNullOrEmpty(errorStr))
				{
					result.Append(", ");
					result.Append(_request.responseCode.ToString());
				}
				else
				{
					result.Append(", ");
					result.Append(errorStr);
					result.Append(" (");
					result.Append(_request.responseCode.ToString());
					result.Append(')');
				}
			}
			else if (IsCompleted)
			{
				result.Append(", ");
				result.Append(_request.responseCode.ToString());
			}

			result.Append(')');
			return result.ToString();
		}

		#endregion

		#region implementation

		private void SetCompleted(bool completedSynchronously)
		{
#if UNITY_5
			if (_request.isError)
#else
			if (_request.isHttpError || _request.isNetworkError)
#endif
			{
				TrySetException(new WebRequestException(_request.error, _request.responseCode), completedSynchronously);
			}
			else if (_request.downloadHandler != null)
			{
				TrySetResult(GetResult(_request), completedSynchronously);
			}
			else
			{
				TrySetCompleted(completedSynchronously);
			}
		}

		#endregion
	}

#endif
}
