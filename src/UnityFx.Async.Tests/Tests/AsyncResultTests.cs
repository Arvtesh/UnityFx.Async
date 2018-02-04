// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Xunit;

namespace UnityFx.Async
{
	public class AsyncResultTests
	{
		#region constructors

		[Fact]
		public void DefaultConstructor_SetsStatusToCreated()
		{
			// Act
			var op = new AsyncResult();

			// Assert
			Assert.Equal(AsyncOperationStatus.Created, op.Status);
			Assert.False(op.IsCompleted);
			Assert.False(op.CompletedSynchronously);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
			Assert.Null(op.AsyncState);
		}

		[Fact]
		public void Constructor_SetsStatusToScheduled()
		{
			// Act
			var op = new AsyncResult(false);

			// Assert
			Assert.Equal(AsyncOperationStatus.Scheduled, op.Status);
			Assert.False(op.IsCompleted);
			Assert.False(op.CompletedSynchronously);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
			Assert.Null(op.AsyncState);
		}

		[Fact]
		public void Constructor_SetsStatusToRunning()
		{
			// Act
			var op = new AsyncResult(true);

			// Assert
			Assert.Equal(AsyncOperationStatus.Running, op.Status);
			Assert.False(op.IsCompleted);
			Assert.False(op.CompletedSynchronously);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
			Assert.Null(op.AsyncState);
		}

		[Fact]
		public void Constructor_SetsAsyncState()
		{
			// Arrange
			var state = new object();

			// Act
			var op = new AsyncResult(null, state);

			// Assert
			Assert.Equal(state, op.AsyncState);
			Assert.Equal(AsyncOperationStatus.Created, op.Status);
			Assert.False(op.IsCompleted);
			Assert.False(op.CompletedSynchronously);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		#endregion

		#region static methods

		[Fact]
		public void CompletedOperation_ReturnsCompletedOperation()
		{
			// Act
			var op = AsyncResult.CompletedOperation;

			// Assert
			AssertCompleted(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromCanceled_ReturnsCanceledOperation()
		{
			// Act
			var op = AsyncResult.FromCanceled();

			// Assert
			AssertCanceled(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromCanceled_ReturnsCanceledOperation_Generic()
		{
			// Act
			var op = AsyncResult.FromCanceled<int>();

			// Assert
			AssertCanceled(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromException_ReturnsFailedOperation()
		{
			// Arrange
			var e = new InvalidCastException();

			// Act
			var op = AsyncResult.FromException(e);

			// Assert
			AssertFaulted(op, e);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromException_ReturnsFailedOperation_Generic()
		{
			// Arrange
			var e = new InvalidCastException();

			// Act
			var op = AsyncResult.FromException<int>(e);

			// Assert
			AssertFaulted(op, e);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromResult_ReturnsCompletedOperation()
		{
			// Arrange
			var result = 25;

			// Act
			var op = AsyncResult.FromResult(result);

			// Assert
			AssertCompletedWithResult(op, result);
			Assert.True(op.CompletedSynchronously);
		}

		#endregion

		#region IAsyncCompletionSource

		[Fact]
		public void SetCanceled_SetsStatusToCanceled()
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			op.SetCanceled(true);

			// Assert
			AssertCanceled(op);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void SetCanceled_SetsCompletedSynchronously(bool completedSynchronously)
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			op.SetCanceled(completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void SetCanceled_ThrowsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult();
			op.SetCompleted(false);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCanceled(true));
			Assert.False(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetCanceled_SetsStatusToCanceled()
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			var result = op.TrySetCanceled(true);

			// Assert
			AssertCanceled(op);
			Assert.True(result);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TrySetCanceled_SetsCompletedSynchronously(bool completedSynchronously)
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			op.TrySetCanceled(completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetCanceled_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult();
			op.SetCompleted(false);

			// Act
			var result = op.TrySetCanceled(true);

			// Assert
			Assert.False(result);
			Assert.False(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		#endregion

		#region implementation

		private void AssertCompleted(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.RanToCompletion, op.Status);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		private void AssertCompletedWithResult<T>(IAsyncOperation<T> op, T result)
		{
			Assert.Equal(AsyncOperationStatus.RanToCompletion, op.Status);
			Assert.Equal(result, op.Result);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		private void AssertCanceled(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.Canceled, op.Status);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCanceled);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		private void AssertFaulted(IAsyncOperation op, Exception e)
		{
			Assert.Equal(AsyncOperationStatus.Faulted, op.Status);
			Assert.Equal(e, op.Exception);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsFaulted);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
		}

		#endregion
	}
}
