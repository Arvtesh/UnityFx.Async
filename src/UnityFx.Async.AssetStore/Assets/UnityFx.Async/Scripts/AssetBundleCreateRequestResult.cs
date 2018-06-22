// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A wrapper for <see cref="AssetBundleCreateRequest"/>.
	/// </summary>
	public class AssetBundleCreateRequestResult : AsyncOperationResult<AssetBundle>
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleCreateRequestResult"/> class.
		/// </summary>
		/// <param name="op">Source web request.</param>
		public AssetBundleCreateRequestResult(AssetBundleCreateRequest op)
			: base(op)
		{
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override AssetBundle GetResult(AsyncOperation op)
		{
			return (op as AssetBundleCreateRequest).assetBundle;
		}

		#endregion
	}
}
