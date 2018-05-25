// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async.Promises
{
	public class CatchTests
	{
		[Fact]
		public async Task Catch_DelegateIsNotCalledOnSuccess()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.Delay(1).Catch(e => called = true);

			// Act
			await op;

			// Assert
			Assert.False(called);
		}

		[Fact]
		public async Task Catch_DelegateIsCalledOnFailure()
		{
			// Arrange
			Exception exception = null;
			var op0 = AsyncResult.FromException(new Exception());
			var op = op0.Catch(e => exception = e);

			// Act
			await op;

			// Assert
			Assert.Equal(op0.Exception, exception);
		}

		[Fact]
		public async Task Catch_DelegateIsCalledOnCancel()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromCanceled().Catch(e => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}

		[Fact]
		public async Task Catch_ExceptionIsFiltered()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromCanceled().Catch<OperationCanceledException>(e => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}

		[Fact]
		public async Task Catch_ExceptionIsFiltered_2()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromCanceled().Catch<Exception>(e => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}

		[Fact]
		public async Task Catch_ExceptionIsFiltered_3()
		{
			// Arrange
			var called = false;
			var exceptionThrown = false;
			var op = AsyncResult.FromCanceled().Catch<InvalidOperationException>(e => called = true);

			// Act
			try
			{
				await op;
			}
			catch (Exception)
			{
				exceptionThrown = true;
			}

			// Assert
			Assert.False(called);
			Assert.True(exceptionThrown);
		}
	}
}
