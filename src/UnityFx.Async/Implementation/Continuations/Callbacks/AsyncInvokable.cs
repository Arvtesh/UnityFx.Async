// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal abstract class AsyncInvokable
	{
		#region data

		private static WaitCallback _waitCallback;
		private static SendOrPostCallback _postCallback;

		private IAsyncOperation _op;
		private SynchronizationContext _syncContext;
		private object _callback;

		#endregion

		#region interface

		internal AsyncInvokable(IAsyncOperation op, SynchronizationContext syncContext, object callback)
		{
			_op = op;
			_syncContext = syncContext;
			_callback = callback;
		}

		internal void Invoke()
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				Invoke(_op, _callback);
			}
			else
			{
				InvokeOnSyncContext(_syncContext);
			}
		}

		internal void InvokeAsync()
		{
			var syncContext = _syncContext;

			if (syncContext != null)
			{
				InvokeOnSyncContext(syncContext);
			}
			else
			{
				syncContext = SynchronizationContext.Current;

				if (syncContext != null)
				{
					InvokeOnSyncContext(syncContext);
				}
				else
				{
					if (_waitCallback == null)
					{
						_waitCallback = PostCallback;
					}

					ThreadPool.QueueUserWorkItem(_waitCallback, this);
				}
			}
		}

		protected abstract void Invoke(IAsyncOperation op, object callback);

		#endregion

		#region Object

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, _callback))
			{
				return true;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _callback.GetHashCode();
		}

		#endregion

		#region implementation

		private void InvokeOnSyncContext(SynchronizationContext syncContext)
		{
			Debug.Assert(_syncContext != null);

			if (_postCallback == null)
			{
				_postCallback = PostCallback;
			}

			syncContext.Post(_postCallback, this);
		}

		private static void PostCallback(object args)
		{
			var c = args as AsyncInvokable;
			c.Invoke(c._op, c._callback);
		}

		#endregion
	}
}
