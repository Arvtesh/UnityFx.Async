// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A wrapper for <see cref="ResourceRequest"/> with result value.
	/// </summary>
	/// <typeparam name="T">Result type.</typeparam>
	public class ResourceRequestResult<T> : AsyncOperationResult<T> where T : UnityEngine.Object
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceRequestResult{T}"/> class.
		/// </summary>
		/// <param name="op">Source web request.</param>
		public ResourceRequestResult(ResourceRequest op)
			: base(op)
		{
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override T GetResult(AsyncOperation op)
		{
			return (op as ResourceRequest).asset as T;
		}

		#endregion
	}
}
