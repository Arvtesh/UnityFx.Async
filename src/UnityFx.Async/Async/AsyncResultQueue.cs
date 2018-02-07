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
	public class AsyncResultQueue : ICollection<AsyncResult>
#else
	public class AsyncResultQueue : ICollection<AsyncResult>, IReadOnlyCollection<AsyncResult>
#endif
	{
		#region data

		private int _maxOpsSize = 256;
		private List<AsyncResult> _ops = new List<AsyncResult>();

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets maximum queue size.
		/// </summary>
		/// <value>Maximum queue size.</value>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="MaxSize"/> value is 0 or less.</exception>
		public int MaxSize
		{
			get
			{
				return _maxOpsSize;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(MaxSize), value, "MaxSize value should be > 0");
				}

				_maxOpsSize = value;
			}
		}

		/// <summary>
		/// Get an operation that is running currently or <see langword="null"/> if the queue is empty.
		/// </summary>
		/// <value>An operation that is running currently.</value>
		public AsyncResult Current => _ops.Count > 0 ? _ops[0] : null;

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
		/// <exception cref="InvalidOperationException">Thrown if either queue size exceeds <see cref="MaxSize"/> value or the operation status is not <see cref="AsyncOperationStatus.Created"/>.</exception>
		/// <seealso cref="Remove(AsyncResult)"/>
		public void Add(AsyncResult op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			if (_ops.Count >= _maxOpsSize)
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
		/// <seealso cref="Add(AsyncResult)"/>
		public bool Remove(AsyncResult op)
		{
			lock (_ops)
			{
				return _ops.Remove(op);
			}
		}

		/// <inheritdoc/>
		public bool Contains(AsyncResult op)
		{
			lock (_ops)
			{
				return _ops.Contains(op);
			}
		}

		/// <inheritdoc/>
		public void CopyTo(AsyncResult[] array, int arrayIndex)
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
		public IEnumerator<AsyncResult> GetEnumerator()
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
				_ops.Remove(sender as AsyncResult);

				if (_ops.Count > 0)
				{
					_ops[0].TrySetRunning();
				}
			}
		}

		#endregion
	}
}
