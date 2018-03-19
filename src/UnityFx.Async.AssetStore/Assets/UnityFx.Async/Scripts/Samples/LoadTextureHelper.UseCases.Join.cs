// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async.Samples
{
	partial class LoadTextureHelper
	{
		/// <summary>
		/// Waits for the <see cref="LoadTextureAsync(string)"/> in a thread.
		/// </summary>
		public void WaitForLoadOperationInAnotherThread(string textureUrl)
		{
			var loadOp = LoadTextureAsync(textureUrl);

			ThreadPool.QueueUserWorkItem(
				args =>
				{
					// Dispose the opepration when it is not needed.
					using (var op = args as IAsyncOperation<Texture2D>)
					{
						try
						{
							// Block the thread until the result is available.
							var texture = op.Join();

							// The texture is loaded
							Debug.Log(texture);
						}
						catch (OperationCanceledException)
						{
							// The operation was canceled
						}
						catch (Exception)
						{
							// Load failed
						}
					}
				},
				loadOp);
		}
	}
}
