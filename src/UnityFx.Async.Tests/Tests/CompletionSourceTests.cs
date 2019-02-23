// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Xunit;

namespace UnityFx.Async
{
	public class CompletionSourceTests
	{
		#region SetCanceled/TrySetCanceled

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetCanceled_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCanceled());
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetCanceled_SetsStatusToCanceled(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetCanceled();

			// Assert
			Assert.True(op.IsCanceled);
			Assert.True(result);
		}

		[Fact]
		public void TrySetCanceled_RaisesCompletionCallbacks()
		{
			// Arrange
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(new Action(() => asyncCallbackCalled2 = true), null);

			// Act
			op.TrySetCanceled();

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetCanceled_CallsOnCompleted()
		{
			// Arrange
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetCanceled();

			// Assert
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Fact]
		public void TrySetCanceled_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);

			// Act
			var result = op.TrySetCanceled();

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			Assert.True(op.IsCompletedSuccessfully);
		}

		[Fact]
		public void TrySetCanceled_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCanceled());
		}

		#endregion

		#region SetException/TrySetException

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetException_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetException(e));
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetException_SetsStatusToFaulted(AsyncOperationStatus status)
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetException(e);

			// Assert
			Assert.True(op.IsFaulted);
			Assert.Equal(e, op.Exception);
			Assert.True(result);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetException_SetsStatusToCanceled(AsyncOperationStatus status)
		{
			// Arrange
			var e = new OperationCanceledException();
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetException(e);

			// Assert
			Assert.True(op.IsCanceled);
			Assert.True(result);
			Assert.Equal(e, op.Exception);
		}

		[Fact]
		public void TrySetException_RaisesCompletionCallbacks()
		{
			// Arrange
			var e = new Exception();
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(new Action(() => asyncCallbackCalled2 = true), null);

			// Act
			op.TrySetException(e);

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetException_CallsOnCompleted()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetException(e);

			// Assert
			Assert.Equal(e, op.OnCompletedException);
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Fact]
		public void TrySetException_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);

			// Act
			var result = op.TrySetCanceled();

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			Assert.True(op.IsCompletedSuccessfully);
		}

		[Fact]
		public void TrySetException_ThrowsIfExceptionIsNull()
		{
			// Arrange
			var op = new AsyncCompletionSource();

			// Act/Assert
			Assert.Throws<ArgumentNullException>(() => op.TrySetException(null));
		}

		[Fact]
		public void TrySetException_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetException(e));
		}

		#endregion

		#region SetCompleted/TrySetCompleted

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetCompleted_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCompleted());
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetCompleted_SetsStatusToRanToCompletion(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetCompleted();

			// Assert
			Assert.True(op.IsCompletedSuccessfully);
			Assert.True(result);
		}

		[Fact]
		public void TrySetCompleted_RaisesCompletionCallbacks()
		{
			// Arrange
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(new Action(() => asyncCallbackCalled2 = true), null);

			// Act
			op.TrySetCompleted();

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetCompleted_CallsOnCompleted()
		{
			// Arrange
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetCompleted();

			// Assert
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Fact]
		public void TrySetCompleted_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.Canceled);

			// Act
			var result = op.TrySetCompleted();

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			Assert.True(op.IsCanceled);
		}

		[Fact]
		public void TrySetCompleted_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCompleted());
		}

		#endregion

		#region SetResult/TrySetResult

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetResult_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource<int>(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetResult(10));
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetResult_SetsStatusToRanToCompletion(AsyncOperationStatus status)
		{
			// Arrange
			var resultValue = new object();
			var op = new AsyncCompletionSource<object>(status);

			// Act
			var result = op.TrySetResult(resultValue);

			// Assert
			Assert.True(op.IsCompletedSuccessfully);
			Assert.Equal(resultValue, op.Result);
			Assert.True(result);
		}

		[Fact]
		public void TrySetResult_RaisesCompletionCallbacks()
		{
			// Arrange
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource<int>(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(new Action(() => asyncCallbackCalled2 = true), null);

			// Act
			op.TrySetResult(10);

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetResult_CallsOnCompleted()
		{
			// Arrange
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetResult(10);

			// Assert
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Fact]
		public void TrySetResult_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>(AsyncOperationStatus.Canceled);

			// Act
			var result = op.TrySetResult(10);

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			Assert.True(op.IsCanceled);
		}

		[Fact]
		public void TrySetResult_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetResult(15));
		}

		#endregion
	}
}
