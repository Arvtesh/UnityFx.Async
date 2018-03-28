// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class AsyncExtensionsTests
	{
		#region ToObservable

		[Fact]
		public void ToObservable_OnNextIsCalled()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.ToObservable().Subscribe(observer);

			// Act
			op.SetResult(10);

			// Assert
			Assert.Equal(1, observer.OnNextCount);
			Assert.Equal(10, observer.Result);
		}

		[Fact]
		public void ToObservable_OnCompletedIsCalledAfterOnNext()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.ToObservable().Subscribe(observer);

			// Act
			op.SetResult(10);

			// Assert
			Assert.Equal(1, observer.OnCompletedCount);
		}

		[Fact]
		public void ToObservable_OnCompletedIsCalledOnCancellation()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.ToObservable().Subscribe(observer);

			// Act
			op.SetCanceled();

			// Assert
			Assert.Equal(1, observer.OnCompletedCount);
		}

		[Fact]
		public void ToObservable_OnCompletedIsNotCalledOnError()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.ToObservable().Subscribe(observer);

			// Act
			op.SetException(new Exception());

			// Assert
			Assert.Equal(0, observer.OnCompletedCount);
		}

		[Fact]
		public void ToObservable_OnErrorIsCalled()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.ToObservable().Subscribe(observer);

			// Act
			op.SetException(e);

			// Assert
			Assert.Equal(1, observer.OnErrorCount);
			Assert.Equal(e, observer.Exception);
		}

		#endregion

		#region ContinueWith

		[Fact]
		public async Task ContinueWith_CompletesWhenBothOperationsComplete()
		{
			// Arrange
			var op = AsyncResult.Delay(10);
			var op2 = op.ContinueWith((asyncResult, cs) =>
			{
				Task.Run(() =>
				{
					Thread.Sleep(10);
					cs.SetCompleted();
				});
			});

			// Act
			await op2;

			// Assert
			Assert.True(op.IsCompleted);
			Assert.True(op2.IsCompleted);
		}

		#endregion

		#region ToTask

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
			var op = AsyncResult.Delay(1).ContinueWith(result => AsyncResult.FromException(new Exception()));
			var task = op.ToTask();

			// Act/Assert
			await Assert.ThrowsAsync<Exception>(() => task);
		}

		[Fact]
		public async Task ToTask_FailsWhenSourceIsCanceled()
		{
			// Arrange
			var op = AsyncResult.Delay(1).ContinueWith(result => AsyncResult.FromCanceled());
			var task = op.ToTask();

			// Act/Assert
			await Assert.ThrowsAsync<TaskCanceledException>(() => task);
		}

		#endregion
	}
}
