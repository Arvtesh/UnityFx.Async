// Copyright (c) 2018-2020 Alexander Bogarsukov.
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

		public UpdatableDelayResult(float secondsDelay, IAsyncUpdateSource updateSource)
		{
			_timeToWait = secondsDelay;
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
			TrySetCanceled();
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
				TrySetCompleted();
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
