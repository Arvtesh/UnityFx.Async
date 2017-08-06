// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	internal class AsyncRunnerBehaviour : MonoBehaviour
	{
		#region data

		private static Queue<Action> _updateExecutionQueue;
		private static Queue<Action> _lateUpdateExecutionQueue;
		private static AsyncRunnerBehaviour _instance;

		private List<Action> _tmpList;

		#endregion

		#region interface

		public static AsyncRunnerBehaviour Instance
		{
			get
			{
				const string goName = "UnityFx";

				if (ReferenceEquals(_instance, null))
				{
					var go = GameObject.Find(goName);

					if (ReferenceEquals(go, null))
					{
						go = new GameObject(goName, typeof(AsyncRunnerBehaviour));
						DontDestroyOnLoad(go);
					}

					var c = go.GetComponent<AsyncRunnerBehaviour>();

					if (ReferenceEquals(c, null))
					{
						c = go.AddComponent<AsyncRunnerBehaviour>();
					}

					_instance = c;
				}

				return _instance;
			}
		}

		public static void QueueActionForUpdate(Action op)
		{
			if (_updateExecutionQueue == null)
			{
				Interlocked.CompareExchange(ref _updateExecutionQueue, new Queue<Action>(), null);
			}

			lock (_updateExecutionQueue)
			{
				_updateExecutionQueue.Enqueue(op);
			}
		}

		public static void QueueActionForLateUpdate(Action op)
		{
			if (_lateUpdateExecutionQueue == null)
			{
				Interlocked.CompareExchange(ref _lateUpdateExecutionQueue, new Queue<Action>(), null);
			}

			lock (_lateUpdateExecutionQueue)
			{
				_lateUpdateExecutionQueue.Enqueue(op);
			}
		}

		#endregion

		#region MonoBehaviour

		private void Update()
		{
			if (_updateExecutionQueue != null)
			{
				UpdateQueue(_updateExecutionQueue);
			}
		}

		private void LateUpdate()
		{
			if (_lateUpdateExecutionQueue != null)
			{
				UpdateQueue(_lateUpdateExecutionQueue);
			}
		}

		#endregion

		#region implementation

		private void UpdateQueue(Queue<Action> ops)
		{
			// check the queue size first to avoid locking each frame
			if (ops.Count > 0)
			{
				if (_tmpList == null)
				{
					_tmpList = new List<Action>();
				}

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
