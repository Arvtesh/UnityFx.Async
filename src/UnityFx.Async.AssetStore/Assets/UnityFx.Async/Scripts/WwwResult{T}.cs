// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Text;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A wrapper for <see cref="WWW"/> with result value.
	/// </summary>
	/// <typeparam name="T">Type of the request result.</typeparam>
	public class WwwResult<T> : AsyncResult<T> where T : class
	{
		#region data

		private readonly WWW _www;

		#endregion

		#region interface

		/// <summary>
		/// Gets the underlying <see cref="WWW"/> instance.
		/// </summary>
		public WWW WebRequest
		{
			get
			{
				return _www;
			}
		}

		/// <summary>
		/// Gets the request url string.
		/// </summary>
		public string Url
		{
			get
			{
				return _www.url;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WwwResult{T}"/> class.
		/// </summary>
		/// <param name="request">Source web request.</param>
		public WwwResult(WWW request)
			: base(AsyncOperationStatus.Running, null, request)
		{
			_www = request;
		}

		/// <summary>
		/// Initializes the operation result value. Called when the underlying <see cref="WWW"/> has completed withou errors.
		/// </summary>
		protected virtual T GetResult(WWW request)
		{
			if (typeof(T) == typeof(byte[]))
			{
				return request.bytes as T;
			}
			else if (typeof(T) == typeof(AssetBundle))
			{
				return request.assetBundle as T;
			}
			else if (typeof(T) == typeof(Texture2D))
			{
				return request.texture as T;
			}
			else if (typeof(T) == typeof(AudioClip))
			{
#if UNITY_2017_1_OR_NEWER
				return request.GetAudioClip() as T;
#else
				return request.audioClip as T;
#endif
			}
			else if (typeof(T) == typeof(MovieTexture))
			{
#if UNITY_2017_1_OR_NEWER
				return request.GetMovieTexture() as T;
#else
				return request.movie as T;
#endif
			}
			else if (typeof(T) != typeof(object))
			{
				return request.text as T;
			}

			return null;
		}

		/// <summary>
		/// Creates a wrapper for the specified <see cref="WWW"/> instance.
		/// </summary>
		public static WwwResult<T> FromWWW(WWW request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			var result = new WwwResult<T>(request);

			if (request.isDone)
			{
				result.SetCompleted(true);
			}
			else
			{
				AsyncUtility.AddCompletionCallback(request, () => result.SetCompleted(false));
			}

			return result;
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override void OnStarted()
		{
			base.OnStarted();

			AsyncUtility.AddCompletionCallback(_www, () => SetCompleted(false));
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_www.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Object

		/// <inheritdoc/>
		public override string ToString()
		{
			var result = new StringBuilder();
			var errorStr = _www.error;

			result.Append(_www.GetType().Name);
			result.Append(" (");
			result.Append(_www.url);

			if (IsFaulted && !string.IsNullOrEmpty(errorStr))
			{
				result.Append(", ");
				result.Append(errorStr);
			}

			result.Append(')');
			return result.ToString();
		}

		#endregion

		#region implementation

		private void SetCompleted(bool completedSynchronously)
		{
			if (string.IsNullOrEmpty(_www.error))
			{
				TrySetResult(GetResult(_www), completedSynchronously);
			}
			else
			{
				TrySetException(new WebRequestException(_www.error), completedSynchronously);
			}
		}

		#endregion
	}
}
