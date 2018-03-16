// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Utility classes used by the project.
	/// </summary>
	public static class AsyncUtility
	{
		/// <summary>
		/// Returns a <see cref="GameObject"/> used by the library tools.
		/// </summary>
		public static GameObject GetRootGo()
		{
			var go = GameObject.Find("UnityFx.Async");

			if (go == null)
			{
				go = new GameObject("UnityFx.Async");
				GameObject.DontDestroyOnLoad(go);
			}

			return go;
		}
	}
}
