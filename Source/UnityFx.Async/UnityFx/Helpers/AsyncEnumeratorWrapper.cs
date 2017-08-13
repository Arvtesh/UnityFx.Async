// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/> wrapper for <see cref="IEnumerator"/>.
	/// </summary>
	internal sealed class AsyncEnumeratorWrapper : AsyncResult
	{
		#region data

		private readonly IEnumerator _op;

		#endregion

		#region interface

		public AsyncEnumeratorWrapper(IEnumerator op)
			: base(op)
		{
			_op = op;
		}

#if UNITYFX_NET46
		public AsyncEnumeratorWrapper(IEnumerator op, CancellationToken cancellationToken)
			: base(op, cancellationToken)
		{
			_op = op;
		}
#endif

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			if (_op.MoveNext())
			{
				SetCurrent(_op.Current);
			}
			else
			{
				SetCompleted();
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
