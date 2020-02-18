// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal class ThenResult<T, U> : AsyncResult<U>
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

			op.AddCompletionCallback(this);
		}

		protected virtual IAsyncOperation InvokeSuccessCallback(IAsyncOperation op, object continuation)
		{
			IAsyncOperation result = null;

			switch (continuation)
			{
				case Action a:
					{
						a.Invoke();
						TrySetCompleted();
						break;
					}

				case Action<T> a1:
					{
						a1.Invoke(((IAsyncOperation<T>)op).Result);
						TrySetCompleted();
						break;
					}

				case Func<IAsyncOperation<U>> f3:
					{
						result = f3();
						result.AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
						break;
					}

				case Func<IAsyncOperation> f1:
					{
						result = f1();
						result.AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
						break;
					}

				case Func<T, IAsyncOperation<U>> f4:
					{
						result = f4(((IAsyncOperation<T>)op).Result);
						result.AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
						break;
					}

				case Func<T, IAsyncOperation> f2:
					{
						result = f2(((IAsyncOperation<T>)op).Result);
						result.AddCompletionCallback(new Action<IAsyncOperation>(op2 => TryCopyCompletionState(op2, false)), null);
						break;
					}

				default:
					{
						TrySetCanceled();
						break;
					}
			}

			return result;
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

		public override void Invoke(IAsyncOperation op)
		{
			try
			{
				if (op.IsCompletedSuccessfully)
				{
					if (IsCancellationRequested)
					{
						TrySetCanceled();
					}
					else
					{
						_continuation = InvokeSuccessCallback(op, _successCallback);
					}
				}
				else
				{
					_errorCallback?.Invoke(op.Exception);
					TrySetException(op.Exception);
				}
			}
			catch (Exception e)
			{
				TrySetException(e);
			}
		}

		#endregion
	}
}
