// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
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

		/// <summary>
		/// Starts a coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine to run.</param>
		public static Coroutine StartCoroutine(IEnumerator enumerator)
		{
			var go = GetRootGo();
			var runner = go.GetComponent<CoroutineRunner>();

			if (!runner)
			{
				runner = go.AddComponent<CoroutineRunner>();
			}

			return runner.StartCoroutine(enumerator);
		}

		/// <summary>
		/// Stops the specified coroutine.
		/// </summary>
		/// <param name="coroutine">The coroutine to run.</param>
		public static void StopCoroutine(Coroutine coroutine)
		{
			var go = GetRootGo();
			var runner = go.GetComponent<CoroutineRunner>();

			if (runner)
			{
				runner.StopCoroutine(coroutine);
			}
		}

		/// <summary>
		/// Stops the specified coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine to run.</param>
		public static void StopCoroutine(IEnumerator enumerator)
		{
			var go = GetRootGo();
			var runner = go.GetComponent<CoroutineRunner>();

			if (runner)
			{
				runner.StopCoroutine(enumerator);
			}
		}

		/// <summary>
		/// Stops all coroutines.
		/// </summary>
		public static void StopAllCoroutines()
		{
			var go = GetRootGo();
			var runner = go.GetComponent<CoroutineRunner>();

			if (runner)
			{
				runner.StopAllCoroutines();
			}
		}

		#endregion

		#region implementation

		private class CoroutineRunner : MonoBehaviour
		{
		}

		#endregion
	}
}
