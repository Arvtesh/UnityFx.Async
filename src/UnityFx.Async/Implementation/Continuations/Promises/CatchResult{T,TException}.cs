// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal sealed class CatchResult<T, TException> : AsyncResult<T>, IAsyncContinuation where TException : Exception
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly Action<TException> _errorCallback;

		#endregion

		#region interface

		public CatchResult(IAsyncOperation op, Action<TException> errorCallback)
			: base(AsyncOperationStatus.Running)
		{
			_op = op;
			_errorCallback = errorCallback;

			op.AddCompletionCallback(this);
		}

		#endregion

		#region AsyncResult

		protected override float GetProgress()
		{
			return _op.Progress;
		}

		protected override void OnCancel()
		{
			_op.Cancel();
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (op.IsCompletedSuccessfully)
			{
				TrySetCompleted(false);
			}
			else if (!(op.Exception is TException))
			{
				TrySetException(op.Exception, false);
			}
			else
			{
				try
				{
					_errorCallback.Invoke(op.Exception as TException);
					TrySetCompleted(false);
				}
				catch (Exception e)
				{
					TrySetException(e, false);
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
