// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A callback with <see cref="SynchronizationContext"/>.
	/// </summary>
	internal struct CallbackData
	{
		public readonly object Callback;
		public readonly SynchronizationContext SyncContext;

		public CallbackData(object callback, SynchronizationContext syncContext)
		{
			Callback = callback;
			SyncContext = syncContext;
		}
	}
}
