// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async.Promises
{
	/// <summary>
	/// Promise-related helpers.
	/// </summary>
	public static class Promise
	{
		#region interface

		/// <summary>
		/// Event raised for unhandled exceptions. For this to work you have to complete your promises with a call to Done().
		/// </summary>
		public static event EventHandler<ExceptionEventArgs> UnhandledException;

		internal static void PropagateUnhandledException(object sender, Exception e)
		{
			UnhandledException?.Invoke(sender, new ExceptionEventArgs(e));
		}

		#endregion
	}
}
