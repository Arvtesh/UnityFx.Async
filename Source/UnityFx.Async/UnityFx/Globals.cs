// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Responsible for initialization of global data used by the assembly classes.
	/// </summary>
	internal static class Globals
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Returns the global <see cref="GameObject"/> containing all components used by the assembly classes. Read only.
		/// </summary>
		public static GameObject RootGo { get; }

		/// <summary>
		/// Returns a <see cref="MonoBehaviour"/> instance that is used for running coroutines. Read only.
		/// </summary>
		public static MonoBehaviour AsyncRunner { get; }

		/// <summary>
		/// Create a new <see cref="GameObject"/> instance and attaches it to the <see cref="RootGo"/>.
		/// </summary>
		/// <param name="childName">Name of the <see cref="GameObject"/> to create.</param>
		/// <returns>Returns the <see cref="GameObject"/> instance created.</returns>
		public static GameObject AddChildService(string childName)
		{
			var go = new GameObject(childName);
			go.transform.SetParent(RootGo.transform, false);
			return go;
		}

		#endregion

		#region implementation

		static Globals()
		{
			RootGo = new GameObject(typeof(Globals).Assembly.GetName().Name);
			GameObject.DontDestroyOnLoad(RootGo);

			AsyncRunner = RootGo.AddComponent<AsyncRunnerBehaviour>();
		}

		#endregion
	}
}
