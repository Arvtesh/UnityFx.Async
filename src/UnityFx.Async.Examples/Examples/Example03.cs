// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Net;

namespace UnityFx.Async.Examples
{
	/// <summary>
	/// Example03: AsyncCompletionSource.
	/// </summary>
	internal class Example03 : Example
	{
		public override string GetName()
		{
			return typeof(AsyncCompletionSource).Name;
		}

		public override void Run()
		{
			// NOTE: The operation should be disposed because of the Wait() call.
			using (var op = DownloadAsTextAsync("http://www.google.com"))
			{
				op.Completed += (sender, args) =>
				{
					if (op.IsCompletedSuccessfully)
					{
						var s = op.Result;

						if (s.Length > 128)
						{
							s = s.Substring(0, 128) + "...";
						}

						Console.WriteLine("Download result: " + s);
					}
				};

				op.Wait();
			}
		}

		private IAsyncOperation<string> DownloadAsTextAsync(string url)
		{
			var op = new AsyncCompletionSource<string>();
			var webClient = new WebClient();

			webClient.DownloadStringCompleted += (sender, args) =>
			{
				webClient.Dispose();

				if (args.Error != null)
				{
					Console.WriteLine("Download failed: " + args.Error.Message);
					op.SetException(args.Error);
				}
				else
				{
					Console.WriteLine("Download completed.");
					op.SetResult(args.Result);
				}
			};

			webClient.DownloadProgressChanged += (sender, args) =>
			{
				Console.WriteLine("Download progress: " + args.ProgressPercentage + "%.");
				op.TrySetProgress(args.ProgressPercentage / 100f);
			};

			Console.WriteLine("Downloading " + url + ".");
			webClient.DownloadStringAsync(new Uri(url));

			return op.Operation;
		}
	}
}
