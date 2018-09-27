// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using BenchmarkDotNet.Attributes;

namespace UnityFx.Async.Benchmarks
{
	[MemoryDiagnoser]
	public class CallbackBenchmarks
	{
		private readonly Action<IAsyncOperation> _action = op => { };
		private readonly Action<float> _action2 = op => { };
		private readonly AsyncCompletedEventHandler _eventHandler = (sender, args) => { };
		private readonly ProgressChangedEventHandler _eventHandler2 = (sender, args) => { };

		[Params(1, 2, 4, 8)]
		public int N { get; set; }

		[Benchmark(Baseline = true)]
		public void Add_List()
		{
			var list = new List<Action<IAsyncOperation>>(N);

			for (var i = 0; i < N; i++)
			{
				list.Add(_action);
			}
		}

		[Benchmark]
		public void AddCompletionCallback()
		{
			var op = new AsyncResult();

			for (var i = 0; i < N; i++)
			{
				op.AddCompletionCallback(_action);
			}
		}

		[Benchmark]
		public void AddCompletionEvent()
		{
			var op = new AsyncResult();

			for (var i = 0; i < N; i++)
			{
				op.Completed += _eventHandler;
			}
		}

		[Benchmark]
		public void AddProgressCallback()
		{
			var op = new AsyncResult();

			for (var i = 0; i < N; i++)
			{
				op.AddProgressCallback(_action2);
			}
		}

		[Benchmark]
		public void AddProgressEvent()
		{
			var op = new AsyncResult();

			for (var i = 0; i < N; i++)
			{
				op.ProgressChanged += _eventHandler2;
			}
		}
	}
}
