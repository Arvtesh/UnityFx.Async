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
	public sealed class AssetBundleRequestResult<T> : AsyncOperationResult<T> where T : UnityEngine.Object
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		/// <param name="op">Source operation.</param>
		public AssetBundleRequestResult(AssetBundleRequest op)
			: base(op)
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
	}
}
