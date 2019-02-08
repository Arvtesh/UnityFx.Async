// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityFx.Async.Helpers
{
	/// <summary>
	/// A wrapper for <see cref="AsyncOperation"/> with <see cref="Scene"/> result value.
	/// </summary>
	internal class AssetBundleSceneRequestResult : AsyncOperationResult<Scene>
	{
		#region data

		private readonly LoadSceneMode _loadMode;
		private readonly string _sceneName;
		private string _finalSceneName;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleSceneRequestResult"/> class.
		/// </summary>
		protected AssetBundleSceneRequestResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleSceneRequestResult"/> class.
		/// </summary>
		public AssetBundleSceneRequestResult(string sceneName, LoadSceneMode loadMode)
		{
			_sceneName = sceneName;
			_loadMode = loadMode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleSceneRequestResult"/> class.
		/// </summary>
		public AssetBundleSceneRequestResult(string sceneName, LoadSceneMode loadMode, object userState)
			: base(null, userState)
		{
			_sceneName = sceneName;
			_loadMode = loadMode;
		}

		#endregion

		#region AsyncOperationResult

		/// <inheritdoc/>
		protected override Scene GetResult(AsyncOperation op)
		{
			var scene = default(Scene);

			// NOTE: Grab the last scene with the specified name from the list of loaded scenes.
			for (var i = SceneManager.sceneCount - 1; i >= 0; --i)
			{
				var s = SceneManager.GetSceneAt(i);

				if (s.name == _finalSceneName)
				{
					scene = s;
					break;
				}
			}

			if (!scene.isLoaded)
			{
				// TODO: Use dedicated exception type.
				throw new IOException(string.Format("Failed to load scene {0}.", _finalSceneName));
			}

			return scene;
		}

		#endregion

		#region IAsyncContinuation

		/// <inheritdoc/>
		public override void Invoke(IAsyncOperation op)
		{
			var abr = op as IAsyncOperation<AssetBundle>;

			if (abr != null && abr.IsCompletedSuccessfully)
			{
				Debug.Assert(Operation == null);

				var assetBundle = abr.Result;

				if (assetBundle.isStreamedSceneAssetBundle)
				{
					_finalSceneName = _sceneName;

					if (string.IsNullOrEmpty(_finalSceneName))
					{
						var scenePaths = assetBundle.GetAllScenePaths();

						if (scenePaths != null && scenePaths.Length > 0 && !string.IsNullOrEmpty(scenePaths[0]))
						{
							_finalSceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
						}
					}

					if (string.IsNullOrEmpty(_finalSceneName))
					{
						// TODO: Use dedicated exception type.
						TrySetException("The asset bundle does not contain scenes.");
					}
					else
					{
						Operation = SceneManager.LoadSceneAsync(_finalSceneName, _loadMode);
						Start();
					}
				}
				else
				{
					// TODO: Use dedicated exception type.
					TrySetException("The asset bundle does not contain scenes.");
				}
			}
			else
			{
				base.Invoke(op);
			}
		}

		#endregion
	}
}
