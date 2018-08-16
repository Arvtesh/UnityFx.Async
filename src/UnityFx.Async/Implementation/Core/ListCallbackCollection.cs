// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// An implementation of <see cref="IAsyncCallbackCollection"/> based on a <see cref="List{T}"/>. A <see cref="SynchronizationContext"/>
	/// instance is stored for each callback.
	/// </summary>
	internal class ListCallbackCollection : IAsyncCallbackCollection
	{
		#region data

		private readonly IAsyncOperation _op;

		private List<CallbackData> _progressCallbacks;
		private List<CallbackData> _completionCallbacks;

		#endregion

		#region interface

		public ListCallbackCollection(IAsyncOperation op)
		{
			_op = op;
		}

		public ListCallbackCollection(IAsyncOperation op, object callback, SynchronizationContext syncContext)
		{
			_op = op;
			_completionCallbacks = new List<CallbackData>() { new CallbackData(callback, syncContext) };
		}

		#endregion

		#region IAsyncCallbackCollection

		public void AddCompletionCallback(object callback, SynchronizationContext syncContext)
		{
			Debug.Assert(callback != null);

			var newCallback = new CallbackData(callback, syncContext);

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

		public void Invoke(bool invokeAsync)
		{
			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					InvokeProgressCallback(item);
				}
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

		private void Invoke(CallbackData callbackData, bool invokeAsync)
		{
			var syncContext = callbackData.SyncContext;

			if (invokeAsync)
			{
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
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeInline(callbackData.Callback);
			}
			else
			{
				syncContext.Post(InvokeInline, callbackData.Callback);
			}
		}

		private void InvokeProgressCallback(CallbackData callbackData)
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
