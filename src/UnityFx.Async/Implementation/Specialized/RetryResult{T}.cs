// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal abstract class RetryResult<T> : AsyncResult<T>
	{
		#region data

		private readonly object _opFactory;
		private readonly int _millisecondsRetryDelay;
		private readonly int _maxRetryCount;

		private IAsyncOperation _op;
		private int _numberOfRetriesLeft;

		#endregion

		#region interface

		protected RetryResult(object opFactory, int millisecondsRetryDelay, int maxRetryCount)
		{
			_opFactory = opFactory;
			_millisecondsRetryDelay = millisecondsRetryDelay;
			_maxRetryCount = maxRetryCount;
			_numberOfRetriesLeft = maxRetryCount;
		}

		protected void EndWait()
		{
			if (!IsCompleted)
			{
				if (IsCancellationRequested)
				{
					TrySetCanceled();
				}
				else
				{
					Retry();
				}
			}
		}

		protected abstract void BeginWait(int millisecondsDelay);

		#endregion

		#region AsyncResult

		protected override void OnStarted()
		{
			StartOperation();
		}

		protected override void OnCancel()
		{
			_op.Cancel();
		}

		#endregion

		#region IAsyncContinuation

		public override void Invoke(IAsyncOperation op)
		{
			Debug.Assert(_op == op);
			Debug.Assert(_op.IsCompleted);

			if (!IsCompleted)
			{
				if (_op.IsCompletedSuccessfully)
				{
					SetResult();
				}
				else if (IsCancellationRequested)
				{
					TrySetCanceled();
				}
				else if (_millisecondsRetryDelay > 0)
				{
					ReportProgress();
					BeginWait(_millisecondsRetryDelay);
				}
				else
				{
					Retry();
				}
			}
		}

		#endregion

		#region implementation

		private void StartOperation()
		{
			try
			{
				if (_opFactory is Func<IAsyncOperation> f1)
				{
					_op = f1();
				}
				else if (_opFactory is Func<IAsyncOperation<T>> f2)
				{
					_op = f2();
				}
				else
				{
					throw new InvalidOperationException(Messages.FormatError_InvalidDelegateType(_opFactory.GetType()));
				}

				_op.AddCompletionCallback(this);
			}
			catch (Exception e)
			{
				TrySetException(e);
			}
		}

		private void Retry()
		{
			Debug.Assert(_op != null);
			Debug.Assert(!_op.IsCompletedSuccessfully);

			if (_maxRetryCount == 0 || --_numberOfRetriesLeft > 0)
			{
				StartOperation();
			}
			else if (_op.IsFaulted)
			{
				TrySetException(_op.Exception);
			}
			else if (_op.IsCanceled)
			{
				TrySetCanceled();
			}
			else
			{
				// NOTE: Should not get here.
				TrySetException(new Exception(Messages.FormatError_MaxNumberOrRetriesReached(_maxRetryCount)));
			}
		}

		private void SetResult()
		{
			Debug.Assert(_op != null);
			Debug.Assert(_op.IsCompletedSuccessfully);

			if (_op is IAsyncOperation<T> rop)
			{
				TrySetResult(rop.Result);
			}
			else
			{
				TrySetCompleted();
			}
		}

		#endregion
	}
}
