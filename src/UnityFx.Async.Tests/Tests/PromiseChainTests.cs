// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async.Promises
{
	public class PromiseChainTests
	{
		[Fact]
		public async Task Catch_CalledIfThenThrows()
		{
			// Arrange
			var op = AsyncResult.Delay(10);
			var thenOp = op.Then(() => throw new Exception());
			var catchCalled = false;
			var catchOp = thenOp.Catch(e => catchCalled = true);

			// Act
			await catchOp;

			// Assert
			Assert.True(thenOp.IsFaulted);
			Assert.IsType<Exception>(thenOp.Exception);
			Assert.True(catchOp.IsCompletedSuccessfully);
			Assert.True(catchCalled);
		}

		[Fact]
		public async Task Catch_CalledIfThenFails()
		{
			// Arrange
			var op = AsyncResult.Delay(10);
			var thenOp = op.Then(() => AsyncResult.FaultedOperation);
			var catchCalled = false;
			var catchOp = thenOp.Catch(e => catchCalled = true);

			// Act
			await catchOp;

			// Assert
			Assert.True(thenOp.IsFaulted);
			Assert.IsType<Exception>(thenOp.Exception);
			Assert.True(catchOp.IsCompletedSuccessfully);
			Assert.True(catchCalled);
		}

		[Fact]
		public async Task Then_NotCalledIfPreviousThenThrows()
		{
			// Arrange
			var op = AsyncResult.Delay(10);
			var thenOp = op.Then(() => throw new Exception());
			var thenCalled = false;
			var thenOp2 = thenOp.Then(() => thenCalled = true);
			var catchCalled = false;
			var catchOp = thenOp2.Catch(e => catchCalled = true);

			// Act
			await catchOp;

			// Assert
			Assert.True(thenOp.IsFaulted);
			Assert.True(thenOp2.IsFaulted);
			Assert.True(catchOp.IsCompletedSuccessfully);
			Assert.True(catchCalled);
			Assert.False(thenCalled);
		}

		[Fact]
		public async Task Then_NotCalledIfPreviousThenFails()
		{
			// Arrange
			var op = AsyncResult.Delay(10);
			var thenOp = op.Then(() => AsyncResult.FaultedOperation);
			var thenCalled = false;
			var thenOp2 = thenOp.Then(() => thenCalled = true);
			var catchCalled = false;
			var catchOp = thenOp2.Catch(e => catchCalled = true);

			// Act
			await catchOp;

			// Assert
			Assert.True(thenOp.IsFaulted);
			Assert.True(thenOp2.IsFaulted);
			Assert.True(catchOp.IsCompletedSuccessfully);
			Assert.True(catchCalled);
			Assert.False(thenCalled);
		}
	}
}
