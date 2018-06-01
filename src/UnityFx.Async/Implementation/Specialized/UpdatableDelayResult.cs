// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal sealed class UpdatableDelayResult : AsyncResult, IAsyncUpdatable
	{
		#region data

		private const float _progressEventTimeout = 0.1f;

		private readonly IAsyncUpdateSource _updateService;
		private readonly float _timeToWait;

		private float _progressEventTimer;
		private float _timer;

		#endregion

		#region interface

		public UpdatableDelayResult(int millisecondsDelay, IAsyncUpdateSource updateSource)
		{
			_timeToWait = millisecondsDelay / 1000f;
			_timer = _timeToWait;
			_updateService = updateSource;
			_updateService.AddListener(this);
		}

		#endregion

		#region AsyncResult

		protected override float GetProgress()
		{
			return (_timeToWait - _timer) / _timeToWait;
		}

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
			else
			{
				_progressEventTimer += frameTime;

				if (_progressEventTimer >= _progressEventTimeout)
				{
					_progressEventTimer = 0;
					ReportProgress();
				}
			}
		}

		#endregion
	}
}
