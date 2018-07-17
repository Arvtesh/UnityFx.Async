// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A wrapper for <see cref="AssetBundleRequest"/> with result value.
	/// </summary>
	/// <typeparam name="T">Result type.</typeparam>
	public class AssetBundleRequestResult<T> : AsyncOperationResult<T> where T : UnityEngine.Object
	{
		#region data

		private readonly string _assetName;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		protected AssetBundleRequestResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		/// <param name="assetName">Name of an asset to load.</param>
		protected AssetBundleRequestResult(string assetName)
		{
			_assetName = assetName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		/// <param name="assetbundle">The asset bundle to load asset from.</param>
		/// <param name="assetName">Name of an asset to load.</param>
		protected AssetBundleRequestResult(AssetBundle assetbundle, string assetName)
			: base(assetbundle.LoadAssetAsync(assetName))
		{
			_assetName = assetName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		/// <param name="op">Source operation.</param>
		public AssetBundleRequestResult(AssetBundleRequest op)
			: base(op)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		/// <param name="op">Source operation.</param>
		/// <param name="userState">User-defined data.</param>
		public AssetBundleRequestResult(AssetBundleRequest op, object userState)
			: base(op, userState)
		{
		}

		#endregion

		#region AsyncOperationResult

		/// <inheritdoc/>
		protected override T GetResult(AsyncOperation op)
		{
			return (op as AssetBundleRequest).asset as T;
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
				Debug.Assert(_assetName != null);

				Operation = abr.Result.LoadAssetAsync(_assetName);
				Start();
			}
			else
			{
				base.Invoke(op);
			}
		}

		#endregion
	}
}
