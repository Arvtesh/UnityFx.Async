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
#if NET35
	public class AsyncResultQueue<T> : ICollection<T> where T : AsyncResult
#else
	public class AsyncResultQueue<T> : ICollection<T>, IReadOnlyCollection<T> where T : AsyncResult
#endif
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
		/// Raised when the queue becomes emtpy.
		/// </summary>
		/// <seealso cref="OnEmpty"/>
		/// <seealso cref="IsEmpty"/>
		public event EventHandler Empty;

		/// <summary>
		/// Gets a value indicating whether the queue is empty.
		/// </summary>
		/// <value>The empty flag.</value>
		/// <seealso cref="OnEmpty"/>
		/// <seealso cref="Empty"/>
		/// <seealso cref="Count"/>
		public bool IsEmpty => _ops.Count == 0;

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
					throw new ArgumentOutOfRangeException(nameof(MaxCount), value, Constants.ErrorValueIsLessThanZero);
				}

				_maxOpsSize = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the queue in on pause. When in suspended state queue does not change state of the operations.
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
		/// Gets an operation that is running currently or <see langword="null"/> if the queue is empty.
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

			if (op.TryAddCompletionCallback(_completionCallback, AsyncContinuationOptions.None, _syncContext))
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
		/// Removes all operations from the queue and return them.
		/// </summary>
		/// <seealso cref="Remove(T)"/>
		public T[] Release()
		{
			lock (_ops)
			{
				if (_ops.Count > 0)
				{
					var result = new T[_ops.Count];

					for (var i = 0; i < result.Length; ++i)
					{
						var op = _ops[i];
						op.RemoveCompletionCallback(_completionCallback);
						result[i] = op;
					}

					_ops.Clear();
					OnEmpty();
					return result;
				}

				return new T[0];
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

		/// <summary>
		/// Called when the operation should be started.
		/// </summary>
		/// <param name="op">The operation to start.</param>
		protected virtual void OnStart(T op)
		{
			op.TrySetRunning();
		}

		/// <summary>
		/// Called when the queue becomes empty.
		/// </summary>
		/// <seealso cref="Empty"/>
		/// <seealso cref="IsEmpty"/>
		protected virtual void OnEmpty()
		{
			Empty?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region ICollection

		/// <inheritdoc/>
		public int Count => _ops.Count;

		/// <inheritdoc/>
		public bool IsReadOnly => false;

		/// <inheritdoc/>
		public void Add(T op)
		{
			if (!TryAdd(op))
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public bool Contains(T item)
		{
			lock (_ops)
			{
				return _ops.Contains(item);
			}
		}

		/// <inheritdoc/>
		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_ops)
			{
				_ops.CopyTo(array, arrayIndex);
			}
		}

		/// <inheritdoc/>
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

		#endregion

		#region IEnumerable

		private class Enumerator : IEnumerator<T>
		{
			private List<T> _list;
			private List<T>.Enumerator _enumerator;

			public Enumerator(List<T> list)
			{
				Monitor.Enter(list);

				_list = list;
				_enumerator = list.GetEnumerator();
			}

			public T Current => _enumerator.Current;

			object IEnumerator.Current => _enumerator.Current;

			public bool MoveNext() => _enumerator.MoveNext();

			public void Reset() => throw new NotSupportedException();

			public void Dispose()
			{
				if (_list != null)
				{
					_enumerator.Dispose();
					Monitor.Exit(_list);
					_list = null;
				}
			}
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(_ops);
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(_ops);
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
					var firstOp = _ops[0];

					if (firstOp.IsCompleted)
					{
						_ops.RemoveAt(0);
					}
					else
					{
						OnStart(firstOp);
						break;
					}
				}

				if (_ops.Count == 0)
				{
					OnEmpty();
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
