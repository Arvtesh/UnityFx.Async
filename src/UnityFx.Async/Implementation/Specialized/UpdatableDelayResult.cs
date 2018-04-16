// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class UpdatableDelayResult : AsyncResult, IAsyncUpdatable
	{
		#region data

		private readonly IAsyncUpdateSource _updateService;
		private float _timer;

		#endregion

		#region interface

		public UpdatableDelayResult(int millisecondsDelay, IAsyncUpdateSource updateSource)
			: base(AsyncOperationStatus.Running)
		{
			_timer = millisecondsDelay;
			_updateService = updateSource;
			_updateService.AddListener(this);
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			TrySetCanceled(false);
		}

		protected override void OnCompleted()
		{
			_updateService.RemoveListener(this);
			base.OnCompleted();
		}

		#endregion

		#region IAsyncUpdatable

		public void Update(float frameTime)
		{
			_timer -= frameTime;

			if (_timer <= 0)
			{
				TrySetCompleted(false);
			}
		}

		#endregion
	}
}
