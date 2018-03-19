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
		#region data

		private static GameObject _go;

		#endregion

		#region interface

		/// <summary>
		/// Returns a <see cref="GameObject"/> used by the library tools.
		/// </summary>
		public static GameObject GetRootGo()
		{
			if (!_go)
			{
				_go = GameObject.Find("UnityFx.Async");

				if (!_go)
				{
					_go = new GameObject("UnityFx.Async");
					GameObject.DontDestroyOnLoad(_go);
				}
			}

			return _go;
		}

		#endregion

		#region implementation
		#endregion
	}
}
