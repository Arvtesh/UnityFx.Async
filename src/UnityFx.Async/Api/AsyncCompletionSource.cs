// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents an asynchronous operation with external completion control.
	/// </summary>
	/// <threadsafety static="true" instance="true"/>
	/// <seealso cref="AsyncCompletionSource{T}"/>
	public class AsyncCompletionSource : AsyncResult, IAsyncCompletionSource
	{
		#region data

		private float _progress;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		public AsyncCompletionSource()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(object asyncState)
			: base(default(AsyncCallback), asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCallback asyncCallback, object asyncState)
			: base(asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncCompletionSource(AsyncCreationOptions options)
			: base(options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCreationOptions options, object asyncState)
			: base(options, null, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: base(options, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		public AsyncCompletionSource(AsyncOperationStatus status)
			: base(status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, object asyncState)
			: base(status, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: base(status, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCreationOptions options)
			: base(status, options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCreationOptions options, object asyncState)
			: base(status, options, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: base(status, options, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetScheduled"/>
		/// <seealso cref="SetRunning"/>
		public void SetScheduled()
		{
			ThrowIfDisposed();

			if (!base.TrySetScheduled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetScheduled"/>
		/// <seealso cref="TrySetRunning"/>
		public new bool TrySetScheduled()
		{
			ThrowIfDisposed();
			return base.TrySetScheduled();
		}

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="SetScheduled"/>
		public void SetRunning()
		{
			ThrowIfDisposed();

			if (!base.TrySetRunning())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetRunning"/>
		/// <seealso cref="TrySetScheduled"/>
		public new bool TrySetRunning()
		{
			ThrowIfDisposed();
			return base.TrySetRunning();
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override float GetProgress()
		{
			return _progress;
		}

		/// <inheritdoc/>
		protected override void OnCancel()
		{
			TrySetCanceled(false);
		}

		#endregion

		#region IAsyncCompletionSource

		/// <summary>
		/// Gets the operation being controller by the source.
		/// </summary>
		/// <value>The underlying operation instance.</value>
		public IAsyncOperation Operation => this;

		/// <summary>
		/// Attempts to set the operation progress value in range [0, 1].
		/// </summary>
		/// <param name="progress">The operation progress in range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="progress"/> is not in range [0, 1].</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public bool TrySetProgress(float progress)
		{
			ThrowIfDisposed();

			if (progress < 0 || progress > 1)
			{
				throw new ArgumentOutOfRangeException(nameof(progress), progress, Messages.FormatError_InvalidProgressValue());
			}

			// Make sure the operation has been started before going further.
			base.TrySetRunning();

			// Now try set the new progress value.
			if (!IsCompleted)
			{
				if (_progress != progress)
				{
					_progress = progress;
					ReportProgress();
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetCanceled()
		{
			ThrowIfDisposed();
			return TrySetCanceled(false);
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetCompleted()
		{
			ThrowIfDisposed();
			return TrySetCompleted(false);
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		public new bool TrySetException(Exception exception)
		{
			ThrowIfDisposed();
			return TrySetException(exception, false);
		}

		#endregion
	}
}
