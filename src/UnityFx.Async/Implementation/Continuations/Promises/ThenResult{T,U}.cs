// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal class ThenResult<T, U> : AsyncResult<U>, IAsyncContinuation
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly object _successCallback;
		private readonly Action<Exception> _errorCallback;

		private IAsyncOperation _continuation;

		#endregion

		#region interface

		public ThenResult(IAsyncOperation op, object successCallback, Action<Exception> errorCallback)
			: base(AsyncOperationStatus.Running)
		{
			_op = op;
			_successCallback = successCallback;
			_errorCallback = errorCallback;

			op.AddContinuation(this);
		}

		protected virtual void InvokeSuccessCallback(IAsyncOperation op, bool completedSynchronously, object continuation)
		{
			switch (continuation)
			{
				case Action a:
					a.Invoke();
					TrySetCompleted(completedSynchronously);
					break;

				case Action<T> a1:
					a1.Invoke((op as IAsyncOperation<T>).Result);
					TrySetCompleted(completedSynchronously);
					break;

				case Func<IAsyncOperation<U>> f3:
					_continuation = f3();
					_continuation.AddContinuation(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<IAsyncOperation> f1:
					_continuation = f1();
					_continuation.AddContinuation(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation<U>> f4:
					_continuation = f4((op as IAsyncOperation<T>).Result);
					_continuation.AddContinuation(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation> f2:
					_continuation = f2((op as IAsyncOperation<T>).Result);
					_continuation.AddContinuation(op2 => TryCopyCompletionState(op2, false), null);
					break;

				default:
					TrySetCanceled(completedSynchronously);
					break;
			}
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			_op.Cancel();
			_continuation?.Cancel();
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool inline)
		{
			try
			{
				if (op.IsCompletedSuccessfully)
				{
					if (IsCancellationRequested)
					{
						TrySetCanceled(inline);
					}
					else
					{
						InvokeSuccessCallback(op, inline, _successCallback);
					}
				}
				else
				{
					_errorCallback?.Invoke(op.Exception.InnerException);
					TrySetException(op.Exception, inline);
				}
			}
			catch (Exception e)
			{
				TrySetException(e, inline);
			}
		}

		#endregion
	}
}
