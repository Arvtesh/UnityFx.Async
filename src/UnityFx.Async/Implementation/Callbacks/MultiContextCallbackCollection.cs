// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// An implementation of <see cref="IAsyncCallbackCollection"/>. The implementation assumes that in most cases
	/// there are 3 or less completion callbacks and 1 or less progress callbacks. A <see cref="SynchronizationContext"/>
	/// instance is stored for each callback.
	/// </summary>
	internal class MultiContextCallbackCollection : IAsyncCallbackCollection
	{
		#region data

		private readonly IAsyncOperation _op;

		private CallbackData _completionCallback1;
		private CallbackData _completionCallback2;
		private CallbackData _completionCallback3;
		private CallbackData _progressCallback1;

		private List<CallbackData> _progressCallbacks;
		private List<CallbackData> _completionCallbacks;

		#endregion

		#region interface

		public MultiContextCallbackCollection(IAsyncOperation op)
		{
			_op = op;
		}

		#endregion

		#region IAsyncCallbackCollection

		public void AddCompletionCallback(object callback, SynchronizationContext syncContext)
		{
			Debug.Assert(callback != null);

			var newCallback = new CallbackData(callback, syncContext);

			if (_completionCallback1.Callback == null)
			{
				_completionCallback1 = newCallback;
				return;
			}

			if (_completionCallback2.Callback == null)
			{
				_completionCallback2 = newCallback;
				return;
			}

			if (_completionCallback3.Callback == null)
			{
				_completionCallback3 = newCallback;
				return;
			}

			if (_completionCallbacks == null)
			{
				_completionCallbacks = new List<CallbackData>() { newCallback };
			}
			else
			{
				_completionCallbacks.Add(newCallback);
			}
		}

		public void AddProgressCallback(object callback, SynchronizationContext syncContext)
		{
			Debug.Assert(callback != null);

			var newCallback = new CallbackData(callback, syncContext);

			if (_progressCallback1.Callback == null)
			{
				_progressCallback1 = newCallback;
				return;
			}

			if (_progressCallbacks == null)
			{
				_progressCallbacks = new List<CallbackData>() { newCallback };
			}
			else
			{
				_progressCallbacks.Add(newCallback);
			}
		}

		public bool Remove(object callback)
		{
			if (_completionCallback1.Callback == callback)
			{
				_completionCallback1 = default(CallbackData);
				return true;
			}

			if (_completionCallback2.Callback == callback)
			{
				_completionCallback2 = default(CallbackData);
				return true;
			}

			if (_completionCallback3.Callback == callback)
			{
				_completionCallback3 = default(CallbackData);
				return true;
			}

			if (_progressCallback1.Callback == callback)
			{
				_progressCallback1 = default(CallbackData);
				return true;
			}

			if (_completionCallbacks != null)
			{
				var count = _completionCallbacks.Count;

				for (var i = 0; i < count; ++i)
				{
					if (_completionCallbacks[i].Callback == callback)
					{
						_completionCallbacks.RemoveAt(i);
						return true;
					}
				}
			}

			if (_progressCallbacks != null)
			{
				var count = _progressCallbacks.Count;

				for (var i = 0; i < count; ++i)
				{
					if (_progressCallbacks[i].Callback == callback)
					{
						_progressCallbacks.RemoveAt(i);
						return true;
					}
				}
			}

			return false;
		}

#if !DEBUG
		[DebuggerHidden]
#endif
		public void Invoke(bool invokeAsync)
		{
			if (_progressCallback1.Callback != null)
			{
				CallbackUtility.InvokeProgressCallback(_op, _progressCallback1.Callback, _progressCallback1.SyncContext);
			}

			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					CallbackUtility.InvokeProgressCallback(_op, item.Callback, item.SyncContext);
				}
			}

			if (_completionCallback1.Callback != null)
			{
				CallbackUtility.InvokeCompletionCallback(_op, _completionCallback1.Callback, _completionCallback1.SyncContext, invokeAsync);
			}

			if (_completionCallback2.Callback != null)
			{
				CallbackUtility.InvokeCompletionCallback(_op, _completionCallback2.Callback, _completionCallback2.SyncContext, invokeAsync);
			}

			if (_completionCallback3.Callback != null)
			{
				CallbackUtility.InvokeCompletionCallback(_op, _completionCallback3.Callback, _completionCallback3.SyncContext, invokeAsync);
			}

			if (_completionCallbacks != null)
			{
				foreach (var item in _completionCallbacks)
				{
					CallbackUtility.InvokeCompletionCallback(_op, item.Callback, item.SyncContext, invokeAsync);
				}
			}
		}

#if !DEBUG
		[DebuggerHidden]
#endif
		public void InvokeProgressCallbacks()
		{
			if (_progressCallback1.Callback != null)
			{
				CallbackUtility.InvokeProgressCallback(_op, _progressCallback1.Callback, _progressCallback1.SyncContext);
			}

			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					CallbackUtility.InvokeProgressCallback(_op, item.Callback, item.SyncContext);
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
