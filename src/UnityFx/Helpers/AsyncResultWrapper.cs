// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/> wrapper for <see cref="IAsyncResult"/>.
	/// </summary>
	internal sealed class AsyncResultWrapper : AsyncResult
	{
		#region data

		private readonly IAsyncResult _op;

		#endregion

		#region interface

		public AsyncResultWrapper(IAsyncResult op)
			: base(op)
		{
			_op = op;
		}

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			if (_op.IsCompleted)
			{
				SetCompleted();
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
