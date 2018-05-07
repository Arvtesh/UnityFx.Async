// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

#if UNITY_2017_2_OR_NEWER || UNITY_2018

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

#if UNITY_2017_2_OR_NEWER || UNITY_2018

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

#if UNITY_2017_2_OR_NEWER || UNITY_2018

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				op.completed += o => result.TrySetResult(op.asset as T);

#else

				AsyncUtility.AddCompletionCallback(op, () => result.TrySetResult(op.asset as T));

#endif

				return result;
			}
		}

#if NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Provides an object that waits for the completion of an <see cref="AsyncOperation"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public struct AsyncOperationAwaiter : INotifyCompletion
		{
			private readonly AsyncOperation _op;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncOperationAwaiter"/> struct.
			/// </summary>
			public AsyncOperationAwaiter(AsyncOperation op)
			{
				_op = op;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.isDone;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public void GetResult()
			{
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
#if UNITY_2017_2_OR_NEWER || UNITY_2018

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				_op.completed += o => continuation();

#else

				AsyncUtility.AddCompletionCallback(_op, continuation);

#endif
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation op)
		{
			return new AsyncOperationAwaiter(op);
		}

#endif

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

#if NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Provides an object that waits for the completion of an <see cref="WWW"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public struct UnityWebRequestAwaiter : INotifyCompletion
		{
			private readonly UnityWebRequest _op;

			/// <summary>
			/// Initializes a new instance of the <see cref="UnityWebRequestAwaiter"/> struct.
			/// </summary>
			public UnityWebRequestAwaiter(UnityWebRequest op)
			{
				_op = op;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.isDone;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public void GetResult()
			{
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				AsyncUtility.AddCompletionCallback(_op, continuation);
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequest op)
		{
			return new UnityWebRequestAwaiter(op);
		}

#endif

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

#if NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Provides an object that waits for the completion of an <see cref="WWW"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public struct WwwAwaiter : INotifyCompletion
		{
			private readonly WWW _op;

			/// <summary>
			/// Initializes a new instance of the <see cref="WwwAwaiter"/> struct.
			/// </summary>
			public WwwAwaiter(WWW op)
			{
				_op = op;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.isDone;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public void GetResult()
			{
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				AsyncUtility.AddCompletionCallback(_op, continuation);
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		public static WwwAwaiter GetAwaiter(this WWW op)
		{
			return new WwwAwaiter(op);
		}

#endif

		#endregion

		#region implementation
		#endregion
	}
}
