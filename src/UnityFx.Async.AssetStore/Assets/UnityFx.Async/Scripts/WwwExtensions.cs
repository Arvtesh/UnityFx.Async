// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Extensions for <see cref="WWW"/> class.
	/// </summary>
	public static class WwwExtensions
	{
		#region data

		private static RequestStatusUpdater _asyncUpdater;

		#endregion

		#region interface

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static AsyncResult ToAsync(this WWW request)
		{
			return WwwResult<object>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<T> ToAsync<T>(this WWW request) where T : class
		{
			return WwwResult<T>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<AssetBundle> ToAsyncAssetBundle(this WWW request)
		{
			return WwwResult<AssetBundle>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<Texture2D> ToAsyncTexture(this WWW request)
		{
			return WwwResult<Texture2D>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<AudioClip> ToAsyncAudioClip(this WWW request)
		{
			return WwwResult<AudioClip>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<MovieTexture> ToAsyncMovieTexture(this WWW request)
		{
			return WwwResult<MovieTexture>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static WwwResult<byte[]> ToAsyncByteArray(this WWW request)
		{
			return WwwResult<byte[]>.FromWWW(request);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{T}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		/// <returns>Returns a <see cref="IAsyncOperation{T}"/> instance that will complete when the source operation have completed.</returns>
		public static WwwResult<string> ToAsyncString(this WWW request)
		{
			return WwwResult<string>.FromWWW(request);
		}

		/// <summary>
		/// Register a completion callback for the specified <see cref="WWW"/> instance.
		/// </summary>
		/// <param name="request">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="request"/> has completed.</param>
		public static void RegisterCompletionCallback(this WWW request, Action completionCallback)
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
			private readonly Dictionary<WWW, Action> _ops = new Dictionary<WWW, Action>();
			private readonly List<WWW> _opsToRemove = new List<WWW>();

			public void AddAsync(WWW op, Action cb)
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
