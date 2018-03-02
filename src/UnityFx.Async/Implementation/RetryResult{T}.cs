// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal class RetryResult<T> : AsyncResult<T>
	{
		#region data

		private readonly object _opFactory;
		private readonly AsyncOperationCallback _opCompletionCallback;
		private readonly int _millisecondsRetryDelay;
		private readonly int _maxRetryCount;

		private Timer _timer;
		private TimerCallback _timerCallback;
		private IAsyncOperation _op;
		private int _numberOfRetriesLeft;

		#endregion

		#region interface

		internal RetryResult(object opFactory, int millisecondsRetryDelay, int maxRetryCount)
			: base(AsyncOperationStatus.Running)
		{
			_opFactory = opFactory;
			_millisecondsRetryDelay = millisecondsRetryDelay;
			_maxRetryCount = maxRetryCount;
			_opCompletionCallback = OnOperationCompleted;
			_numberOfRetriesLeft = maxRetryCount;

			StartOperation(true);
		}

		#endregion

		#region AsyncResult

		protected override void OnCompleted()
		{
			base.OnCompleted();

			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}

		#endregion

		#region implementation

		private void StartOperation(bool calledFromConstructor)
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
					throw new InvalidOperationException("Invalid delegate type.");
				}

				if (!_op.TryAddCompletionCallback(_opCompletionCallback, null))
				{
					if (_op.IsCompletedSuccessfully)
					{
						SetResult(calledFromConstructor);
					}
					else
					{
						Retry(calledFromConstructor);
					}
				}
			}
			catch (Exception e)
			{
				TrySetException(e, calledFromConstructor);
			}
		}

		private void OnOperationCompleted(IAsyncOperation op)
		{
			Debug.Assert(_op == op);
			Debug.Assert(_op.IsCompleted);

			if (!IsCompleted)
			{
				if (_op.IsCompletedSuccessfully)
				{
					SetResult(false);
				}
				else if (_millisecondsRetryDelay > 0)
				{
					if (_timerCallback == null)
					{
						_timerCallback = OnTimer;
					}

					if (_timer == null)
					{
						_timer = new Timer(_timerCallback, null, _millisecondsRetryDelay, Timeout.Infinite);
					}
					else
					{
						_timer.Change(_millisecondsRetryDelay, Timeout.Infinite);
					}
				}
				else
				{
					Retry(false);
				}
			}
		}

		private void OnTimer(object args)
		{
			if (!IsCompleted)
			{
				Retry(false);
			}
		}

		private void Retry(bool calledFromConstructor)
		{
			Debug.Assert(_op != null);
			Debug.Assert(!_op.IsCompletedSuccessfully);

			if (_maxRetryCount == 0 || --_numberOfRetriesLeft > 0)
			{
				StartOperation(calledFromConstructor);
			}
			else if (_op.IsFaulted)
			{
				TrySetException(_op.Exception, calledFromConstructor);
			}
			else if (_op.IsCanceled)
			{
				TrySetCanceled(calledFromConstructor);
			}
			else
			{
				// NOTE: should not get here.
				TrySetException(new Exception("Maximum number of retries exceeded."), calledFromConstructor);
			}
		}

		private void SetResult(bool completedSynchronously)
		{
			Debug.Assert(_op != null);
			Debug.Assert(_op.IsCompletedSuccessfully);

			if (_op is IAsyncOperation<T> rop)
			{
				TrySetResult(rop.Result, completedSynchronously);
			}
			else
			{
				TrySetCompleted(completedSynchronously);
			}
		}

		#endregion
	}
}
