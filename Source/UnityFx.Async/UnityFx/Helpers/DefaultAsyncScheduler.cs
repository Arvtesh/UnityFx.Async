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

		protected override void QueueForExecution(IEnumerator op)
		{
			_owner.StartCoroutine(op);
		}

		#endregion
	}
}
