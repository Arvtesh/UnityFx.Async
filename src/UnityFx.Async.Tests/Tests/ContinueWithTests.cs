// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class ContinuWithTests
	{
		[Theory]
		[InlineData(AsyncContinuationOptions.None, true)]
		[InlineData(AsyncContinuationOptions.NotOnRanToCompletion, false)]
		[InlineData(AsyncContinuationOptions.NotOnFaulted, true)]
		[InlineData(AsyncContinuationOptions.NotOnCanceled, true)]
		[InlineData(AsyncContinuationOptions.OnlyOnRanToCompletion, true)]
		[InlineData(AsyncContinuationOptions.OnlyOnFaulted, false)]
		[InlineData(AsyncContinuationOptions.OnlyOnCanceled, false)]
		public async Task ContinueWth_ExecutesOnAntecedentSuccess(AsyncContinuationOptions options, bool expectedCalled)
		{
			// Arrange
			var op = AsyncResult.Delay(1);
			var continuationCalled = false;
			var continuationCanceled = false;
			var continuation = op.ContinueWith(o => continuationCalled = true, options);

			// Act
			try
			{
				await continuation;
			}
			catch (OperationCanceledException)
			{
				continuationCanceled = true;
			}

			// Assert
			Assert.Equal(expectedCalled, continuationCalled);
			Assert.Equal(!expectedCalled, continuationCanceled);
		}

		[Theory]
		[InlineData(AsyncContinuationOptions.None, true)]
		[InlineData(AsyncContinuationOptions.NotOnRanToCompletion, true)]
		[InlineData(AsyncContinuationOptions.NotOnFaulted, false)]
		[InlineData(AsyncContinuationOptions.NotOnCanceled, true)]
		[InlineData(AsyncContinuationOptions.OnlyOnRanToCompletion, false)]
		[InlineData(AsyncContinuationOptions.OnlyOnFaulted, true)]
		[InlineData(AsyncContinuationOptions.OnlyOnCanceled, false)]
		public async Task ContinueWth_ExecutesOnAntecedentFailure(AsyncContinuationOptions options, bool expectedCalled)
		{
			// Arrange
			var op = AsyncResult.FromException(new Exception());
			var continuationCalled = false;
			var continuationCanceled = false;
			var continuation = op.ContinueWith(o => continuationCalled = true, options);

			// Act
			try
			{
				await continuation;
			}
			catch (OperationCanceledException)
			{
				continuationCanceled = true;
			}

			// Assert
			Assert.Equal(expectedCalled, continuationCalled);
			Assert.Equal(!expectedCalled, continuationCanceled);
		}

		[Theory]
		[InlineData(AsyncContinuationOptions.None, true)]
		[InlineData(AsyncContinuationOptions.NotOnRanToCompletion, true)]
		[InlineData(AsyncContinuationOptions.NotOnFaulted, true)]
		[InlineData(AsyncContinuationOptions.NotOnCanceled, false)]
		[InlineData(AsyncContinuationOptions.OnlyOnRanToCompletion, false)]
		[InlineData(AsyncContinuationOptions.OnlyOnFaulted, false)]
		[InlineData(AsyncContinuationOptions.OnlyOnCanceled, true)]
		public async Task ContinueWth_ExecutesOnAntecedentCancellation(AsyncContinuationOptions options, bool expectedCalled)
		{
			// Arrange
			var op = AsyncResult.FromCanceled();
			var continuationCalled = false;
			var continuationCanceled = false;
			var continuation = op.ContinueWith(o => continuationCalled = true, options);

			// Act
			try
			{
				await continuation;
			}
			catch (OperationCanceledException)
			{
				continuationCanceled = true;
			}

			// Assert
			Assert.Equal(expectedCalled, continuationCalled);
			Assert.Equal(!expectedCalled, continuationCanceled);
		}
	}
}
