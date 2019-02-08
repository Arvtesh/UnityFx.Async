// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace UnityFx.Async
{
	/// <summary>
	/// Unity web request utilities.
	/// </summary>
	public static class AsyncWww
	{
		#region interface

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading text via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the text to download.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetBytesAsync(string)"/>
		public static IAsyncOperation<string> GetTextAsync(string url)
		{
			var webRequest = UnityWebRequest.Get(url);
			var result = new Helpers.WebRequestResult<string>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading binary content via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the binary content to download.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetTextAsync(string)"/>
		public static IAsyncOperation<byte[]> GetBytesAsync(string url)
		{
			var webRequest = UnityWebRequest.Get(url);
			var result = new Helpers.WebRequestResult<byte[]>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading a <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundleAsync(string, Hash128, uint)"/>
		public static IAsyncOperation<AssetBundle> GetAssetBundleAsync(string url)
		{
#if UNITY_2018_1_OR_NEWER
			var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
#else
			var webRequest = UnityWebRequest.GetAssetBundle(url);
#endif

			var result = new Helpers.WebRequestResult<AssetBundle>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading a <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="hash">A version hash. If this hash does not match the hash for the cached version of this asset bundle, the asset bundle will be redownloaded.</param>
		/// <param name="crc">If nonzero, this number will be compared to the checksum of the downloaded asset bundle data. If the CRCs do not match, an error will be logged and the asset bundle will not be loaded. If set to zero, CRC checking will be skipped.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundleAsync(string)"/>
		public static IAsyncOperation<AssetBundle> GetAssetBundleAsync(string url, Hash128 hash, uint crc)
		{
#if UNITY_2018_1_OR_NEWER
			var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, hash, crc);
#else
			var webRequest = UnityWebRequest.GetAssetBundle(url, hash, crc);
#endif

			var result = new Helpers.WebRequestResult<AssetBundle>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading assets from <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="assetName">Name of the prefab to load.</param>
		/// <param name="unloadAssetBundle">Specified whether to unload asset bundle ater the opertaion is complete.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundleAssetAsync(string, Hash128, uint, string, bool)"/>
		public static IAsyncOperation<T> GetAssetBundleAssetAsync<T>(string url, string assetName, bool unloadAssetBundle) where T : UnityEngine.Object
		{
#if UNITY_2018_1_OR_NEWER
			var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
#else
			var webRequest = UnityWebRequest.GetAssetBundle(url);
#endif

			var result = new Helpers.AssetBundleLoadAssetResult<T>(webRequest, assetName, unloadAssetBundle, null);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading assets from <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="hash">A version hash. If this hash does not match the hash for the cached version of this asset bundle, the asset bundle will be redownloaded.</param>
		/// <param name="crc">If nonzero, this number will be compared to the checksum of the downloaded asset bundle data. If the CRCs do not match, an error will be logged and the asset bundle will not be loaded. If set to zero, CRC checking will be skipped.</param>
		/// <param name="assetName">Name of the prefab to load.</param>
		/// <param name="unloadAssetBundle">Specified whether to unload asset bundle ater the opertaion is complete.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundleAssetAsync(string, string, bool)"/>
		public static IAsyncOperation<T> GetAssetBundleAssetAsync<T>(string url, Hash128 hash, uint crc, string assetName, bool unloadAssetBundle) where T : UnityEngine.Object
		{
#if UNITY_2018_1_OR_NEWER
			var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, hash, crc);
#else
			var webRequest = UnityWebRequest.GetAssetBundle(url, hash, crc);
#endif

			var result = new Helpers.AssetBundleLoadAssetResult<T>(webRequest, assetName, unloadAssetBundle, null);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading prefabs from <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="prefabName">Name of the prefab to load.</param>
		/// <param name="unloadAssetBundle">Specified whether to unload asset bundle ater the opertaion is complete.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundlePrefabAsync(string, Hash128, uint, string, bool)"/>
		public static IAsyncOperation<GameObject> GetAssetBundlePrefabAsync(string url, string prefabName, bool unloadAssetBundle)
		{
			return GetAssetBundleAssetAsync<GameObject>(url, prefabName, unloadAssetBundle);
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading prefabs from <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="hash">A version hash. If this hash does not match the hash for the cached version of this asset bundle, the asset bundle will be redownloaded.</param>
		/// <param name="crc">If nonzero, this number will be compared to the checksum of the downloaded asset bundle data. If the CRCs do not match, an error will be logged and the asset bundle will not be loaded. If set to zero, CRC checking will be skipped.</param>
		/// <param name="prefabName">Name of the prefab to load.</param>
		/// <param name="unloadAssetBundle">Specified whether to unload asset bundle ater the opertaion is complete.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundlePrefabAsync(string, string, bool)"/>
		public static IAsyncOperation<GameObject> GetAssetBundlePrefabAsync(string url, Hash128 hash, uint crc, string prefabName, bool unloadAssetBundle)
		{
			return GetAssetBundleAssetAsync<GameObject>(url, hash, crc, prefabName, unloadAssetBundle);
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading scenes from <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="sceneName">Name of the prefab to load.</param>
		/// <param name="loadMode">Scene load mode.</param>
		/// <param name="unloadAssetBundle">Specified whether to unload asset bundle ater the opertaion is complete.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundleSceneAsync(string, Hash128, uint, string, LoadSceneMode, bool)"/>
		public static IAsyncOperation<Scene> GetAssetBundleSceneAsync(string url, string sceneName, LoadSceneMode loadMode, bool unloadAssetBundle)
		{
#if UNITY_2018_1_OR_NEWER
			var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
#else
			var webRequest = UnityWebRequest.GetAssetBundle(url);
#endif

			var result = new Helpers.AssetBundleLoadSceneResult(webRequest, sceneName, loadMode, unloadAssetBundle, null);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading scenes from <see cref="AssetBundle"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the asset bundle to download.</param>
		/// <param name="hash">A version hash. If this hash does not match the hash for the cached version of this asset bundle, the asset bundle will be redownloaded.</param>
		/// <param name="crc">If nonzero, this number will be compared to the checksum of the downloaded asset bundle data. If the CRCs do not match, an error will be logged and the asset bundle will not be loaded. If set to zero, CRC checking will be skipped.</param>
		/// <param name="sceneName">Name of the prefab to load.</param>
		/// <param name="loadMode">Scene load mode.</param>
		/// <param name="unloadAssetBundle">Specified whether to unload asset bundle ater the opertaion is complete.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAssetBundleSceneAsync(string, string, LoadSceneMode, bool)"/>
		public static IAsyncOperation<Scene> GetAssetBundleSceneAsync(string url, Hash128 hash, uint crc, string sceneName, LoadSceneMode loadMode, bool unloadAssetBundle)
		{
#if UNITY_2018_1_OR_NEWER
			var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, hash, crc);
#else
			var webRequest = UnityWebRequest.GetAssetBundle(url, hash, crc);
#endif

			var result = new Helpers.AssetBundleLoadSceneResult(webRequest, sceneName, loadMode, unloadAssetBundle, null);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading a <see cref="AudioClip"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the audio clip to download.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAudioClipAsync(string, AudioType)"/>
		public static IAsyncOperation<AudioClip> GetAudioClipAsync(string url)
		{
#if UNITY_2017_1_OR_NEWER
			var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
#else
			var webRequest = UnityWebRequest.GetAudioClip(url, AudioType.UNKNOWN);
#endif

			var result = new Helpers.WebRequestResult<AudioClip>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading a <see cref="AudioClip"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the audio clip to download.</param>
		/// <param name="audioType">The type of audio encoding for the downloaded audio clip.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetAudioClipAsync(string)"/>
		public static IAsyncOperation<AudioClip> GetAudioClipAsync(string url, AudioType audioType)
		{
#if UNITY_2017_1_OR_NEWER
			var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
#else
			var webRequest = UnityWebRequest.GetAudioClip(url, audioType);
#endif

			var result = new Helpers.WebRequestResult<AudioClip>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading a <see cref="Texture2D"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the texture to download.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetTextureAsync(string, bool)"/>
		public static IAsyncOperation<Texture2D> GetTextureAsync(string url)
		{
#if UNITY_2017_1_OR_NEWER
			var webRequest = UnityWebRequestTexture.GetTexture(url, false);
#else
			var webRequest = UnityWebRequest.GetTexture(url);
#endif

			var result = new Helpers.WebRequestResult<Texture2D>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an asyncronous operation optimized for downloading a <see cref="Texture2D"/> via HTTP GET.
		/// </summary>
		/// <param name="url">The URI of the texture to download.</param>
		/// <param name="nonReadable">If <see langword="true"/>, the texture's raw data will not be accessible to script. This can conserve memory.</param>
		/// <returns>An operation that can be used to track the download process.</returns>
		/// <seealso cref="GetTextureAsync(string)"/>
		public static IAsyncOperation<Texture2D> GetTextureAsync(string url, bool nonReadable)
		{
#if UNITY_2017_1_OR_NEWER
			var webRequest = UnityWebRequestTexture.GetTexture(url, nonReadable);
#else
			var webRequest = UnityWebRequest.GetTexture(url, nonReadable);
#endif

			var result = new Helpers.WebRequestResult<Texture2D>(webRequest);
			result.Start();
			return result;
		}

		/// <summary>
		/// Returns result value of the specified <see cref="UnityWebRequest"/> instance.
		/// </summary>
		/// <param name="request">The request to get result for.</param>
		public static T GetResult<T>(UnityWebRequest request) where T : class
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (request.downloadHandler is DownloadHandlerAssetBundle)
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
			else if (typeof(T) == typeof(byte[]))
			{
				return request.downloadHandler.data as T;
			}
			else if (typeof(T) != typeof(object))
			{
				return request.downloadHandler.text as T;
			}

			return default(T);
		}

#if !UNITY_2018_3_OR_NEWER

		/// <summary>
		/// Returns result value of the specified <see cref="WWW"/> instance.
		/// </summary>
		/// <param name="request">The request to get result for.</param>
		public static T GetResult<T>(WWW request) where T : class
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (typeof(T) == typeof(AssetBundle))
			{
				return request.assetBundle as T;
			}
			else if (typeof(T) == typeof(Texture2D))
			{
				return request.texture as T;
			}
#if UNITY_5_6_OR_NEWER
			else if (typeof(T) == typeof(AudioClip))
			{
				return request.GetAudioClip() as T;
			}
#else
			else if (typeof(T) == typeof(AudioClip))
			{
				return request.audioClip as T;
			}
#endif
			else if (typeof(T) == typeof(byte[]))
			{
				return request.bytes as T;
			}
			else if (typeof(T) != typeof(object))
			{
				return request.text as T;
			}

			return null;
		}

#endif

		#endregion

		#region implementation
		#endregion
	}
}
