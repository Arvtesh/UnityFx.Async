// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace UnityFx.Async
{
	public class CompletionCallbackTests
	{
		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			op.SetCanceled();

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompletedSynchronously()
		{
			// Arrange
			var op = AsyncResult.CompletedOperation;

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task TryAddCompletionCallback_ExecutesWhenOperationCompletes()
		{
			// Arrange
			var op = AsyncResult.Delay(1);
			var callbackCalled = false;

			op.TryAddCompletionCallback(_ => callbackCalled = true, null);

			// Act
			await op;

			// Assert
			Assert.True(callbackCalled);
		}

		[Fact]
		public void TryAddContinuation_ExecutesWhenOperationCompletes()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var continuation = Substitute.For<IAsyncContinuation>();
			op.TryAddContinuation(continuation);

			// Act
			op.SetCompleted();

			// Assert
			continuation.Received(1).Invoke(op);
		}
	}
}
