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
		#endregion

		#region interface

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
		public static void QueueForUpdate(Action op)
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
		public static void QueueForLateUpdate(Action op)
		{
			if (op == null)
			{
				throw new ArgumentNullException(nameof(op));
			}

			AsyncRunnerBehaviour.QueueActionForLateUpdate(op);
		}

		/// <summary>
		/// Queues the specified action for execution.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="a"/> is <c>null</c>.</exception>
		public void QueueAction(Action a)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}

			AsyncRunnerBehaviour.QueueActionForUpdate(a);
		}

		/// <summary>
		/// Queues the specified enumerator instance for execution.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="op"/> is <c>null</c>.</exception>
		public void QueueCoroutine(IEnumerator op)
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
		protected abstract void QueueForExecution(IEnumerator op);

		#endregion
	}
}
