// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
#if !NET35
using System.Collections.Concurrent;
#endif
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
	public class AsyncResultQueue<T> : IEnumerable<T>, IAsyncCancellable where T : AsyncResult
#else
	public class AsyncResultQueue<T> : IReadOnlyCollection<T>, IAsyncCancellable where T : AsyncResult
#endif
	{
		#region data

		private readonly SynchronizationContext _syncContext;

		private int _maxOpsSize = 0;
		private bool _suspended;
#if NET35
		private List<T> _ops = new List<T>();
#else
		private ConcurrentQueue<T> _ops = new ConcurrentQueue<T>();
#endif
		private SendOrPostCallback _startCallback;
		private Action<IAsyncOperation> _completionCallback;

		#endregion

		#region interface

		/// <summary>
		/// Gets a value indicating whether the queue is empty.
		/// </summary>
		/// <value>The empty flag.</value>
		/// <seealso cref="Count"/>
		public bool IsEmpty
		{
			get
			{
#if NET35
				return _ops.Count == 0;
#else
				return _ops.IsEmpty;
#endif
			}
		}

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
#if NET35
				lock (_ops)
				{
					return _ops.Count > 0 ? _ops[0] : null;
				}
#else
				return _ops.TryPeek(out var result) ? result : null;
#endif
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
		/// <exception cref="InvalidOperationException">Thrown if the operatinon is started.</exception>
		/// <seealso cref="Clear"/>
		public bool Add(T op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (op.IsStarted)
			{
				throw new InvalidOperationException();
			}

			if (_maxOpsSize > 0 && _ops.Count >= _maxOpsSize)
			{
				return false;
			}

			if (_completionCallback == null)
			{
				_completionCallback = OnCompletedCallback;
			}

			op.AddCompletionCallback(_completionCallback, _syncContext);

#if NET35
			lock (_ops)
			{
				_ops.Add(op);
				TryStart(op);
			}
#else
			_ops.Enqueue(op);
			TryStart(op);
#endif

			return true;
		}

		/// <summary>
		/// Removes all elements from the collection.
		/// </summary>
		public void Clear()
		{
#if NET35
			lock (_ops)
			{
				foreach (var op in _ops)
				{
					op.RemoveCompletionCallback(_completionCallback);
				}

				_ops.Clear();
			}
#else
			while (_ops.TryDequeue(out var op))
			{
				op.RemoveCompletionCallback(_completionCallback);
			}
#endif
		}

		/// <summary>
		/// Copies the collection elements to an array.
		/// </summary>
		public void CopyTo(T[] array, int arrayIndex)
		{
#if NET35
			lock (_ops)
			{
				_ops.CopyTo(array, arrayIndex);
			}
#else
			_ops.CopyTo(array, arrayIndex);
#endif
		}

		/// <summary>
		/// Returns the queue snapshot as array.
		/// </summary>
		/// <returns>An array containing the queue snapshot.</returns>
		public T[] ToArray()
		{
#if NET35
			lock (_ops)
			{
				return _ops.ToArray();
			}
#else
			return _ops.ToArray();
#endif
		}

		#endregion

		#region IAsyncCancellable

		/// <inheritdoc/>
		public void Cancel()
		{
#if NET35
			lock (_ops)
			{
				foreach (var op in _ops)
				{
					op.RemoveCompletionCallback(_completionCallback);
					op.Cancel();
				}

				_ops.Clear();
			}
#else
			while (_ops.TryDequeue(out var op))
			{
				op.RemoveCompletionCallback(_completionCallback);
				op.Cancel();
			}
#endif
		}

		#endregion

		#region IReadOnlyCollection

		/// <inheritdoc/>
		public int Count => _ops.Count;

		#endregion

		#region IEnumerable

#if NET35

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

#else

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return _ops.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _ops.GetEnumerator();
		}

#endif

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

#if NET35
				while (_ops.Count > 0)
				{
					var firstOp = _ops[0];

					if (firstOp.IsCompleted)
					{
						_ops.RemoveAt(0);
					}
					else
					{
						firstOp.TrySetRunning();
						break;
					}
				}
#else
				while (_ops.TryPeek(out var firstOp))
				{
					if (firstOp.IsCompleted)
					{
						_ops.TryDequeue(out firstOp);
					}
					else
					{
						firstOp.TrySetRunning();
						break;
					}
				}
#endif
			}
		}

		private void OnStartCallback(object args)
		{
#if NET35
			lock (_ops)
			{
				TryStartUnsafe(args as T);
			}
#else
			TryStartUnsafe(args as T);
#endif
		}

		private void OnCompletedCallback(IAsyncOperation op)
		{
#if NET35
			lock (_ops)
			{
				TryStartUnsafe(null);
			}
#else
			TryStartUnsafe(null);
#endif
		}

		#endregion
	}
}
