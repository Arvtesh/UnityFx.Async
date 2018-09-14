// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using BenchmarkDotNet.Attributes;
using RSG;

namespace UnityFx.Async.Benchmarks
{
	[MemoryDiagnoser]
	public class ConstructorBenchmarks
	{
		private readonly AsyncCallback _asyncCallback = op => { };
		private readonly Action _action = () => { };

		[Benchmark(Baseline = true)]
		public object Create_object()
		{
			return new object();
		}

		[Benchmark]
		public object Create_Async()
		{
			return new AsyncResult();
		}

		[Benchmark]
		public object Create_RsgPromise()
		{
			return new Promise();
		}
	}
}
