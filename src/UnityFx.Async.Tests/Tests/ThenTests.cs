﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class ThenTests
	{
		[Fact]
		public async Task Then_DelegateIsCalledOnSuccess()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.Delay(1).Then(() => called = true);

			// Act
			await op;

			// Assert
			Assert.True(called);
		}

		[Fact]
		public async Task Then_DelegateIsNotCalledOnFailure()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromException(new Exception()).Then(() => called = true);

			// Act
			try
			{
				await op;
			}
			catch
			{
			}

			// Assert
			Assert.False(called);
		}

		[Fact]
		public async Task Then_DelegateIsNotCalledOnCancel()
		{
			// Arrange
			var called = false;
			var op = AsyncResult.FromCanceled().Then(() => called = true);

			// Act
			try
			{
				await op;
			}
			catch
			{
			}

			// Assert
			Assert.False(called);
		}
	}
}
