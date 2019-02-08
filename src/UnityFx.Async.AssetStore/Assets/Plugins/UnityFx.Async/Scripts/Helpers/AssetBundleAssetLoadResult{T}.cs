// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Async.Helpers
{
	using Debug = System.Diagnostics.Debug;

	internal class AssetBundleAssetLoadResult<T> : AsyncResult<T> where T : UnityEngine.Object
	{
		#region data

		private readonly WebRequestResult<AssetBundle> _assetBundleLoadResult;
		private readonly AssetBundleRequestResult<T> _assetLoadResult;
		private readonly bool _unloadAssetBundle;

		#endregion

		#region interface

		public AssetBundleAssetLoadResult(UnityWebRequest request, string assetName, bool unloadAssetBundle, object userState)
			: base(AsyncOperationStatus.Running, userState)
		{
			_unloadAssetBundle = unloadAssetBundle;
			_assetBundleLoadResult = new WebRequestResult<AssetBundle>(request);
			_assetLoadResult = new AssetBundleRequestResult<T>(assetName);
			_assetBundleLoadResult.AddCompletionCallback(_assetLoadResult);
			_assetLoadResult.AddCompletionCallback(this);
		}

		#endregion

		#region AsyncResult

		protected override void OnStarted()
		{
			base.OnStarted();
			_assetBundleLoadResult.Start();
		}

		public override void Invoke(IAsyncOperation op)
		{
			if (_unloadAssetBundle)
			{
				_assetBundleLoadResult.Result.Unload(false);
			}

			if (op.IsCompletedSuccessfully)
			{
				TrySetResult(_assetLoadResult.Result);
			}
			else
			{
				TrySetException(op.Exception);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
