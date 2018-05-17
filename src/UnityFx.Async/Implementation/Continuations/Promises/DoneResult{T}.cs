// Copyright (c) Alexander Bogarsukov.
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

		public DoneResult(object successCallback, Action<Exception> errorCallback)
		{
			_successCallback = successCallback;
			_errorCallback = errorCallback;
		}

		public void Invoke(IAsyncOperation op, bool inline)
		{
			if (op.IsCompletedSuccessfully)
			{
				if (_successCallback is Action a)
				{
					a();
				}
				else if (_successCallback is Action<T> a1)
				{
					a1((op as IAsyncOperation<T>).Result);
				}
			}
			else
			{
				_errorCallback?.Invoke(op.Exception);
			}
		}
	}
}
