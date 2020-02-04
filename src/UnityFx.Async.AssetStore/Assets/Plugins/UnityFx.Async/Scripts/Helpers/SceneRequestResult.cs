// Copyright (c) 2018-2020 Alexander Bogarsukov.
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
	internal class SceneRequestResult : AsyncOperationResult<Scene>
	{
		#region data

		private readonly LoadSceneMode _loadMode;
		private readonly string _sceneName;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="SceneRequestResult"/> class.
		/// </summary>
		protected SceneRequestResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SceneRequestResult"/> class.
		/// </summary>
		public SceneRequestResult(string sceneName, LoadSceneMode loadMode)
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

				if (s.name == _sceneName)
				{
					scene = s;
					break;
				}
			}

			if (!scene.isLoaded)
			{
				throw new AssetLoadException(_sceneName, typeof(Scene));
			}

			return scene;
		}

		#endregion

		#region AsyncResult

		protected override void OnStarted()
		{
			Operation = SceneManager.LoadSceneAsync(_sceneName, _loadMode);
			base.OnStarted();
		}

		#endregion

		#region implementation
		#endregion
	}
}
