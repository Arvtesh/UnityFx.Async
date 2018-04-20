// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async.Promises
{
	public class ThenAnyTests
	{
		[Fact]
		public async Task ThenAny_CompletesWhenAllOperationsComplete()
		{
			// Arrange
			var op1 = AsyncResult.Delay(1);
			var op2 = AsyncResult.Delay(2);
			var op3 = new AsyncResult();

			// Act
			await op1.ThenAny(() => new IAsyncOperation[] { op2, op3 });

			// Assert
			Assert.True(op1.IsCompleted);
			Assert.True(op2.IsCompleted);
		}

		[Fact]
		public async Task ThenAny_CompletesWhenCanceled()
		{
			// Arrange
			var cs = new CancellationTokenSource();
			cs.Cancel();

			var op1 = new AsyncCompletionSource();
			var op2 = new AsyncCompletionSource();
			var op = AsyncResult.CompletedOperation.ThenAny(() => new IAsyncOperation[] { op2, op1 }).WithCancellation(cs.Token);

			// Act
			try
			{
				await op;
			}
			catch (OperationCanceledException)
			{
			}

			// Assert
			Assert.True(op.IsCanceled);
			Assert.True(op1.IsCanceled);
			Assert.True(op2.IsCanceled);
		}
	}
}
