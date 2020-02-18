// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal sealed class ActionResult<T> : AsyncResult<T>
	{
		#region data

		private readonly Func<T> _action;

		#endregion

		#region interface

		public ActionResult(Func<T> action)
			: base(AsyncOperationStatus.Scheduled)
		{
			_action = action;
		}

		#endregion

		#region AsyncResult

		protected override void OnStarted()
		{
			try
			{
				TrySetResult(_action());
			}
			catch (Exception e)
			{
				TrySetException(e);
			}
		}

		#endregion
	}
}
