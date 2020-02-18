// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityFx.Async.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="AssetBundle"/>.
	/// </summary>
	public static class AssetBundleExtensions
	{
		/// <summary>
		/// Loads a <see cref="Scene"/> from an asset bundle.
		/// </summary>
		/// <param name="assetBundle">The source asset bundle.</param>
		/// <param name="loadMode">The scene load mode.</param>
		/// <param name="sceneName">Name of the scene to load or <see langword="null"/> to load the any scene.</param>
		public static IAsyncOperation<Scene> LoadSceneAsync(this AssetBundle assetBundle, LoadSceneMode loadMode, string sceneName)
		{
			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				throw new InvalidOperationException();
			}

			if (string.IsNullOrEmpty(sceneName))
			{
				var scenePaths = assetBundle.GetAllScenePaths();

				if (scenePaths != null && scenePaths.Length > 0 && !string.IsNullOrEmpty(scenePaths[0]))
				{
					sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
				}

				if (string.IsNullOrEmpty(sceneName))
				{
					throw new AssetLoadException("The asset bundle contains no scenes.", null, typeof(Scene));
				}
			}

			return AsyncUtility.LoadSceneAsync(sceneName, loadMode);
		}
	}
}
