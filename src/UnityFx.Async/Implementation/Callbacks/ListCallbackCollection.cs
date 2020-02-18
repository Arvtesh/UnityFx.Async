// Copyright (c) 2018-2020 Alexander Bogarsukov.
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
	internal class ListCallbackCollection : List<CallbackData>, IAsyncCallbackCollection
	{
		#region data

		private readonly IAsyncOperation _op;
		private List<CallbackData> _progressCallbacks;

		#endregion

		#region interface

		public ListCallbackCollection(IAsyncOperation op)
		{
			_op = op;
		}

		#endregion

		#region IAsyncCallbackCollection

		public void AddCompletionCallback(object callback, SynchronizationContext syncContext)
		{
			Debug.Assert(callback != null);

			var newCallback = new CallbackData(callback, syncContext);
			Add(newCallback);
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
			{
				var count = Count;

				for (var i = 0; i < count; ++i)
				{
					if (this[i].Callback == callback)
					{
						RemoveAt(i);
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
			if (_progressCallbacks != null)
			{
				foreach (var item in _progressCallbacks)
				{
					CallbackUtility.InvokeProgressCallback(_op, item.Callback, item.SyncContext);
				}
			}

			foreach (var item in this)
			{
				CallbackUtility.InvokeCompletionCallback(_op, item.Callback, item.SyncContext, invokeAsync);
			}
		}

#if !DEBUG
		[DebuggerHidden]
#endif
		public void InvokeProgressCallbacks()
		{
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
