// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal sealed class CatchResult<T, TException> : AsyncResult<T> where TException : Exception
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

		public override void Invoke(IAsyncOperation op)
		{
			if (op.IsCompletedSuccessfully)
			{
				TrySetCompleted();
			}
			else if (!(op.Exception is TException))
			{
				TrySetException(op.Exception);
			}
			else
			{
				try
				{
					_errorCallback.Invoke(op.Exception as TException);
					TrySetCompleted();
				}
				catch (Exception e)
				{
					TrySetException(e);
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
