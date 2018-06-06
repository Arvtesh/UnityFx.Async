// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Main thread scheduler that can be used to make sure the code is executed on the main thread.
	/// The implementation setups a <see cref="SynchronizationContext"/> (if there is none) and
	/// attaches it to the main thread.
	/// </summary>
	/// <seealso cref="UnitySynchronizationContext"/>
	public sealed class MainThreadScheduler : MonoBehaviour
	{
		#region data

		private sealed class InvokeResult : AsyncResult
		{
			private readonly SendOrPostCallback _callback;

			public InvokeResult(SendOrPostCallback d, object asyncState)
				: base(null, asyncState)
			{
				_callback = d;
			}

			public void SetCompleted()
			{
				_callback.Invoke(AsyncState);
			}

			public void SetException(Exception e)
			{
				TrySetException(e);
			}
		}

		private SynchronizationContext _context;
		private int _mainThreadId;
		private Queue<InvokeResult> _actionQueue = new Queue<InvokeResult>();

		#endregion

		#region interface

		/// <summary>
		/// Dispatches a synchronous message to the main thread.
		/// </summary>
		/// <param name="d">The delegate to execute.</param>
		/// <param name="state">The object passed to the delegate.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
		/// <seealso cref="Post(SendOrPostCallback, object)"/>
		public void Send(SendOrPostCallback d, object state)
		{
			if (d == null)
			{
				throw new ArgumentNullException("d");
			}

			if (!this)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				d.Invoke(state);
			}
			else
			{
				var asyncResult = new InvokeResult(d, state);

				lock (_actionQueue)
				{
					_actionQueue.Enqueue(asyncResult);
				}

				asyncResult.Wait();
			}
		}

		/// <summary>
		/// Dispatches an asynchronous message to the main thread.
		/// </summary>
		/// <param name="d">The delegate to execute.</param>
		/// <param name="state">The object passed to the delegate.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the object is disposed.</exception>
		/// <seealso cref="Send(SendOrPostCallback, object)"/>
		public void Post(SendOrPostCallback d, object state)
		{
			if (d == null)
			{
				throw new ArgumentNullException("d");
			}

			if (!this)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			var asyncResult = new InvokeResult(d, state);

			lock (_actionQueue)
			{
				_actionQueue.Enqueue(asyncResult);
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			var currentContext = SynchronizationContext.Current;

			if (currentContext == null)
			{
				var context = new MainThreadSynchronizationContext(this);
				SynchronizationContext.SetSynchronizationContext(context);
				_context = context;
			}

			_mainThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		private void OnDestroy()
		{
			if (_context != null && _context == SynchronizationContext.Current)
			{
				SynchronizationContext.SetSynchronizationContext(null);
			}

			lock (_actionQueue)
			{
				_actionQueue.Clear();
			}

			_context = null;
		}

		private void Update()
		{
			if (_actionQueue.Count > 0)
			{
				lock (_actionQueue)
				{
					while (_actionQueue.Count > 0)
					{
						var asyncResult = _actionQueue.Dequeue();

						if (asyncResult != null)
						{
							try
							{
								asyncResult.SetCompleted();
							}
							catch (Exception e)
							{
								asyncResult.SetException(e);
							}
						}
					}
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
