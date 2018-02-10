// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	public class Observer<T> : IObserver<T>
	{
		public int OnNextCount { get; private set; }
		public int OnErrorCount { get; private set; }
		public int OnCompletedCount { get; private set; }

		public bool IsCompleted { get; private set; }
		public Exception Exception { get; private set; }
		public T Result { get; private set; }

		public void OnNext(T value)
		{
			Result = value;
			OnNextCount += 1;
		}

		public void OnError(Exception error)
		{
			Exception = error;
			IsCompleted = true;
			OnErrorCount += 1;
		}

		public void OnCompleted()
		{
			IsCompleted = true;
			OnCompletedCount += 1;
		}
	}
}
