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
				if (op.IsCompletedSuccessfully)
				{
					observer.OnNext((op as IAsyncOperation<T>).Result);
					observer.OnCompleted();
				}
				else if (op.IsFaulted)
				{
					observer.OnError(op.Exception);
				}
				else
				{
					observer.OnCompleted();
				}
			};

			if (_op.TryAddContinuation(d, null))
			{
				return new AsyncObservableSubscription(_op, d);
			}
			else
			{
				return Disposable.Empty;
			}
		}

		#endregion
	}

#endif
}
