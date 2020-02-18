// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.IO;

namespace UnityFx.Async.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="Stream"/> class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class StreamExtensions
	{
		#region interface

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="stream">The stream to read data from.</param>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentException">Thrown if the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the <paramref name="buffer"/> length.</exception>
		/// <exception cref="NotSupportedException">Thrown if the stream does not support reading.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the stream is currently in use by a previous read operation.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the stream has been disposed.</exception>
		/// <returns>An <see cref="IAsyncOperation{TResult}"/> that represents the asynchronous read operation. The value of the result
		/// parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested
		/// if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream
		/// has been reached.</returns>
		public static IAsyncOperation<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
		{
			var op = new ApmResult<Stream, int>(stream);
			stream.BeginRead(buffer, offset, count, OnReadCompleted, op);
			return op;
		}

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="stream">The stream to write data to.</param>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentException">Thrown if the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the <paramref name="buffer"/> length.</exception>
		/// <exception cref="NotSupportedException">Thrown if the stream does not support writing.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the stream is currently in use by a previous write operation.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the stream has been disposed.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the asynchronous write operation.</returns>
		public static IAsyncOperation WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
		{
			var op = new ApmResult<Stream, VoidResult>(stream);
			stream.BeginWrite(buffer, offset, count, OnWriteCompleted, op);
			return op;
		}

		#endregion

		#region implementation

		private static void OnReadCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Stream, int>)asyncResult.AsyncState;

			try
			{
				op.TrySetResult(op.Source.EndRead(asyncResult));
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnWriteCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Stream, VoidResult>)asyncResult.AsyncState;

			try
			{
				op.Source.EndWrite(asyncResult);
				op.TrySetCompleted();
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		#endregion
	}
}
