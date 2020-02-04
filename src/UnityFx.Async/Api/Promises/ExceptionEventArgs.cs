// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	/// <summary>
	/// Arguments of <see cref="Promise.UnhandledException"/> event.
	/// </summary>
	public class ExceptionEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the stored <see cref="Exception"/>.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
		/// </summary>
		public ExceptionEventArgs(Exception e)
		{
			Exception = e;
		}
	}
}
