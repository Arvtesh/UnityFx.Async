// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// An object that handles the low-level work of running asynchronous operations.
	/// </summary>
	public abstract class AsyncScheduler
	{
		#region data

		private static AsyncScheduler _defaultScheduler;

		#endregion

		#region interface

		/// <summary>
		/// Returns default <see cref="MonoBehaviour"/>-based scheduler instance. Read only.
		/// </summary>
		public static AsyncScheduler Default
		{
			get
			{
				if (_defaultScheduler == null)
				{
					_defaultScheduler = new DefaultAsyncScheduler(AsyncRunnerBehaviour.Instance);
				}

				return _defaultScheduler;
			}
		}

		/// <summary>
		/// Returns a new instance of <see cref="AsyncScheduler"/> for the specified <see cref="MonoBehaviour"/>.
		/// </summary>
		public static AsyncScheduler FromMonoBehaviour(MonoBehaviour b)
		{
			if (b == null)
			{
				throw new ArgumentNullException(nameof(b));
			}

			return new DefaultAsyncScheduler(b);
		}

		/// <summary>
		/// Executes the specified delegate on the main thread during Update method.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public static void ExecuteOnUpdate(Action op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			AsyncRunnerBehaviour.QueueActionForUpdate(op);
		}

		/// <summary>
		/// Executes the specified delegate on the main thread during LateUpdate method.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public static void ExecuteOnLateUpdate(Action op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			AsyncRunnerBehaviour.QueueActionForLateUpdate(op);
		}

		/// <summary>
		/// Queues the specified asynchronous operation for execution.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public void QueueAsync(IAsyncResult op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			QueueForExecution(op);
		}

		/// <summary>
		/// Queues the specified enumerator instance for execution.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public void QueueEnum(IEnumerator op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			QueueForExecution(op);
		}

		/// <summary>
		/// Queues the specified operation for execution.
		/// </summary>
		protected abstract void QueueForExecution(IAsyncResult op);

		/// <summary>
		/// Queues the specified operation for execution.
		/// </summary>
		protected abstract void QueueForExecution(IEnumerator op);

		#endregion

		#region implementation
		#endregion
	}
}
