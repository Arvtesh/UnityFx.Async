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
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetProgress_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetProgress(0.1f));
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, true)]
		[InlineData(AsyncOperationStatus.Scheduled, true)]
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
			var pc = Substitute.For<IProgress<float>>();

			op.ProgressChanged += (sender, args) =>
			{
				asyncCallbackCalled = true;
				progress = args.ProgressPercentage;
			};

			op.AddProgressCallback(
				new Action<float>(p =>
				{
					asyncCallbackCalled2 = true;
					progress2 = p;
				}),
				null);

			op.AddProgressCallback(pc, null);

			// Act
			op.SetCompleted();

			// Assert
			Assert.True(asyncCallbackCalled);
			Assert.True(asyncCallbackCalled2);
			Assert.Equal(100, progress);
			Assert.Equal(1, progress2);
			pc.Received(1).Report(1);
		}

		#endregion
	}
}
