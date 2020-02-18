// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class ApmResult<TSource, TResult> : AsyncResult<TResult>
	{
		public TSource Source { get; }

		public ApmResult(TSource source)
			: base(AsyncOperationStatus.Running)
		{
			Source = source;
		}
	}
}
