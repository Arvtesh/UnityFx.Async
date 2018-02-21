// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;

namespace UnityFx.Async
{
#if UNITYFX_SUPPORT_TAP

	/// <summary>
	/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation"/>. This type is intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public struct ConfiguredAsyncAwaitable
	{
		#region data

		private readonly AsyncAwaiter _awaiter;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfiguredAsyncAwaitable"/> struct.
		/// </summary>
		public ConfiguredAsyncAwaitable(IAsyncOperation op, bool continueOnCapturedContext)
		{
			_awaiter = new AsyncAwaiter(op, continueOnCapturedContext);
		}

		/// <summary>
		/// Returns the awaiter.
		/// </summary>
		public AsyncAwaiter GetAwaiter() => _awaiter;

		#endregion
	}

#endif
}
