// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class ToTaskTests
	{

		[Fact]
		public async Task ToTask_CompletesWhenSourceCompletes()
		{
			// Arrange
			var op = AsyncResult.Delay(1);
			var task = op.ToTask();

			// Act
			await task;

			// Assert
			Assert.True(op.IsCompleted);
		}

		[Fact]
		public async Task ToTask_FailsWhenSourceFails()
		{
			// Arrange
			var op = AsyncResult.FromException(new Exception());
			var task = op.ToTask();

			// Act/Assert
			await Assert.ThrowsAsync<Exception>(() => task);
		}

		[Fact]
		public async Task ToTask_FailsWhenSourceIsCanceled()
		{
			// Arrange
			var op = AsyncResult.FromCanceled();
			var task = op.ToTask();

			// Act/Assert
			await Assert.ThrowsAsync<TaskCanceledException>(() => task);
		}
	}
}
