// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Async
{
	internal class AsyncRunnerBehaviour : MonoBehaviour
	{
		#region data

		private static readonly Queue<Action> _updateExecutionQueue = new Queue<Action>();
		private static readonly Queue<Action> _lateUpdateExecutionQueue = new Queue<Action>();

		private readonly List<Action> _tmpList = new List<Action>();

		#endregion

		#region interface

		public static void QueueActionForUpdate(Action op)
		{
			lock (_updateExecutionQueue)
			{
				_updateExecutionQueue.Enqueue(op);
			}
		}

		public static void QueueActionForLateUpdate(Action op)
		{
			lock (_lateUpdateExecutionQueue)
			{
				_lateUpdateExecutionQueue.Enqueue(op);
			}
		}

		#endregion

		#region MonoBehaviour

		private void Update()
		{
			UpdateQueue(_updateExecutionQueue);
		}

		private void LateUpdate()
		{
			UpdateQueue(_lateUpdateExecutionQueue);
		}

		#endregion

		#region implementation

		private void UpdateQueue(Queue<Action> ops)
		{
			// check the queue size first to avoid locking each frame
			if (ops.Count > 0)
			{
				lock (ops)
				{
					_tmpList.AddRange(ops);
					ops.Clear();
				}

				foreach (var item in _tmpList)
				{
					try
					{
						item?.Invoke();
					}
					catch
					{
						// TODO
					}
				}

				_tmpList.Clear();
			}
		}

		#endregion
	}
}
