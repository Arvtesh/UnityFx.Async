// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using UnityEngine.Networking;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents a <see cref="UnityWebRequest"/> error.
	/// </summary>
	public class UnityWebRequestException : Exception
	{
		#region data

		private const string _responseCodeSerializationName = "_reason";

		private readonly long _responseCode;

		#endregion

		#region interface

		/// <summary>
		/// Gets a response code for the source <see cref="UnityWebRequest"/>.
		/// </summary>
		public long ResponseCode
		{
			get
			{
				return _responseCode;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityWebRequestException"/> class.
		/// </summary>
		public UnityWebRequestException()
			: base("UnityWebRequest error.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityWebRequestException"/> class.
		/// </summary>
		public UnityWebRequestException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityWebRequestException"/> class.
		/// </summary>
		public UnityWebRequestException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityWebRequestException"/> class.
		/// </summary>
		public UnityWebRequestException(string message, long responseCode)
			: base(message)
		{
			_responseCode = responseCode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityWebRequestException"/> class.
		/// </summary>
		public UnityWebRequestException(string message, long responseCode, Exception innerException)
			: base(message, innerException)
		{
			_responseCode = responseCode;
		}

		#endregion

		#region ISerializable

		private UnityWebRequestException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_responseCode = info.GetInt64(_responseCodeSerializationName);
		}

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(_responseCodeSerializationName, _responseCode);
		}

		#endregion
	}
}
