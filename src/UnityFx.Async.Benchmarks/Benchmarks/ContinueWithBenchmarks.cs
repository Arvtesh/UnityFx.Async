// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace UnityFx.Async.Benchmarks
{
	[MemoryDiagnoser]
	public class ContinueWithBenchmarks
	{
		private readonly Action<IAsyncOperation<object>> _action1 = op => { };
		private readonly Action<Task<object>> _action2 = task => { };

		[Params(1, 2, 4, 8, 16)]
		public int N { get; set; }

		[Benchmark]
		public void Async_ContinueWith()
		{
			var acp = new AsyncCompletionSource<object>();

			for (int i = 0; i < N; i++)
			{
				acp.Operation.ContinueWith(_action1);
			}
		}

		[Benchmark]
		public void Task_ContinueWith()
		{
			var tcs = new TaskCompletionSource<object>();

			for (int i = 0; i < N; i++)
			{
				tcs.Task.ContinueWith(_action2);
			}
		}
	}
}
