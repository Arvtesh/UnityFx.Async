// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
#if !NET35

	/// <summary>
	/// Helper class used for <see cref="AsyncExtensions.ToObservable{T}(IAsyncOperation{T})"/> implementation.
	/// </summary>
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
			Action d = () =>
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

			_op.AddOrInvokeCompletionCallback(d);

			return new AsyncObservableSubscription(_op, d);
		}

		#endregion
	}

#endif
}
