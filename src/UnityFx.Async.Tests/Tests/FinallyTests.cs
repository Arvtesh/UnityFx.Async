// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async.Promises
{
	public class FinallyTests
	{
		[Fact]
		public async Task Finally_DelegateIsCalledOnSuccess()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.Delay(1).Finally(() => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}

		[Fact]
		public async Task Finally_DelegateIsCalledOnFailure()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromException(new Exception()).Finally(() => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}

		[Fact]
		public async Task Finally_DelegateIsCalledOnCancel()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromCanceled().Finally(() => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}
	}
}
