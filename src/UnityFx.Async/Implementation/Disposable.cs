// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal sealed class Disposable
	{
		#region data

		private class EmptyDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}

		private static IDisposable _instance;

		#endregion

		#region interface

		public static IDisposable Empty
		{
			get
			{
				if (_instance == null)
				{
					_instance = new EmptyDisposable();
				}

				return _instance;
			}
		}

		#endregion
	}
}
