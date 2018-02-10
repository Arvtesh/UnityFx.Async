// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
#if !NET35

	internal class AsyncObservable<T> : IObservable<T>
	{
		#region data

		private readonly IAsyncOperation<T> _op;

		#endregion

		#region interface

		public AsyncObservable(IAsyncOperation<T> op)
		{
			_op = op;
		}

		#endregion

		#region IObservable

		public IDisposable Subscribe(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			AsyncOperationCallback d = op =>
			{
				if (_op.IsCompletedSuccessfully)
				{
					observer.OnNext(_op.Result);
					observer.OnCompleted();
				}
				else if (_op.IsFaulted)
				{
					observer.OnError(_op.Exception);
				}
				else
				{
					observer.OnCompleted();
				}
			};

			if (_op.TryAddCompletionCallback(d, null))
			{
				return new AsyncObservableSubscription(_op, d);
			}
			else
			{
				return EmptyDisposable.Instance;
			}
		}

		#endregion
	}

#endif
}
