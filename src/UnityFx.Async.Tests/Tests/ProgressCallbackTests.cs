// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Xunit;

namespace UnityFx.Async
{
	public class ProgressSourceTests
	{
		#region SetProgress/TrySetProgress

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetProgress_ThrowsIfOperationIsNotRunning(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetProgress(0.1f));
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, false)]
		[InlineData(AsyncOperationStatus.Scheduled, false)]
		[InlineData(AsyncOperationStatus.Running, true)]
		[InlineData(AsyncOperationStatus.RanToCompletion, false)]
		[InlineData(AsyncOperationStatus.Faulted, false)]
		[InlineData(AsyncOperationStatus.Canceled, false)]
		public void TrySetProgress_ReturnsCorrentValue(AsyncOperationStatus status, bool expectedResult)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetProgress(1);

			// Assert
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void SetCompleted_RaisesProgressCallbacks()
		{
			// Arrange
			var asyncCallbackCalled = false;
			var progress = 0f;
			var op = new AsyncCompletionSource();

			op.AddProgressCallback(
				asyncOp =>
				{
					asyncCallbackCalled = true;
					progress = asyncOp.Progress;
				},
				null);

			// Act
			op.SetCompleted();

			// Assert
			Assert.True(asyncCallbackCalled);
			Assert.Equal(1, progress);
		}

		[Fact]
		public void SetCompleted_RaisesProgressChanged()
		{
			// Arrange
			var asyncCallbackCalled = false;
			var progress = 0;
			var op = new AsyncCompletionSource();

			op.ProgressChanged += (sender, args) =>
			{
				asyncCallbackCalled = true;
				progress = args.ProgressPercentage;
			};

			// Act
			op.SetCompleted();

			// Assert
			Assert.True(asyncCallbackCalled);
			Assert.Equal(100, progress);
		}

		#endregion
	}
}
