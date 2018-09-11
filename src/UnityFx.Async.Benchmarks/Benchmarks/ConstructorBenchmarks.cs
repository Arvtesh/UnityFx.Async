// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace UnityFx.Async.Benchmarks
{
	[MemoryDiagnoser]
	public class ConstructorBenchmarks
	{
		private readonly AsyncCallback _asyncCallback = op => { };
		private readonly Action _action = () => { };

		[Benchmark]
		public object CreateAsync()
		{
			return new AsyncResult(_asyncCallback, null);
		}

		[Benchmark]
		public object CreateAsyncCompletionSource()
		{
			return new AsyncCompletionSource<object>();
		}

		[Benchmark]
		public object CreateTask()
		{
			return new Task(_action);
		}

		[Benchmark]
		public object CreateTaskCompletionSource()
		{
			return new TaskCompletionSource<object>();
		}
	}
}
