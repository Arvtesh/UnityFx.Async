// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
#if NET35

	/// <summary>
	/// Represents one or more errors that occur during application execution.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	public class AggregateException : Exception
	{
		#region data

		private readonly List<Exception> _exceptions = new List<Exception>();

		#endregion

		#region interface

		/// <summary>
		/// Gets an enumerator for the <see cref="Exception"/> instances that caused the current exception.
		/// </summary>
		public IEnumerable<Exception> InnerExceptions => _exceptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(IEnumerable<Exception> exceptions)
		{
			_exceptions.AddRange(exceptions);
		}

		#endregion
	}

#endif
}
