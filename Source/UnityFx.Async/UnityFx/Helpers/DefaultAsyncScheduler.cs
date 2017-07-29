// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	internal class DefaultAsyncScheduler : AsyncScheduler
	{
		#region data

		private readonly MonoBehaviour _owner;

		#endregion

		#region interface

		internal DefaultAsyncScheduler(MonoBehaviour owner)
		{
			Debug.Assert(owner != null);
			_owner = owner;
		}

		#endregion

		#region AsyncScheduler

		protected override void QueueForExecution(IAsyncResult op)
		{
			if (op is IEnumerator e)
			{
				QueueForExecution(e);
			}
			else
			{
				QueueForExecution(RunEnum(op));
			}
		}

		protected override void QueueForExecution(IEnumerator op)
		{
			_owner.StartCoroutine(op);
		}

		#endregion

		#region implementation

		private static IEnumerator RunEnum<T>(T op) where T : class
		{
			var asyncOp = op as IAsyncResult;

			// IAsyncResult is not supported by StartCoroutine so it cannot be yielded
			if (op == null || op is IEnumerator)
			{
				yield return op;
			}
			else
			{
				while (!asyncOp.IsCompleted)
				{
					yield return null;
				}
			}
		}

		#endregion
	}
}
