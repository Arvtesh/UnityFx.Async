// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async.Promises
{
	public class SequenceTests
	{
		[Fact]
		public async Task Sequence_CompletesWhenAllOperationsComplete()
		{
			// Arrange
			var op1 = AsyncResult.Delay(1);
			var op2 = AsyncResult.Delay(2);

			// Act
			await Promise.Sequence(() => op1, () => op2);

			// Assert
			Assert.True(op1.IsCompleted);
			Assert.True(op2.IsCompleted);
		}

		[Fact]
		public async Task Sequence_ExecutesOperationsSequentally()
		{
			// Arrange
			var counter = 0;
			var op1Counter = 0;
			var op2Counter = 0;

			// Act
			await Promise.Sequence(
				() =>
				{
					op1Counter = counter++;
					return AsyncResult.CompletedOperation;
				},
				() =>
				{
					op2Counter = counter++;
					return AsyncResult.CompletedOperation;
				});

			// Assert
			Assert.Equal(0, op1Counter);
			Assert.Equal(1, op2Counter);
		}

		[Fact]
		public async Task Sequence_FailsIfAnyOperationFails()
		{
			// Arrange
			var op1 = AsyncResult.Delay(1);
			var op2 = Promise.Sequence(() => op1, () => throw new NotImplementedException());

			// Act
			try
			{
				await op2;
			}
			catch (NotImplementedException)
			{
			}

			// Assert
			Assert.True(op1.IsCompleted);
			Assert.True(op2.IsFaulted);
		}
	}
}
