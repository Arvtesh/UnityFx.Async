// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// A FIFO queue of <see cref="AsyncResult"/> instances that are executed sequentially. Completed operations are removed from the queue automatically.
	/// </summary>
	/// <threadsafety static="true" instance="true"/>
	/// <seealso cref="AsyncResult"/>
#if NET35
	public class AsyncResultQueue<T> : ICollection<T> where T : AsyncResult
#else
	public class AsyncResultQueue<T> : ICollection<T>, IReadOnlyCollection<T> where T : AsyncResult
#endif
	{
		#region data

		private int _maxOpsSize = 0;
		private List<T> _ops = new List<T>();

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets maximum queue size. Default is 0 (no constraints).
		/// </summary>
		/// <value>Maximum queue size. Zero means no constraints.</value>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="MaxCount"/> value is less than 0.</exception>
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
		/// Get an operation that is running currently or <see langword="null"/> if the queue is empty.
		/// </summary>
		/// <value>An operation that is running currently.</value>
		public T Current => _ops.Count > 0 ? _ops[0] : null;

		#endregion

		#region ICollection

		/// <inheritdoc/>
		public int Count => _ops.Count;

		/// <inheritdoc/>
		public bool IsReadOnly => false;

		/// <summary>
		/// Adds a new operation to the end of the queue. Operation should have its status set to <see cref="AsyncOperationStatus.Created"/>.
		/// </summary>
		/// <param name="op">The operation to enqueue.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if either queue size exceeds <see cref="MaxCount"/> value or the operation status is not <see cref="AsyncOperationStatus.Created"/>.</exception>
		/// <seealso cref="Remove(T)"/>
		public void Add(T op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (_maxOpsSize > 0 && _ops.Count >= _maxOpsSize)
			{
				throw new InvalidOperationException();
			}

			op.SetScheduled();
			op.Completed += OnOperationCompleted;

			lock (_ops)
			{
				_ops.Add(op);

				if (_ops.Count == 1)
				{
					op.SetRunning();
				}
			}
		}

		/// <summary>
		/// Removes the specified operation from the queue.
		/// </summary>
		/// <param name="op">The operation to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if the operation is removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="Add(T)"/>
		public bool Remove(T op)
		{
			lock (_ops)
			{
				return _ops.Remove(op);
			}
		}

		/// <inheritdoc/>
		public bool Contains(T op)
		{
			lock (_ops)
			{
				return _ops.Contains(op);
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
				_ops.Clear();
			}
		}

		#endregion

		#region IEnumerable

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

		#endregion

		#region implementation

		private void OnOperationCompleted(object sender, EventArgs args)
		{
			lock (_ops)
			{
				_ops.Remove(sender as T);

				if (_ops.Count > 0)
				{
					_ops[0].TrySetRunning();
				}
			}
		}

		#endregion
	}
}
