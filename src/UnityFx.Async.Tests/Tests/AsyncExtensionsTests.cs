// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class AsyncExtensionsTests
	{
		#region ContinueWith

		[Fact]
		public async Task ContinueWith_CompletesWhenBothOperationsComplete()
		{
			// Arrange
			var op = AsyncResult.Delay(10);
			var op2 = op.ContinueWith((asyncResult, cs) =>
			{
				Task.Run(() =>
				{
					Thread.Sleep(10);
					cs.SetCompleted();
				});
			});

			// Act
			await op2;

			// Assert
			Assert.True(op.IsCompleted);
			Assert.True(op2.IsCompleted);
		}

		#endregion
	}
}
