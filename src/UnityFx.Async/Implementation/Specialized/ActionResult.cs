// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class ActionResult : AsyncResult
	{
		#region data

		private readonly object _action;

		#endregion

		#region interface

		public ActionResult(Action action)
			: base(AsyncOperationStatus.Scheduled)
		{
			_action = action;
		}

		public ActionResult(SendOrPostCallback action, object state)
			: base(AsyncOperationStatus.Scheduled, state)
		{
			_action = action;
		}

		#endregion

		#region AsyncResult

		protected override void OnStarted()
		{
			try
			{
				if (_action is Action a)
				{
					a();
				}
				else
				{
					((SendOrPostCallback)_action)(AsyncState);
				}

				TrySetCompleted();
			}
			catch (Exception e)
			{
				TrySetException(e);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
