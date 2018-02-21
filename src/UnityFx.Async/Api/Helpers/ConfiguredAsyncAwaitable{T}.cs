// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;

namespace UnityFx.Async
{
#if UNITYFX_SUPPORT_TAP

	/// <summary>
	/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation{T}"/>. This type is intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation{T}"/>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public struct ConfiguredAsyncAwaitable<T>
	{
		#region data

		private readonly AsyncAwaiter<T> _awaiter;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfiguredAsyncAwaitable{T}"/> struct.
		/// </summary>
		public ConfiguredAsyncAwaitable(IAsyncOperation<T> op, bool continueOnCapturedContext)
		{
			_awaiter = new AsyncAwaiter<T>(op, continueOnCapturedContext);
		}

		/// <summary>
		/// Returns the awaiter.
		/// </summary>
		public AsyncAwaiter<T> GetAwaiter() => _awaiter;

		#endregion
	}

#endif
}
