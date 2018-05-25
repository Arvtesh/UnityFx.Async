// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
#if !NET35

	internal class AsyncObservableSubscription<T> : IAsyncContinuation, IDisposable
	{
		#region data

		private readonly IAsyncOperation<T> _op;
		private readonly IObserver<T> _observer;

		#endregion

		#region interface

		public AsyncObservableSubscription(IAsyncOperation<T> op, IObserver<T> observer)
		{
			_op = op;
			_observer = observer;
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool inline)
		{
			if (_op.IsCompletedSuccessfully)
			{
				_observer.OnNext(_op.Result);
				_observer.OnCompleted();
			}
			else
			{
				_observer.OnError(_op.Exception);
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			_op.RemoveContinuation(this);
		}

		#endregion
	}

#endif
}
