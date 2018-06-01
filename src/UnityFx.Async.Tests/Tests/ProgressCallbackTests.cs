// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using NSubstitute;
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

		[Theory]
		[InlineData(AsyncOperationStatus.Created, 0)]
		[InlineData(AsyncOperationStatus.Scheduled, 0)]
		[InlineData(AsyncOperationStatus.RanToCompletion, 1)]
		[InlineData(AsyncOperationStatus.Faulted, 0)]
		[InlineData(AsyncOperationStatus.Canceled, 0)]
		public void Progress_ReturnsCorrentValue(AsyncOperationStatus status, float expectedValue)
		{
			// Arrange
			var progress = 0.3f;
			var op = new AsyncCompletionSource(status);

			// Act
			op.TrySetProgress(progress);

			// Assert
			Assert.Equal(expectedValue, op.Progress);
		}

		[Fact]
		public void SetProgress_SetsCorrectValue()
		{
			// Arrange
			var progress = 0.7f;
			var op = new AsyncCompletionSource(AsyncOperationStatus.Running);

			// Act
			op.SetProgress(progress);

			// Assert
			Assert.Equal(progress, op.Progress);
		}

		[Fact]
		public void SetProgress_RaisesProgressChanged()
		{
			// Arrange
			var asyncCallbackCalled = false;
			var op = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.ProgressChanged += (sender, args) =>
			{
				asyncCallbackCalled = true;
			};

			// Act
			op.SetProgress(0.8f);

			// Assert
			Assert.True(asyncCallbackCalled);
			
		}

		[Fact]
		public void SetCompleted_RaisesProgressChanged()
		{
			// Arrange
			var asyncCallbackCalled = false;
			var asyncCallbackCalled2 = false;
			var progress = 0;
			var progress2 = 0f;
			var op = new AsyncCompletionSource();
			var p = Substitute.For<IProgress<float>>();

			op.ProgressChanged += (sender, args) =>
			{
				asyncCallbackCalled = true;
				progress = args.ProgressPercentage;
			};

			op.AddProgressCallback(
				asyncOp =>
				{
					asyncCallbackCalled2 = true;
					progress2 = asyncOp.Progress;
				},
				null);

			op.AddProgressCallback(p, null);

			// Act
			op.SetCompleted();

			// Assert
			Assert.True(asyncCallbackCalled);
			Assert.True(asyncCallbackCalled2);
			Assert.Equal(100, progress);
			Assert.Equal(1, progress2);
			p.Received(1).Report(1);
		}

		#endregion
	}
}
