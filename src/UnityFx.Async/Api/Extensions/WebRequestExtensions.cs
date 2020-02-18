// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace UnityFx.Async.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="WebRequest"/> class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class WebRequestExtensions
	{
		#region interface

		/// <summary>
		/// Returns a <see cref="Stream"/> for writing data to the Internet resource as an asynchronous operation.
		/// </summary>
		/// <param name="webRequest">The source <see cref="WebRequest"/>.</param>
		/// <exception cref="NotImplementedException">Thrown if an attempt is made to access the method, when the method is not overridden in a descendant class.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<Stream> GetRequestStreamAsync(this WebRequest webRequest)
		{
			var op = new ApmResult<WebRequest, Stream>(webRequest);
			webRequest.BeginGetRequestStream(OnGetRequestStreamCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous request for an Internet resource.
		/// </summary>
		/// <param name="webRequest">The source <see cref="WebRequest"/>.</param>
		/// <exception cref="NotImplementedException">Thrown if an attempt is made to access the method, when the method is not overridden in a descendant class.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<WebResponse> GetResponseAsync(this WebRequest webRequest)
		{
			var op = new ApmResult<WebRequest, WebResponse>(webRequest);
			webRequest.BeginGetResponse(OnGetResponseCompleted, op);
			return op;
		}

		#endregion

		#region implementation

		private static void OnGetRequestStreamCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<WebRequest, Stream>)asyncResult.AsyncState;

			try
			{
				op.TrySetResult(op.Source.EndGetRequestStream(asyncResult));
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnGetResponseCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<WebRequest, WebResponse>)asyncResult.AsyncState;

			try
			{
				op.TrySetResult(op.Source.EndGetResponse(asyncResult));
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		#endregion
	}
}
