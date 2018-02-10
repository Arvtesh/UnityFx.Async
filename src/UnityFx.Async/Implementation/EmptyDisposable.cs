// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal sealed class EmptyDisposable : IDisposable
	{
		#region data

		private static IDisposable _instance;

		#endregion

		#region interface

		public static IDisposable Instance
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

		#region Disposable

		public void Dispose() { }

		#endregion
	}
}
