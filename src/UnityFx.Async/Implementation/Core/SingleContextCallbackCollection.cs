// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A single-threaded implementation of <see cref="IAsyncCallbackCollection"/>. The implementation assumes that in most cases
	/// there are 3 or less completion callbacks and 1 or less progress callbacks. A shared <see cref="SynchronizationContext"/>
	/// instance is used for all callbacks.
	/// </summary>
	internal class SingleContextCallbackCollection : IAsyncCallbackCollection
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly SynchronizationContext _sharedContext;

		private object _completionCallback1;
		private object _completionCallback2;
		private object _completionCallback3;
		private object _progressCallback1;

		private List<object> _progressCallbacks;
		private List<object> _completionCallbacks;

		#endregion

		#region interface

		public SingleContextCallbackCollection(IAsyncOperation op, SynchronizationContext sharedContext)
		{
			_op = op;
			_sharedContext = sharedContext;
		}

		public SingleContextCallbackCollection(IAsyncOperation op, object callback, SynchronizationContext syncContext)
		{
			_op = op;
			_completionCallback1 = callback;
			_sharedContext = syncContext;
		}

		#endregion

		#region IAsyncCallbackCollection

		public void AddCompletionCallback(object newCallback, SynchronizationContext syncContext)
		{
			// NOTE: syncContext is ignored
			Debug.Assert(newCallback != null);

			if (_completionCallback1 == null)
			{
				_completionCallback1 = newCallback;
				return;
			}

			if (_completionCallback2 == null)
			{
				_completionCallback2 = newCallback;
				return;
			}

			if (_completionCallback3 == null)
			{
				_completionCallback3 = newCallback;
				return;
			}

			if (_completionCallbacks == null)
			{
				_completionCallbacks = new List<object>() { newCallback };
			}
			else
			{
				_completionCallbacks.Add(newCallback);
			}
		}

		public void AddProgressCallback(object callback, SynchronizationContext syncContext)
		{
			// NOTE: syncContext is ignored
			Debug.Assert(callback != null);

			if (_progressCallback1 == null)
			{
				_progressCallback1 = callback;
				return;
			}

			if (_progressCallbacks == null)
			{
				_progressCallbacks = new List<object>() { callback };
			}
			else
			{
				_progressCallbacks.Add(callback);
			}
		}

		public bool Remove(object callback)
		{
			if (_completionCallback1 == callback)
			{
				_completionCallback1 = null;
				return true;
			}

			if (_completionCallback2 == callback)
			{
				_completionCallback2 = null;
				return true;
			}

			if (_completionCallback3 == callback)
			{
				_completionCallback3 = null;
				return true;
			}

			if (_progressCallback1 == callback)
			{
				_progressCallback1 = null;
				return true;
			}

			if (_completionCallbacks != null)
			{
				if (_completionCallbacks.Remove(callback))
				{
					return true;
				}
			}

			if (_progressCallbacks != null)
			{
				if (_progressCallbacks.Remove(callback))
				{
					return true;
				}
			}

			return false;
		}

		public void Invoke(bool invokeAsync)
		{
			if (_progressCallback1 != null)
			{
				InvokeProgressCallback(_progressCallback1);
			}

			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					InvokeProgressCallback(item);
				}
			}

			if (_completionCallback1 != null)
			{
				Invoke(_completionCallback1, invokeAsync);
			}

			if (_completionCallback2 != null)
			{
				Invoke(_completionCallback2, invokeAsync);
			}

			if (_completionCallback3 != null)
			{
				Invoke(_completionCallback3, invokeAsync);
			}

			if (_completionCallbacks != null)
			{
				foreach (var item in _completionCallbacks)
				{
					Invoke(item, invokeAsync);
				}
			}
		}

		public void InvokeProgressCallbacks()
		{
			if (_progressCallback1 != null)
			{
				InvokeProgressCallback(_progressCallback1);
			}

			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					InvokeProgressCallback(item);
				}
			}
		}

		#endregion

		#region implementation

		private void Invoke(object callback, bool invokeAsync)
		{
			var syncContext = _sharedContext;

			if (invokeAsync)
			{
				if (syncContext != null)
				{
					syncContext.Post(InvokeInline, callback);
				}
				else
				{
					syncContext = SynchronizationContext.Current;

					if (syncContext != null)
					{
						syncContext.Post(InvokeInline, callback);
					}
					else
					{
						ThreadPool.QueueUserWorkItem(InvokeInline, callback);
					}
				}
			}
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeInline(callback);
			}
			else
			{
				syncContext.Post(InvokeInline, callback);
			}
		}

		private void InvokeProgressCallback(object callback)
		{
			var syncContext = _sharedContext;

			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeProgressChangedInline(callback);
			}
			else
			{
				syncContext.Post(InvokeProgressChangedInline, callback);
			}
		}

		private void InvokeInline(object callback)
		{
			Debug.Assert(callback != null);
			CallbackUtility.InvokeCompletionCallback(_op, callback);
		}

		private void InvokeProgressChangedInline(object callback)
		{
			Debug.Assert(callback != null);
			CallbackUtility.InvokeProgressCallback(_op, callback);
		}

		#endregion
	}
}
