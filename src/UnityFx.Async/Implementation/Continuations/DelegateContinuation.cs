// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class DelegateContinuation : AsyncContinuation
	{
		#region data

		private readonly AsyncResult _op;
		private readonly object _continuation;

		#endregion

		#region interface

		internal DelegateContinuation(AsyncResult op, SynchronizationContext syncContext, object continuation)
			: base(op, syncContext, continuation)
		{
			_op = op;
			_continuation = continuation;
		}

		#endregion

		#region Object

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, _continuation))
			{
				return true;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _continuation.GetHashCode();
		}

		#endregion
	}
}
