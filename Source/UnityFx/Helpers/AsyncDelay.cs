// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/> wrapper for <see cref="IEnumerator"/>.
	/// </summary>
	internal sealed class AsyncDelay : AsyncResult
	{
		#region data

		private readonly float _startTime = Time.time;
		private readonly float _delay;

		#endregion

		#region interface

		public AsyncDelay(TimeSpan delay)
		{
			_delay = (float)delay.TotalSeconds;
		}

#if NET46
		public AsyncDelay(TimeSpan delay, CancellationToken cancellationToken)
			: base(null, cancellationToken)
		{
			_delay = (float)delay.TotalSeconds;
		}
#endif

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			var timeElapsed = Time.time - _startTime;

			if (timeElapsed < _delay)
			{
				SetProgress(timeElapsed / _delay);
			}
			else
			{
				SetCompleted();
			}
		}

		#endregion
	}
}
