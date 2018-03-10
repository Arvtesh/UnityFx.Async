// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/data-structures-for-parallel-programming#aggregate-exceptions"/>
	/// <seealso cref="IAsyncOperation"/>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class AggregateException : Exception
	{
		#region data

		private const string _exceptionsName = "_exceptions";
		private readonly ReadOnlyCollection<Exception> _exceptions;

		#endregion

		#region interface

		/// <summary>
		/// Gets an enumerator for the <see cref="Exception"/> instances that caused the current exception.
		/// </summary>
		public ReadOnlyCollection<Exception> InnerExceptions => _exceptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException()
		{
			_exceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(string message)
			: base(message)
		{
			_exceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(string message, Exception innerException)
			: base(message, innerException)
		{
			_exceptions = new ReadOnlyCollection<Exception>(new Exception[] { innerException });
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(params Exception[] exceptions)
			: this(string.Empty, exceptions)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(string message, params Exception[] exceptions)
			: this(message, (IList<Exception>)exceptions)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(IEnumerable<Exception> exceptions)
			: this(string.Empty, exceptions)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(string message, IEnumerable<Exception> exceptions)
			: this(message, exceptions as IList<Exception> ?? (exceptions == null ? null : new List<Exception>(exceptions)))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(IList<Exception> exceptions)
			: this(string.Empty, exceptions)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AggregateException"/> class.
		/// </summary>
		public AggregateException(string message, IList<Exception> exceptions)
			: base(message, exceptions.Count > 0 ? exceptions[0] : null)
		{
			_exceptions = new ReadOnlyCollection<Exception>(exceptions);
		}

		#endregion

		#region ISerializable

		[SecurityCritical]
		private AggregateException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			var innerExceptions = info.GetValue(_exceptionsName, typeof(Exception[])) as Exception[];
			_exceptions = new ReadOnlyCollection<Exception>(innerExceptions);
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
