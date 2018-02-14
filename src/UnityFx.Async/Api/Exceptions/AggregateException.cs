// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;

namespace UnityFx.Async
{
#if NET35

	/// <summary>
	/// Represents one or more errors that occur during application execution.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class AggregateException : Exception
	{
		#region data

		private const string _exceptionsName = "_exceptions";
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

		#region ISerializable

		[SecurityCritical]
		private AggregateException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			var innerExceptions = info.GetValue(_exceptionsName, typeof(Exception[])) as Exception[];
			_exceptions.AddRange(innerExceptions);
		}

		/// <inheritdoc/>
		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			var innerExceptions = new Exception[_exceptions.Count];
			_exceptions.CopyTo(innerExceptions, 0);
			info.AddValue(_exceptionsName, innerExceptions, typeof(Exception[]));
		}

		#endregion

		#region Obejct

		/// <inheritdoc/>
		public override string ToString()
		{
			var text = base.ToString();

			for (var i = 0; i < _exceptions.Count; ++i)
			{
				text = string.Format(
					CultureInfo.InvariantCulture,
					"{0}{1}{2}: {3}{4}{5}",
					text,
					Environment.NewLine,
					i,
					_exceptions[i].ToString(),
					"<---",
					Environment.NewLine);
			}

			return text;
		}

		#endregion

		#region implementation

		private string DebuggerDisplay
		{
			get
			{
				return "Count = " + _exceptions.Count.ToString(CultureInfo.InvariantCulture);
			}
		}

		#endregion
	}

#endif
}
