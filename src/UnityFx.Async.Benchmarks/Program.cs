﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using BenchmarkDotNet.Running;

namespace UnityFx.Async.Benchmarks
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//BenchmarkRunner.Run<ConstructorBenchmarks>();
			BenchmarkRunner.Run<ContinueWithBenchmarks>();
		}
	}
}
