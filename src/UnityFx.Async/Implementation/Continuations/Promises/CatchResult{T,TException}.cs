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

			op.AddContinuation(this);
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			if (_op is IAsyncCancellable c)
			{
				c.Cancel();
			}
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool inline)
		{
			if (op.IsCompletedSuccessfully)
			{
				TrySetCompleted(inline);
			}
			else if (!(op.Exception.InnerException is TException))
			{
				TrySetException(op.Exception, inline);
			}
			else
			{
				try
				{
					_errorCallback.Invoke(op.Exception.InnerException as TException);
					TrySetCompleted(inline);
				}
				catch (Exception e)
				{
					TrySetException(e, inline);
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
