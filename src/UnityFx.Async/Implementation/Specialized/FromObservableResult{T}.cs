// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
#if !NET35

	internal sealed class FromObservableResult<T> : AsyncResult<T>, IObserver<T>
	{
		#region data

		private readonly IDisposable _subscription;

		#endregion

		#region interface

		internal FromObservableResult(IObservable<T> observable)
			: base(AsyncOperationStatus.Running)
		{
			_subscription = observable.Subscribe(this);
		}

		#endregion

		#region AsyncResult

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscription.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region IObserver

		void IObserver<T>.OnNext(T value)
		{
			_subscription.Dispose();
			TrySetResult(value, false);
		}

		void IObserver<T>.OnError(Exception error)
		{
			_subscription.Dispose();
			TrySetException(error, false);
		}

		void IObserver<T>.OnCompleted()
		{
		}

		#endregion
	}

#endif
}
