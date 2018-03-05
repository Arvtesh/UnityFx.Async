// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Async.Samples
{
	/// <summary>
	/// A <see cref="MonoBehaviour"/> that demonstrates wrapping <c>Unity3d</c> web requests in Task-based Asynchronous Pattern manner.
	/// </summary>
	public class LoadTextureHelper : MonoBehaviour
	{
		#region LoadTextureAsync

		/// <summary>
		/// Load a <see cref="Texture2D"/> from the specified URL.
		/// </summary>
		public IAsyncOperation<Texture2D> LoadTextureAsync(string textureUrl)
		{
			var result = new AsyncCompletionSource<Texture2D>();
			StartCoroutine(LoadTextureInternal(result, textureUrl));
			return result.Operation;
		}

		private IEnumerator LoadTextureInternal(IAsyncCompletionSource<Texture2D> op, string textureUrl)
		{
			var www = UnityWebRequestTexture.GetTexture(textureUrl);
			yield return www.Send();

			if (www.isNetworkError || www.isHttpError)
			{
				op.SetException(new Exception(www.error));
			}
			else
			{
				op.SetResult(((DownloadHandlerTexture)www.downloadHandler).texture);
			}
		}

		#endregion

		#region Samples

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

#if UNITYFX_SUPPORT_TAP

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

		/// <summary>
		/// Waits for the <see cref="LoadTextureAsync(string)"/> in a thread.
		/// </summary>
		public void WaitForLoadOperationInAnotherThread(string textureUrl)
		{
			var op = LoadTextureAsync(textureUrl);

			ThreadPool.QueueUserWorkItem(
				args =>
				{
					try
					{
						var texture = (args as IAsyncOperation<Texture2D>).Join();

						// The texture is loaded
					}
					catch (OperationCanceledException)
					{
						// The operation was canceled
					}
					catch (Exception)
					{
						// Load failed
					}
				},
				op);
		}

		#endregion
	}
}
