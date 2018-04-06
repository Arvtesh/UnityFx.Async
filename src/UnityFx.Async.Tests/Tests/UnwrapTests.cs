// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class UnwrapTests
	{
		[Fact]
		public async Task Unwrap_SucceedsIfBothOperationsSucceed()
		{
			// Arrange
			var op1 = AsyncResult.Delay(1);
			var op2 = AsyncResult.FromResult(op1);

			// Act
			var op = op2.Unwrap();
			await op;

			// Assert
			Assert.True(op.IsCompletedSuccessfully);
			Assert.True(op1.IsCompleted);
			Assert.True(op2.IsCompleted);
		}

		[Fact]
		public async Task Unwrap_FailsIfInnerOperationFails()
		{
			// Arrange
			var expectedException = new Exception();
			var actualException = default(Exception);
			var op1 = AsyncResult.FromException(expectedException);
			var op2 = AsyncResult.FromResult(op1);
			var op = op2.Unwrap();

			// Act
			try
			{
				await op;
			}
			catch (Exception e)
			{
				actualException = e;
			}

			// Assert
			Assert.True(op.IsFaulted);
			Assert.Equal(expectedException, actualException);
		}

		[Fact]
		public async Task Unwrap_FailsIfOuterOperationFails()
		{
			// Arrange
			var actualException = default(Exception);
			var op1 = AsyncResult.FromCanceled<AsyncResult>();
			var op = op1.Unwrap();

			// Act
			try
			{
				await op;
			}
			catch (OperationCanceledException e)
			{
				actualException = e;
			}

			// Assert
			Assert.True(op.IsCanceled);
			Assert.NotNull(actualException);
		}

		[Fact]
		public async Task Unwrap_ReturnsInnerResult()
		{
			// Arrange
			var op1 = AsyncResult.FromResult(1);
			var op2 = AsyncResult.FromResult(op1);

			// Act
			var op = op2.Unwrap();
			await op;

			// Assert
			Assert.True(op.IsCompletedSuccessfully);
			Assert.Equal(1, op.Result);
		}
	}
}
