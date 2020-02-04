// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UnityFx.Async.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="Socket"/> class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class SocketExtensions
	{
		#region interface

		/// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt from a specified socket and
		/// receives the first block of data sent by the client application.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="acceptSocket">The accepted <see cref="Socket"/> object. This value may be <see langword="null"/>.</param>
		/// <param name="receiveSize">The maximum number of bytes to receive.</param>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<Socket> AcceptAsync(this Socket socket, Socket acceptSocket, int receiveSize)
		{
			var op = new ApmResult<Socket, Socket>(socket);
			socket.BeginAccept(acceptSocket, receiveSize, OnAcceptCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt and receives the first block of data
		/// sent by the client application.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="receiveSize">The maximum number of bytes to receive.</param>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<Socket> AcceptAsync(this Socket socket, int receiveSize)
		{
			var op = new ApmResult<Socket, Socket>(socket);
			socket.BeginAccept(receiveSize, OnAcceptCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<Socket> AcceptAsync(this Socket socket)
		{
			var op = new ApmResult<Socket, Socket>(socket);
			socket.BeginAccept(OnAcceptCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous request for a remote host connection.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/> that represents the remote host.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="remoteEP"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">The <paramref name="socket"/> is listening.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation ConnectAsync(this Socket socket, EndPoint remoteEP)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginConnect(remoteEP, OnConnectCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous request for a remote host connection. The host is specified by an <see cref="IPAddress"/> and a port number.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="address">The <see cref="IPAddress"/> of the remote host.</param>
		/// <param name="port">The port number of the remote host.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="address"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> number is invalid.</exception>
		/// <exception cref="InvalidOperationException">The <paramref name="socket"/> is listening.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation ConnectAsync(this Socket socket, IPAddress address, int port)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginConnect(address, port, OnConnectCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous request for a remote host connection. The host is specified by a host name and a port number.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="host">The name of the remote host.</param>
		/// <param name="port">The port number of the remote host.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="host"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> number is invalid.</exception>
		/// <exception cref="InvalidOperationException">The <paramref name="socket"/> is listening.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation ConnectAsync(this Socket socket, string host, int port)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginConnect(host, port, OnConnectCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous request for a remote host connection. The host is specified by an <see cref="IPAddress"/> array and a port number.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="addresses">At least one <see cref="IPAddress"/>, designating the remote host.</param>
		/// <param name="port">The port number of the remote host.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="addresses"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> number is invalid.</exception>
		/// <exception cref="InvalidOperationException">The <paramref name="socket"/> is listening.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation ConnectAsync(this Socket socket, IPAddress[] addresses, int port)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginConnect(addresses, port, OnConnectCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins an asynchronous request to disconnect from a remote endpoint.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="reuseSocket"><see langword="true"/> if this socket can be reused after the connection is closed; otherwise, <see langword="false"/>.</param>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="NotSupportedException">The operating system is Windows 2000 or earlier, and this method requires Windows XP.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation DisconnectAsync(this Socket socket, bool reuseSocket)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginDisconnect(reuseSocket, OnDisconnectCompleted, op);
			return op;
		}

		/// <summary>
		/// Sends data asynchronously to a connected <see cref="Socket"/>.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="buffers">An array of bytes that is the storage location for the received data.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffers"/> is <see langword="null"/>.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<int> SendAsync(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
		{
			var op = new ApmResult<Socket, int>(socket);
			socket.BeginSend(buffers, socketFlags, OnSendCompleted, op);
			return op;
		}

		/// <summary>
		/// Sends data asynchronously to a connected <see cref="Socket"/>.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="buffer">An array of bytes that contains the data to send.</param>
		/// <param name="offset">The zero-based position in the <paramref name="buffer"/> at which to begin sending data.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> is less than 0 or <paramref name="offset"/> is less
		/// than the length of <paramref name="buffer"/> or <paramref name="size"/> is less than 0 or <paramref name="size"/> is greater
		/// than the length of <paramref name="buffer"/> minus the value of the <paramref name="offset"/> parameter.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<int> SendAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
		{
			var op = new ApmResult<Socket, int>(socket);
			socket.BeginSend(buffer, offset, size, socketFlags, OnSendCompleted, op);
			return op;
		}

		/// <summary>
		/// Sends the file <paramref name="fileName"/> to a connected <see cref="Socket"/> object using the <see cref="TransmitFileOptions.UseDefaultWorkerThread"/> flag.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="fileName">A string that contains the path and name of the file to send. This parameter can be <see langword="null"/>.</param>
		/// <exception cref="FileNotFoundException">The file <paramref name="fileName"/> was not found.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="NotSupportedException">The operating system is not Windows NT or later or rhe <paramref name="socket"/> is not connected to a remote host.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation SendFileAsync(this Socket socket, string fileName)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginSendFile(fileName, OnSendFileCompleted, op);
			return op;
		}

		/// <summary>
		/// Sends the file <paramref name="fileName"/> to a connected <see cref="Socket"/> object.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="fileName">A string that contains the path and name of the file to send. This parameter can be <see langword="null"/>.</param>
		/// <param name="preBuffer">A byte array that contains data to be sent before the file is sent. This parameter can be <see langword="null"/>.</param>
		/// <param name="postBuffer">A byte array that contains data to be sent after the file is sent. This parameter can be <see langword="null"/>.</param>
		/// <param name="flags">A bitwise combination of <see cref="TransmitFileOptions"/> values.</param>
		/// <exception cref="FileNotFoundException">The file <paramref name="fileName"/> was not found.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="NotSupportedException">The operating system is not Windows NT or later or rhe <paramref name="socket"/> is not connected to a remote host.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation SendFileAsync(this Socket socket, string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
		{
			var op = new ApmResult<Socket, VoidResult>(socket);
			socket.BeginSendFile(fileName, preBuffer, postBuffer, flags, OnSendFileCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins to asynchronously receive data from a connected <see cref="Socket"/>.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="buffers">An array of bytes that is the storage location for the received data.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffers"/> is <see langword="null"/>.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<int> ReceiveAsync(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
		{
			var op = new ApmResult<Socket, int>(socket);
			socket.BeginReceive(buffers, socketFlags, OnReceiveCompleted, op);
			return op;
		}

		/// <summary>
		/// Begins to asynchronously receive data from a connected <see cref="Socket"/>.
		/// </summary>
		/// <param name="socket">The target socket.</param>
		/// <param name="buffer">An array of bytes that is the storage location for the received data.</param>
		/// <param name="offset">The zero-based position in the <paramref name="buffer"/> at which to store the received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> is less than 0 or <paramref name="offset"/> is greater
		/// than the length of <paramref name="buffer"/> or <paramref name="size"/> is less than 0 or <paramref name="size"/> is greater
		/// than the length of <paramref name="buffer"/> minus the value of the <paramref name="offset"/> parameter.</exception>
		/// <exception cref="SocketException">An error occurred when attempting to access the <paramref name="socket"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="socket"/> has been closed.</exception>
		/// <returns>Returns <see cref="IAsyncOperation{TResult}"/> representing the asynchronous operation.</returns>
		public static IAsyncOperation<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
		{
			var op = new ApmResult<Socket, int>(socket);
			socket.BeginReceive(buffer, offset, size, socketFlags, OnReceiveCompleted, op);
			return op;
		}

		#endregion

		#region implementation

		private static void OnAcceptCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Socket, Socket>)asyncResult.AsyncState;

			try
			{
				op.TrySetResult(op.Source.EndAccept(asyncResult));
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnConnectCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Socket, VoidResult>)asyncResult.AsyncState;

			try
			{
				op.Source.EndConnect(asyncResult);
				op.TrySetCompleted();
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnDisconnectCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Socket, VoidResult>)asyncResult.AsyncState;

			try
			{
				op.Source.EndDisconnect(asyncResult);
				op.TrySetCompleted();
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnSendCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Socket, int>)asyncResult.AsyncState;

			try
			{
				op.TrySetResult(op.Source.EndSend(asyncResult));
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnSendFileCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Socket, VoidResult>)asyncResult.AsyncState;

			try
			{
				op.Source.EndSendFile(asyncResult);
				op.TrySetCompleted();
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		private static void OnReceiveCompleted(IAsyncResult asyncResult)
		{
			var op = (ApmResult<Socket, int>)asyncResult.AsyncState;

			try
			{
				op.TrySetResult(op.Source.EndReceive(asyncResult));
			}
			catch (Exception e)
			{
				op.TrySetException(e);
			}
		}

		#endregion
	}
}
