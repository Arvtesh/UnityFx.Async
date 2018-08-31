// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections;
using UnityEngine;

namespace UnityFx.Async.Samples
{
	partial class LoadTextureHelper
	{
		/// <summary>
		/// Waits for the <see cref="LoadTextureAsync(string)"/> in Unity coroutine.
		/// </summary>
		public IEnumerator WaitForLoadOperationInCoroutine(string textureUrl)
		{
			var op = LoadTextureAsync(textureUrl);
			yield return op;

			if (op.IsCompletedSuccessfully)
			{
				Debug.Log("Yay!");
			}
			else if (op.IsFaulted)
			{
				Debug.LogException(op.Exception);
			}
			else if (op.IsCanceled)
			{
				Debug.LogWarning("The operation was canceled.");
			}
		}
	}
}
