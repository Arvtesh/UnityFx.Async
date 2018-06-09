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
	public sealed class AssetBundleRequestResult<T> : AsyncResult<T> where T : UnityEngine.Object
	{
		#region data

		private readonly AssetBundleRequest _op;

		#endregion

		#region interface

		/// <summary>
		/// Gets the underlying <see cref="AssetBundleRequest"/> instance.
		/// </summary>
		public AssetBundleRequest Request
		{
			get
			{
				return _op;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssetBundleRequestResult{T}"/> class.
		/// </summary>
		/// <param name="op">Source web request.</param>
		public AssetBundleRequestResult(AssetBundleRequest op)
			: base(AsyncOperationStatus.Running)
		{
			_op = op;

			if (op.isDone)
			{
				TrySetResult(op.asset as T, true);
			}
			else
			{
#if UNITY_2017_2_OR_NEWER || UNITY_2018

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				op.completed += o => TrySetResult(_op.asset as T);

#else

				AsyncUtility.AddCompletionCallback(op, () => TrySetResult(_op.asset as T));

#endif
			}
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override float GetProgress()
		{
			return _op.progress;
		}

		#endregion
	}
}
