// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	internal sealed class DoneResult<T> : IAsyncContinuation
	{
		#region data

		private readonly object _successCallback;
		private readonly Action<Exception> _errorCallback;

		#endregion

		#region interface

		public DoneResult(object successCallback, Action<Exception> errorCallback)
		{
			_successCallback = successCallback;
			_errorCallback = errorCallback;
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			try
			{
				if (op.IsCompletedSuccessfully)
				{
					if (_successCallback is Action a)
					{
						a();
					}
					else if (_successCallback is Action<T> a1)
					{
						a1(((IAsyncOperation<T>)op).Result);
					}
				}
				else if (_errorCallback != null)
				{
					_errorCallback.Invoke(op.Exception);
				}
				else
				{
					Promise.PropagateUnhandledException(this, op.Exception);
				}
			}
			catch (Exception e)
			{
				Promise.PropagateUnhandledException(this, e);
			}
		}

		#endregion
	}
}
