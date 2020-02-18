// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class UpdatableRetryResult<T> : RetryResult<T>, IAsyncUpdatable
	{
		#region data

		private readonly IAsyncUpdateSource _updateService;
		private float _timer;

		#endregion

		#region interface

		internal UpdatableRetryResult(object opFactory, int millisecondsRetryDelay, int maxRetryCount, IAsyncUpdateSource updateSource)
			: base(opFactory, millisecondsRetryDelay, maxRetryCount)
		{
			_updateService = updateSource;
		}

		protected override void BeginWait(int millisecondsDelay)
		{
			_timer = millisecondsDelay / 1000f;
			_updateService.AddListener(this);
		}

		#endregion

		#region AsyncResult

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
				_updateService.RemoveListener(this);
				EndWait();
			}
		}

		#endregion
	}
}
