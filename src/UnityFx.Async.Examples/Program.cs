// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Examples
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var examples = new Example[]
			{
				new Example01(),
				new Example02(),
				new Example03(),
			};

			foreach (var example in examples)
			{
				example.WriteHeader();
				example.Run();
			}
		}
	}
}
