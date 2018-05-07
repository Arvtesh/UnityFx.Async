// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
#if NET_4_6 || NET_STANDARD_2_0
using System.Threading.Tasks;
#endif

using UnityEngine;

namespace UnityFx.Async.Samples
{
	partial class LoadTextureHelper
	{
#if SHOULD_NEVER_GET_HERE //NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Waits for the <see cref="LoadTextureAsync(string)"/> in with <c>await</c>.
		/// </summary>
		public async Task WaitForLoadOperationWithAwait(string textureUrl)
		{
			try
			{
				var texture = await LoadTextureAsync(textureUrl);
				Debug.Log("Yay! The texture is loaded!");
			}
			catch (OperationCanceledException)
			{
				Debug.LogWarning("The operation was canceled.");
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

#endif
	}
}
