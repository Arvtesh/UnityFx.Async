// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace UnityFx.Async.Benchmarks
{
	public class Config : ManualConfig
	{
		public Config()
		{
			Add(JitOptimizationsValidator.DontFailOnError);
			Add(DefaultConfig.Instance.GetLoggers().ToArray());
			Add(DefaultConfig.Instance.GetExporters().ToArray());
			Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
		}
	}
}
