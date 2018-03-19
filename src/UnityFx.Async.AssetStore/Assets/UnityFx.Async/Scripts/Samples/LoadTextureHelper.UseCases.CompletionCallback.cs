// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using UnityEngine;

namespace UnityFx.Async.Samples
{
	partial class LoadTextureHelper
	{
		/// <summary>
		/// Waits for the <see cref="LoadTextureAsync(string)"/> in completion callback.
		/// </summary>
		public void WaitForLoadOperationInCompletionCallback(string textureUrl)
		{
			LoadTextureAsync(textureUrl).AddCompletionCallback(op =>
			{
				if (op.IsCompletedSuccessfully)
				{
					var texture = (op as IAsyncOperation<Texture2D>).Result;
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
			});
		}
	}
}
