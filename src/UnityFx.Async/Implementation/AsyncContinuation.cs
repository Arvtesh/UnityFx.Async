// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class AsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly AsyncResult _op;
		private readonly SynchronizationContext _syncContext;
		private readonly object _continuation;

		#endregion

		#region interface

		public AsyncContinuation(AsyncResult op, SynchronizationContext syncContext, object continuation)
		{
			_op = op;
			_syncContext = syncContext;
			_continuation = continuation;
		}

		public void Invoke()
		{
			if (_syncContext == SynchronizationContext.Current)
			{
				Run(_op, _continuation);
			}
			else
			{
				if (_postCallback == null)
				{
					_postCallback = args => (args as AsyncContinuation).Run();
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		public void Run()
		{
			Run(_op, _continuation);
		}

		public static void Run(IAsyncOperation op, object continuation)
		{
			if (continuation is Action a)
			{
				a.Invoke();
			}
			else if (continuation is AsyncCallback ac)
			{
				ac.Invoke(op);
			}
			else if (continuation is EventHandler eh)
			{
				eh.Invoke(op, EventArgs.Empty);
			}
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

		#region implementation
		#endregion
	}
}
