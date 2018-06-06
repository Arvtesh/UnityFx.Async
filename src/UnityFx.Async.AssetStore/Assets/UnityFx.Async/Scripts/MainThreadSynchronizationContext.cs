// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="SynchronizationContext"/> for Unity. The class is a helper for <see cref="MainThreadScheduler"/>;
	/// do not use unless absolutely nessesary.
	/// </summary>
	public class MainThreadSynchronizationContext : SynchronizationContext
	{
		#region data

		private readonly MainThreadScheduler _scheduler;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="MainThreadSynchronizationContext"/> class.
		/// </summary>
		public MainThreadSynchronizationContext(MainThreadScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		#endregion

		#region SynchronizationContext

		/// <inheritdoc/>
		public override SynchronizationContext CreateCopy()
		{
			return new MainThreadSynchronizationContext(_scheduler);
		}

		/// <inheritdoc/>
		public override void Send(SendOrPostCallback d, object state)
		{
			_scheduler.Send(d, state);
		}

		/// <inheritdoc/>
		public override void Post(SendOrPostCallback d, object state)
		{
			_scheduler.Post(d, state);
		}

		#endregion
	}
}
