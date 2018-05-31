// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal class AsyncCallbackCollection
	{
		#region data

		private struct CallbackData
		{
			public readonly object Callback;
			public readonly SynchronizationContext SyncContext;

			public CallbackData(object callback, SynchronizationContext syncContext)
			{
				Callback = callback;
				SyncContext = syncContext;
			}
		}

		private readonly IAsyncOperation _op;

		private List<CallbackData> _callbacks;

		private CallbackData _callback1;
		private CallbackData _callback2;
		private CallbackData _callback3;
		private CallbackData _callback4;

		#endregion

		#region interface

		public AsyncCallbackCollection(IAsyncOperation op)
		{
			_op = op;
		}

		public AsyncCallbackCollection(IAsyncOperation op, object callback, SynchronizationContext syncContext)
		{
			_op = op;
			_callback1 = new CallbackData(callback, syncContext);
		}

		public void Add(object callback, SynchronizationContext syncContext)
		{
			lock (this)
			{
				var newCallback = new CallbackData(callback, syncContext);

				if (_callback1.Callback == null)
				{
					_callback1 = newCallback;
					return;
				}

				if (_callback2.Callback == null)
				{
					_callback2 = newCallback;
					return;
				}

				if (_callback3.Callback == null)
				{
					_callback3 = newCallback;
					return;
				}

				if (_callback4.Callback == null)
				{
					_callback4 = newCallback;
					return;
				}

				if (_callbacks == null)
				{
					_callbacks = new List<CallbackData>() { newCallback };
				}
				else
				{
					_callbacks.Add(newCallback);
				}
			}
		}

		public void Remove(object callback)
		{
			lock (this)
			{
				if (_callback1.Callback == callback)
				{
					_callback1 = default(CallbackData);
					return;
				}

				if (_callback2.Callback == callback)
				{
					_callback2 = default(CallbackData);
					return;
				}

				if (_callback3.Callback == callback)
				{
					_callback3 = default(CallbackData);
					return;
				}

				if (_callback4.Callback == callback)
				{
					_callback4 = default(CallbackData);
					return;
				}

				if (_callbacks != null)
				{
					var count = _callbacks.Count;

					for (var i = 0; i < count; i++)
					{
						if (_callbacks[i].Callback == callback)
						{
							_callbacks.RemoveAt(i);
							return;
						}
					}
				}
			}
		}

		public void Invoke()
		{
			lock (this)
			{
				if (_callback1.Callback != null)
				{
					Invoke(_callback1);
				}

				if (_callback2.Callback != null)
				{
					Invoke(_callback2);
				}

				if (_callback3.Callback != null)
				{
					Invoke(_callback3);
				}

				if (_callback4.Callback != null)
				{
					Invoke(_callback4);
				}

				if (_callbacks != null)
				{
					foreach (var item in _callbacks)
					{
						Invoke(item);
					}
				}
			}
		}

		public void InvokeAsync()
		{
			lock (this)
			{
				if (_callback1.Callback != null)
				{
					InvokeAsync(_callback1);
				}

				if (_callback2.Callback != null)
				{
					InvokeAsync(_callback2);
				}

				if (_callback3.Callback != null)
				{
					InvokeAsync(_callback3);
				}

				if (_callback4.Callback != null)
				{
					InvokeAsync(_callback4);
				}

				if (_callbacks != null)
				{
					foreach (var item in _callbacks)
					{
						InvokeAsync(item);
					}
				}
			}
		}

		public void InvokeProgressChanged()
		{
			lock (this)
			{
				if (_callback1.Callback != null)
				{
					InvokeProgressChanged(_callback1);
				}

				if (_callback2.Callback != null)
				{
					InvokeProgressChanged(_callback2);
				}

				if (_callback3.Callback != null)
				{
					InvokeProgressChanged(_callback3);
				}

				if (_callback4.Callback != null)
				{
					InvokeProgressChanged(_callback4);
				}

				if (_callbacks != null)
				{
					foreach (var item in _callbacks)
					{
						InvokeProgressChanged(item);
					}
				}
			}
		}

		#endregion

		#region implementation

		private void Invoke(CallbackData callbackData)
		{
			var syncContext = callbackData.SyncContext;

			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeInline(callbackData.Callback);
			}
			else
			{
				syncContext.Post(InvokeInline, callbackData.Callback);
			}
		}

		private void InvokeAsync(CallbackData callbackData)
		{
			var syncContext = callbackData.SyncContext;

			if (syncContext != null)
			{
				syncContext.Post(InvokeInline, callbackData.Callback);
			}
			else
			{
				syncContext = SynchronizationContext.Current;

				if (syncContext != null)
				{
					syncContext.Post(InvokeInline, callbackData.Callback);
				}
				else
				{
					ThreadPool.QueueUserWorkItem(InvokeInline, callbackData.Callback);
				}
			}
		}

		private void InvokeProgressChanged(CallbackData callbackData)
		{
			var syncContext = callbackData.SyncContext;

			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeProgressChangedInline(callbackData.Callback);
			}
			else
			{
				syncContext.Post(InvokeProgressChangedInline, callbackData.Callback);
			}
		}

		private void InvokeInline(object callback)
		{
			Debug.Assert(callback != null);
			AsyncContinuation.InvokeInline(_op, callback, false);
		}

		private void InvokeProgressChangedInline(object callback)
		{
			Debug.Assert(callback != null);
			AsyncProgress.InvokeInline(_op, callback);
		}

		#endregion
	}
}
