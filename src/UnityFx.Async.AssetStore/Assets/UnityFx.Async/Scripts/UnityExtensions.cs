// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_4_OR_NEWER || UNITY_2017 || UNITY_2018
using UnityEngine.Networking;
#elif UNITY_5_2_OR_NEWER
using UnityEngine.Experimental.Networking;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Extensions for Unity API.
	/// </summary>
	public static class UnityExtensions
	{
		#region AsyncOperation

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

				AsyncUtility.AddCompletionCallback(op, () => result.TrySetCompleted());

#endif

				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the Unity <see cref="AsyncOperation"/>.
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

				AsyncUtility.AddCompletionCallback(op, () => result.TrySetResult(op.asset as T));

#endif

				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the Unity <see cref="AsyncOperation"/>.
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

				AsyncUtility.AddCompletionCallback(op, () => result.TrySetResult(op.asset as T));

#endif

				return result;
			}
		}

		#endregion

		#region UnityWebRequest

#if UNITY_5_2_OR_NEWER || UNITY_5_3_OR_NEWER || UNITY_2017 || UNITY_2018

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult ToAsync(this UnityWebRequest request)
		{
			return WebRequestResult<object>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<T> ToAsync<T>(this UnityWebRequest request) where T : class
		{
			return WebRequestResult<T>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<AssetBundle> ToAsyncAssetBundle(this UnityWebRequest request)
		{
			return WebRequestResult<AssetBundle>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<Texture2D> ToAsyncTexture(this UnityWebRequest request)
		{
			return WebRequestResult<Texture2D>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<AudioClip> ToAsyncAudioClip(this UnityWebRequest request)
		{
			return WebRequestResult<AudioClip>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<MovieTexture> ToAsyncMovieTexture(this UnityWebRequest request)
		{
			return WebRequestResult<MovieTexture>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WebRequestResult<byte[]> ToAsyncByteArray(this UnityWebRequest request)
		{
			return WebRequestResult<byte[]>.FromUnityWebRequest(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		/// <returns>Returns a <see cref="IAsyncOperation{TResult}"/> instance that will complete when the source operation have completed.</returns>
		public static WebRequestResult<string> ToAsyncString(this UnityWebRequest request)
		{
			return WebRequestResult<string>.FromUnityWebRequest(request);
		}

#endif

		#endregion

		#region WWW

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult ToAsync(this WWW request)
		{
			return WwwResult<object>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<T> ToAsync<T>(this WWW request) where T : class
		{
			return WwwResult<T>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<AssetBundle> ToAsyncAssetBundle(this WWW request)
		{
			return WwwResult<AssetBundle>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<Texture2D> ToAsyncTexture(this WWW request)
		{
			return WwwResult<Texture2D>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<AudioClip> ToAsyncAudioClip(this WWW request)
		{
			return WwwResult<AudioClip>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<MovieTexture> ToAsyncMovieTexture(this WWW request)
		{
			return WwwResult<MovieTexture>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<byte[]> ToAsyncByteArray(this WWW request)
		{
			return WwwResult<byte[]>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<string> ToAsyncString(this WWW request)
		{
			return WwwResult<string>.FromWWW(request);
		}

		#endregion

		#region implementation
		#endregion
	}
}
