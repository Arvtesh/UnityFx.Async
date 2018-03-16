// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Networking;

namespace UnityFx.Async
{
	/// <summary>
	/// tt
	/// </summary>
	public class UnityWebRequestException : Exception
	{
		#region data

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
		public UnityWebRequestException(string message, long responseCode)
			: base(message)
		{
			_responseCode = responseCode;
		}

		#endregion
	}
}
