// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using NSubstitute;
using Xunit;

namespace UnityFx.Async
{
	public class AsyncUpdateSourceTests
	{
		[Fact]
		public void OnNext_CallsUpdate()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IAsyncUpdatable>();

			updateSource.AddListener(observer);

			// Act
			updateSource.OnNext(0);

			// Assert
			observer.Received().Update(0);
		}

		[Fact]
		public void OnNext_CallsNext()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IObserver<float>>();

			updateSource.Subscribe(observer);

			// Act
			updateSource.OnNext(0);

			// Assert
			observer.Received().OnNext(0);
		}

		[Fact]
		public void OnCompleted_CallsCompleted()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IObserver<float>>();

			updateSource.Subscribe(observer);

			// Act
			updateSource.OnCompleted();

			// Assert
			observer.Received().OnCompleted();
		}

		[Fact]
		public void OnError_CallsError()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IObserver<float>>();
			var e = new Exception();

			updateSource.Subscribe(observer);

			// Act
			updateSource.OnError(e);

			// Assert
			observer.Received().OnError(e);
		}

		[Fact]
		public void Dispose_CallsCompleted()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IObserver<float>>();

			updateSource.Subscribe(observer);

			// Act
			updateSource.Dispose();

			// Assert
			observer.Received().OnCompleted();
		}

		[Fact]
		public void RemoveListener_CanBeCalledFromUpdate()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IAsyncUpdatable>();

			observer.When(x => x.Update(Arg.Any<float>())).Do(x => updateSource.RemoveListener(observer));
			updateSource.AddListener(observer);

			// Act
			updateSource.OnNext(0);
			updateSource.OnNext(0);

			// Assert
			observer.Received(1).Update(Arg.Any<float>());
		}

		[Fact]
		public void Unsubscribe_CanBeCalledFromUpdate()
		{
			// Arrange
			var updateSource = new AsyncUpdateSource();
			var observer = Substitute.For<IObserver<float>>();
			var subscription = updateSource.Subscribe(observer);

			observer.When(x => x.OnNext(Arg.Any<float>())).Do(x => subscription.Dispose());

			// Act
			updateSource.OnNext(0);
			updateSource.OnNext(0);

			// Assert
			observer.Received(1).OnNext(Arg.Any<float>());
		}
	}
}
