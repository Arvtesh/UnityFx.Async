// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A FIFO queue of <see cref="AsyncResult"/> instances that are executed sequentially.
	/// </summary>
	/// <remarks>
	/// Completed operations are removed from the queue automatically. All interaction with queued operations is done
	/// through the <see cref="SynchronizationContext"/> that can be specified on queue construction.
	/// </remarks>
	/// <threadsafety static="true" instance="true"/>
	/// <seealso cref="AsyncResult"/>
	public class AsyncResultQueue<T> where T : AsyncResult
	{
		#region data

		private readonly SynchronizationContext _syncContext;

		private int _maxOpsSize = 0;
		private bool _suspended;
		private List<T> _ops = new List<T>();
		private SendOrPostCallback _startCallback;
		private AsyncOperationCallback _completionCallback;

		#endregion

		#region interface

		/// <summary>
		/// Gets the number of operations contained in the queue.
		/// </summary>
		/// <seealso cref="MaxCount"/>
		public int Count => _ops.Count;

		/// <summary>
		/// Gets or sets maximum queue size. Default is 0 (no constraints).
		/// </summary>
		/// <value>Maximum queue size. Zero means no constraints.</value>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="MaxCount"/> value is less than 0.</exception>
		/// <seealso cref="Count"/>
		public int MaxCount
		{
			get
			{
				return _maxOpsSize;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(MaxCount), value, "MaxCount value should be >= 0");
				}

				_maxOpsSize = value;
			}
		}

		/// <summary>
		/// Gets or sets whether the queue in on pause. When in suspended state queue does not change state of the operations.
		/// </summary>
		/// <value>The paused flag.</value>
		public bool Suspended
		{
			get
			{
				return _suspended;
			}
			set
			{
				_suspended = value;

				if (!_suspended)
				{
					TryStart(null);
				}
			}
		}

		/// <summary>
		/// Get an operation that is running currently or <see langword="null"/> if the queue is empty.
		/// </summary>
		/// <value>An operation that is running currently.</value>
		public T Current
		{
			get
			{
				lock (_ops)
				{
					return _ops.Count > 0 ? _ops[0] : null;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultQueue{T}"/> class.
		/// </summary>
		public AsyncResultQueue()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResultQueue{T}"/> class.
		/// </summary>
		/// <param name="syncContext">The synchronization context to to use for marshaling operation calls to specific thread. Can have <see langword="null"/> value.</param>
		public AsyncResultQueue(SynchronizationContext syncContext)
		{
			_syncContext = syncContext;
		}

		/// <summary>
		/// Adds a new operation to the end of the queue. Operation is expected to have its status set to <see cref="AsyncOperationStatus.Created"/>.
		/// </summary>
		/// <param name="op">The operation to enqueue.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is <see langword="null"/>.</exception>
		/// <seealso cref="Add(T)"/>
		/// <seealso cref="Remove(T)"/>
		/// <seealso cref="Clear"/>
		public bool TryAdd(T op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (_maxOpsSize > 0 && _ops.Count >= _maxOpsSize)
			{
				return false;
			}

			if (_completionCallback == null)
			{
				_completionCallback = OnCompletedCallback;
			}

			if (op.TryAddCompletionCallback(_completionCallback, _syncContext))
			{
				lock (_ops)
				{
					_ops.Add(op);
					TryStart(op);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds a new operation to the end of the queue. Operation is expected to have its status set to <see cref="AsyncOperationStatus.Created"/>.
		/// </summary>
		/// <param name="op">The operation to enqueue.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the operation cannot be added.</exception>
		/// <seealso cref="Remove(T)"/>
		/// <seealso cref="Clear"/>
		public void Add(T op)
		{
			if (!TryAdd(op))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Removes the specified operation from the queue.
		/// </summary>
		/// <param name="op">The operation to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if the operation is removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TryAdd(T)"/>
		/// <seealso cref="Clear"/>
		public bool Remove(T op)
		{
			lock (_ops)
			{
				if (_ops.Remove(op))
				{
					op.RemoveCompletionCallback(_completionCallback);
					TryStart(null);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes all operations from the queue.
		/// </summary>
		/// <seealso cref="Remove(T)"/>
		public void Clear()
		{
			lock (_ops)
			{
				foreach (var op in _ops)
				{
					op.RemoveCompletionCallback(_completionCallback);
				}

				_ops.Clear();
			}
		}

		/// <summary>
		/// Removes all operations from the queue and return them.
		/// </summary>
		/// <seealso cref="Remove(T)"/>
		public T[] Release()
		{
			lock (_ops)
			{
				var result = new T[_ops.Count];

				for (var i = 0; i < result.Length; ++i)
				{
					var op = _ops[i];
					op.RemoveCompletionCallback(_completionCallback);
					result[i] = op;
				}

				_ops.Clear();
				return result;
			}
		}

		/// <summary>
		/// Returns the queue snapshot as array.
		/// </summary>
		/// <returns>An array containing the queue snapshot.</returns>
		public T[] ToArray()
		{
			lock (_ops)
			{
				return _ops.ToArray();
			}
		}

		#endregion

		#region implementation

		private void TryStart(T op)
		{
			if (!_suspended)
			{
				if (_syncContext == null || _syncContext == SynchronizationContext.Current)
				{
					TryStartUnsafe(op);
				}
				else
				{
					if (_startCallback == null)
					{
						_startCallback = OnStartCallback;
					}

					_syncContext.Post(_startCallback, op);
				}
			}
		}

		private void TryStartUnsafe(T op)
		{
			if (!_suspended)
			{
				op?.TrySetScheduled();

				while (_ops.Count > 0)
				{
					if (!_ops[0].TrySetRunning())
					{
						_ops.RemoveAt(0);
					}
				}
			}
		}

		private void OnStartCallback(object args)
		{
			lock (_ops)
			{
				TryStartUnsafe(args as T);
			}
		}

		private void OnCompletedCallback(IAsyncOperation op)
		{
			lock (_ops)
			{
				if (_ops.Remove(op as T))
				{
					TryStartUnsafe(null);
				}
			}
		}

		#endregion
	}
}
