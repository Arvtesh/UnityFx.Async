// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using RSG;
using UnityFx.Async.Promises;

namespace UnityFx.Async.Benchmarks
{
	using Promise = RSG.Promise;

	[MemoryDiagnoser]
	public class ThenBenchmarks
	{
		private readonly Action _action = () => { };

		[Params(1, 2, 4, 8)]
		public int N { get; set; }

		[Benchmark(Baseline = true)]
		public void Then_List()
		{
			var list = new List<Action>(N);

			for (var i = 0; i < N; i++)
			{
				list.Add(_action);
			}
		}

		[Benchmark]
		public void Then_Async()
		{
			IAsyncOperation op = new AsyncResult();

			for (var i = 0; i < N; i++)
			{
				op = op.Then(_action);
			}
		}

		[Benchmark]
		public void Then_Async_2()
		{
			IAsyncOperation op = new AsyncResult();

			for (var i = 0; i < N; i++)
			{
				op.Then(_action);
			}
		}

		[Benchmark]
		public void Then_RsgPromise()
		{
			IPromise op = new Promise();

			for (var i = 0; i < N; i++)
			{
				op = op.Then(_action);
			}
		}

		[Benchmark]
		public void Then_RsgPromise_2()
		{
			IPromise op = new Promise();

			for (var i = 0; i < N; i++)
			{
				op.Then(_action);
			}
		}
	}
}
