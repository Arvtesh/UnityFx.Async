// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Examples
{
	internal abstract class Example
	{
		public abstract string GetName();
		public abstract void Run();

		public void WriteHeader()
		{
			Console.WriteLine("==============================================================================");
			Console.WriteLine(GetType().Name + ": " + GetName() + ".");
			Console.WriteLine("==============================================================================");
		}

		public override string ToString()
		{
			return GetName();
		}
	}
}
