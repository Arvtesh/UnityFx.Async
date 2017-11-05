// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation"/> that executes an update callback on each frame.
	/// </summary>
	internal sealed class AsyncUpdateCallback<T> : AsyncResult<T>
	{
		#region data

		private readonly Action<IAsyncOperationController<T>> _updater;

		#endregion

		#region interface

		public AsyncUpdateCallback(Action<IAsyncOperationController<T>> updater)
			: base(null)
		{
			_updater = updater;
		}

#if NET46
		public AsyncUpdateCallback(Action<IAsyncOperationController<T>> updater, CancellationToken cancellationToken)
			: base(null, cancellationToken)
		{
			_updater = updater;
		}
#endif

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			_updater(this);
		}

		#endregion

		#region implementation
		#endregion
	}
}
