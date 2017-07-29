// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/> wrapper of <see cref="AsyncResult"/>.
	/// </summary>
	internal class AsyncOperationWrapper<T> : AsyncResult<T> where T : class
	{
		#region data

		private readonly AsyncOperation _op;

		#endregion

		#region interface

		public AsyncOperationWrapper(AsyncOperation op)
			: base(op)
		{
			_op = op;
		}

		protected override void OnUpdate()
		{
			if (_op.isDone)
			{
				SetResult(GetOperationResult(_op) as T);
			}
			else
			{
				SetProgress(_op.progress);
			}
		}

		#endregion
	}
}
