// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Xunit;

namespace UnityFx.Async
{
	public class ObservableTests
	{
		[Fact]
		public void ToObservable_OnNextIsCalled()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.Subscribe(observer);

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
			var observable = op.Subscribe(observer);

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
			var observable = op.Subscribe(observer);

			// Act
			op.SetCanceled();

			// Assert
			Assert.Equal(1, observer.OnErrorCount);
		}

		[Fact]
		public void ToObservable_OnCompletedIsNotCalledOnError()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>();
			var observer = new Observer<int>();
			var observable = op.Subscribe(observer);

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
			var observable = op.Subscribe(observer);

			// Act
			op.SetException(e);

			// Assert
			Assert.Equal(1, observer.OnErrorCount);
			Assert.Equal(e, observer.Exception);
		}
	}
}
