// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// A lightweight <c>net35</c>-compatible asynchronous operation (promise) for <c>Unity3d</c>.
	/// </summary>
	/// <remarks>
	/// This class is the core entity of the library. In many aspects it mimics <c>Task</c> interface and behaviour.
	/// For example, any <see cref="AsyncResult"/> instance can have any number of continuations (added either explicitly
	/// via <c>AddCompletionCallback</c> call or implicitly using <c>async</c>/<c>await</c> keywords). These continuations
	/// can be invoked on a an arbitrary <see cref="SynchronizationContext"/>. The class can be used to implement Asynchronous
	/// Programming Model (APM). There are operation state accessors that can be used exactly like corresponding properties of
	/// <c>Task</c>. While the class implements <see cref="IDisposable"/> disposing is only required if <see cref="AsyncWaitHandle"/>
	/// property was used.
	/// </remarks>
	/// <threadsafety static="true" instance="true"/>
	/// <seealso href="http://www.what-could-possibly-go-wrong.com/promises-for-game-development/">Promises for game development</seealso>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/">How to implement the IAsyncResult design pattern</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming">Task-based Asynchronous Pattern (TAP)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task">Task</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1">TaskCompletionSource</seealso>
	/// <seealso cref="AsyncCompletionSource"/>
	/// <seealso cref="AsyncResult{T}"/>
	/// <seealso cref="IAsyncResult"/>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
#if !NET35
	[AsyncMethodBuilder(typeof(CompilerServices.AsyncResultMethodBuilder))]
#endif
	public partial class AsyncResult : IAsyncOperation, IAsyncContinuation, IEnumerator
	{
		#region data

		private const int _flagCompletionReserved = 0x00010000;
		private const int _flagCompleted = 0x00020000;
		private const int _flagSynchronous = 0x00040000;
		private const int _flagCompletedSynchronously = _flagCompleted | _flagCompletionReserved | _flagSynchronous;
		private const int _flagCancellationRequested = 0x00100000;
		private const int _flagDisposed = 0x00200000;
		private const int _flagContinueOnDefaultContext = 0x00400000;

		private const int _flagDoNotDispose = OptionDoNotDispose << _optionsOffset;
		private const int _flagRunContinuationsAsynchronously = OptionRunContinuationsAsynchronously << _optionsOffset;
		private const int _flagSuppressCancellation = OptionSuppressCancellation << _optionsOffset;

		private const int _statusMask = 0x0000000f;
		private const int _optionsMask = 0x78000000;
		private const int _optionsOffset = 27;

		private static readonly object _callbackCompletionSentinel = new object();
		private static int _idCounter;
		private static SynchronizationContext _defaultContext;

		private readonly object _asyncState;
		private int _id;
		private Exception _exception;
		private EventWaitHandle _waitHandle;
		private volatile int _flags;
		private volatile object _callback;

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets a reference to <see cref="SynchronizationContext"/> that is used for majority of continuations.
		/// </summary>
		/// <remarks>
		/// This property is supposed to be used as allocation optimization in applications working mostly with single
		/// <see cref="SynchronizationContext"/> instance (such as Unity3d applications). Usually this should be set to
		/// a context attached to the app UI thread.
		/// </remarks>
		/// <value>An instance of <see cref="SynchronizationContext"/> that is used as default one. Initial value is <see langword="null"/>.</value>
		[DebuggerHidden]
		public static SynchronizationContext DefaultSynchronizationContext { get => _defaultContext; set => _defaultContext = value; }

		/// <summary>
		/// Gets the <see cref="AsyncCreationOptions"/> used to create this operation.
		/// </summary>
		/// <value>The operation creation options.</value>
		[DebuggerHidden]
		public AsyncCreationOptions CreationOptions => (AsyncCreationOptions)(_flags >> _optionsOffset);

		/// <summary>
		/// Gets a value indicating whether the operation has been started.
		/// </summary>
		/// <value>A value indicating whether the operation has been started.</value>
		[DebuggerHidden]
		public bool IsStarted => (_flags & _statusMask) >= StatusRunning;

		/// <summary>
		/// Gets a value indicating whether the operation in running.
		/// </summary>
		/// <value>A value indicating whether the operation is running.</value>
		[DebuggerHidden]
		public bool IsRunning => (_flags & _statusMask) == StatusRunning;

		/// <summary>
		/// Gets a value indicating whether the operation instance is disposed.
		/// </summary>
		/// <value>A value indicating whether the operation is disposed.</value>
		[DebuggerHidden]
		protected bool IsDisposed => (_flags & _flagDisposed) != 0;

		/// <summary>
		/// Gets a value indicating whether the operation cancellation was requested.
		/// </summary>
		/// <value>A value indicating whether the operation cancellation was requested.</value>
		[DebuggerHidden]
		protected bool IsCancellationRequested => (_flags & _flagCancellationRequested) != 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		public AsyncResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncCallback asyncCallback, object asyncState)
		{
			_asyncState = asyncState;
			_callback = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="CreationOptions"/>.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncResult(AsyncCreationOptions options)
			: this((int)options << _optionsOffset)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncCreationOptions options, object asyncState)
			: this((int)options << _optionsOffset)
		{
			_asyncState = asyncState;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: this((int)options << _optionsOffset)
		{
			_asyncState = asyncState;
			_callback = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		public AsyncResult(AsyncOperationStatus status)
			: this((int)status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/> and <see cref="CreationOptions"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCreationOptions options)
			: this((int)status | ((int)options << _optionsOffset))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, object asyncState)
			: this((int)status)
		{
			_asyncState = asyncState;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/> and <see cref="CreationOptions"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCreationOptions options, object asyncState)
			: this((int)status | ((int)options << _optionsOffset))
		{
			_asyncState = asyncState;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: this((int)status)
		{
			_asyncState = asyncState;
			_callback = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class with the specified <see cref="Status"/> and <see cref="CreationOptions"/>.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		public AsyncResult(AsyncOperationStatus status, AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: this((int)status | ((int)options << _optionsOffset))
		{
			_asyncState = asyncState;
			_callback = asyncCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class that is faulted. For internal use only.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		internal AsyncResult(Exception exception, object asyncState)
		{
			_exception = exception ?? throw new ArgumentNullException(nameof(exception));

			if (_exception is OperationCanceledException)
			{
				_flags = StatusCanceled | _flagCompletedSynchronously;
			}
			else
			{
				_flags = StatusFaulted | _flagCompletedSynchronously;
			}

			_callback = _callbackCompletionSentinel;
			_asyncState = asyncState;
		}

		/// <summary>
		/// Transitions the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <remarks>
		/// <para>An operation may be started on once. Any attempts to schedule it a second time will result in an exception.</para>
		/// <para>The <see cref="Start"/> is used to execute an operation that has been created by calling one of the constructors.
		/// Typically, you do this when you need to separate the operation's creation from its execution, such as when you conditionally
		/// execute operations that you've created.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if the transition has failed.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TryStart"/>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="OnStarted"/>
		public void Start()
		{
			ThrowIfDisposed();

			if (!TrySetRunning())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transitions the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the operation status was changed to <see cref="AsyncOperationStatus.Running"/>; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="Start"/>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="OnStarted"/>
		public bool TryStart()
		{
			ThrowIfDisposed();
			return TrySetRunning();
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetRunning"/>
		protected internal bool TrySetScheduled()
		{
			if (TrySetStatus(StatusScheduled))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetScheduled"/>
		protected internal bool TrySetRunning()
		{
			if (TrySetStatus(StatusRunning))
			{
				OnStarted();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCanceled(bool)"/>
		protected internal bool TrySetCanceled()
		{
			return TrySetCanceled(false);
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCanceled()"/>
		protected internal bool TrySetCanceled(bool completedSynchronously)
		{
			if (TryReserveCompletion())
			{
				_exception = new OperationCanceledException();
				SetCompleted(StatusCanceled, completedSynchronously);
				return true;
			}
			else if (!IsCompleted)
			{
				SpinUntilCompleted();
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/>. The method calls <see cref="TrySetException(Exception)"/>
		/// passing a new <see cref="System.Exception"/> instance with the specified <paramref name="message"/>.
		/// </summary>
		/// <param name="message">An exception message.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception)"/>
		protected internal bool TrySetException(string message)
		{
			return TrySetException(new Exception(message), false);
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/>. The method calls <see cref="TrySetException(Exception, bool)"/>
		/// passing a new <see cref="System.Exception"/> instance with the specified <paramref name="message"/>.
		/// </summary>
		/// <param name="message">An exception message.</param>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception, bool)"/>
		protected internal bool TrySetException(string message, bool completedSynchronously)
		{
			return TrySetException(new Exception(message), completedSynchronously);
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> (or <see cref="AsyncOperationStatus.Canceled"/>
		/// if the exception is <see cref="OperationCanceledException"/>) state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception, bool)"/>
		protected internal bool TrySetException(Exception exception)
		{
			return TrySetException(exception, false);
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Faulted"/> (or <see cref="AsyncOperationStatus.Canceled"/>
		/// if the exception is <see cref="OperationCanceledException"/>) state.
		/// </summary>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetException(Exception)"/>
		protected internal bool TrySetException(Exception exception, bool completedSynchronously)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			if (TryReserveCompletion())
			{
				if (exception is OperationCanceledException)
				{
					_exception = exception;
					SetCompleted(StatusCanceled, completedSynchronously);
				}
				else
				{
					_exception = exception;
					SetCompleted(StatusFaulted, completedSynchronously);
				}

				return true;
			}
			else if (!IsCompleted)
			{
				SpinUntilCompleted();
			}

			return false;
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCompleted(bool)"/>
		protected internal bool TrySetCompleted()
		{
			return TrySetCompleted(false);
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completedSynchronously">Value of the <see cref="CompletedSynchronously"/> property.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="TrySetCompleted()"/>
		protected internal bool TrySetCompleted(bool completedSynchronously)
		{
			if (TrySetCompleted(StatusRanToCompletion, completedSynchronously))
			{
				return true;
			}
			else if (!IsCompleted)
			{
				SpinUntilCompleted();
			}

			return false;
		}

		/// <summary>
		/// Reports changes in operation progress value.
		/// </summary>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		protected internal bool TryReportProgress()
		{
			var status = _flags & _statusMask;

			if (status == StatusRunning)
			{
				OnProgressChanged();
				InvokeProgressCallbacks();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Throws exception if the operation has failed or canceled.
		/// </summary>
		protected internal void ThrowIfNonSuccess()
		{
			var status = _flags & _statusMask;

			if (status == StatusFaulted)
			{
				if (!TryThrowException(_exception))
				{
					// Should never get here. Exception should never be null in faulted state.
					throw new Exception();
				}
			}
			else if (status == StatusCanceled)
			{
				if (!TryThrowException(_exception))
				{
					throw new OperationCanceledException();
				}
			}
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> if this operation has been disposed.
		/// </summary>
		protected internal void ThrowIfDisposed()
		{
			if ((_flags & _flagDisposed) != 0)
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		/// <summary>
		/// Gets a unique ID for an <see cref="AsyncResult"/> instance.
		/// </summary>
		/// <remarks>
		/// This method should be used by all <see cref="IAsyncOperation"/> implementation for generating value of the <see cref="IAsyncOperation.Id"/> property.
		/// </remarks>
		public static int GetNewId()
		{
			var result = 0;

			do
			{
				result = Interlocked.Increment(ref _idCounter);
			}
			while (result == 0);

			return result;
		}

		#endregion

		#region virtual interface

		/// <summary>
		/// Called when the progress is requested. Default implementation returns 0.
		/// </summary>
		/// <remarks>
		/// Make sure that each method call returns a value greater or equal to the previous. It is important for
		/// progress reporting consistency.
		/// </remarks>
		/// <seealso cref="Progress"/>
		/// <seealso cref="OnProgressChanged"/>
		/// <seealso cref="TryReportProgress"/>
		protected virtual float GetProgress()
		{
			return 0;
		}

		/// <summary>
		/// Called when the progress value has changed. Default implementation does nothing.
		/// </summary>
		/// <remarks>
		/// Throwing an exception in this method results in unspecified behaviour.
		/// </remarks>
		/// <seealso cref="Progress"/>
		/// <seealso cref="GetProgress"/>
		/// <seealso cref="TryReportProgress"/>
		protected virtual void OnProgressChanged()
		{
		}

		/// <summary>
		/// Called when the operation state has changed. Default implementation does nothing.
		/// </summary>
		/// <param name="status">The new status value.</param>
		/// <remarks>
		/// Throwing an exception in this method results in unspecified behaviour.
		/// </remarks>
		/// <seealso cref="Status"/>
		/// <seealso cref="TrySetScheduled"/>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCompleted(bool)"/>
		/// <seealso cref="TrySetException(System.Exception, bool)"/>
		protected virtual void OnStatusChanged(AsyncOperationStatus status)
		{
		}

		/// <summary>
		/// Called when the operation is started (<see cref="Status"/> is set to <see cref="AsyncOperationStatus.Running"/>). Default implementation does nothing.
		/// </summary>
		/// <seealso cref="OnCompleted"/>
		/// <seealso cref="Status"/>
		/// <seealso cref="Start"/>
		/// <seealso cref="TryStart"/>
		/// <seealso cref="TrySetRunning"/>
		protected virtual void OnStarted()
		{
		}

		/// <summary>
		/// Called when the operation cancellation has been requested. Default implementation does nothing.
		/// </summary>
		/// <seealso cref="Cancel"/>
		protected virtual void OnCancel()
		{
		}

		/// <summary>
		/// Called when the operation is completed. Default implementation does nothing.
		/// </summary>
		/// <remarks>
		/// Throwing an exception in this method results in unspecified behaviour.
		/// </remarks>
		/// <seealso cref="OnStarted"/>
		/// <seealso cref="Status"/>
		/// <seealso cref="TrySetCanceled(bool)"/>
		/// <seealso cref="TrySetCompleted(bool)"/>
		/// <seealso cref="TrySetException(System.Exception, bool)"/>
		protected virtual void OnCompleted()
		{
		}

		/// <summary>
		/// Releases unmanaged resources used by the object.
		/// </summary>
		/// <remarks>
		/// Unlike most of the members of <see cref="AsyncResult"/>, this method is not thread-safe. Do not throw exceptions in <see cref="Dispose(bool)"/>.
		/// </remarks>
		/// <param name="disposing">A <see langword="bool"/> value that indicates whether this method is being called due to a call to <see cref="Dispose()"/>.</param>
		/// <seealso cref="Dispose()"/>
		/// <seealso cref="ThrowIfDisposed"/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && (_flags & _flagDoNotDispose) == 0)
			{
				_flags |= _flagDisposed;

				if (_waitHandle != null)
				{
#if NET35
					_waitHandle.Close();
#else
					_waitHandle.Dispose();
#endif
					_waitHandle = null;
				}
			}
		}

		#endregion

		#region internals

		internal const int StatusCreated = 0;
		internal const int StatusScheduled = 1;
		internal const int StatusRunning = 2;
		internal const int StatusRanToCompletion = 3;
		internal const int StatusCanceled = 4;
		internal const int StatusFaulted = 5;

		internal const int OptionDoNotDispose = 1;
		internal const int OptionRunContinuationsAsynchronously = 2;
		internal const int OptionSuppressCancellation = 4;

		/// <summary>
		/// Special status setter for <see cref="AsyncOperationStatus.Scheduled"/> and <see cref="AsyncOperationStatus.Running"/>.
		/// </summary>
		internal bool TrySetStatus(int newStatus)
		{
			Debug.Assert(!IsDisposed);
			Debug.Assert(newStatus < StatusRanToCompletion);

			do
			{
				var flags = _flags;

				if ((flags & (_flagCompleted | _flagCompletionReserved)) != 0)
				{
					return false;
				}

				var status = flags & _statusMask;

				if (status >= newStatus)
				{
					return false;
				}

				var newFlags = (flags & ~_statusMask) | newStatus;

				if (Interlocked.CompareExchange(ref _flags, newFlags, flags) == flags)
				{
					OnStatusChanged((AsyncOperationStatus)newStatus);
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Sets the operation status to one of <see cref="AsyncOperationStatus.RanToCompletion"/>/<see cref="AsyncOperationStatus.Canceled"/>/<see cref="AsyncOperationStatus.Faulted"/>.
		/// The call does the same as calling <see cref="TryReserveCompletion"/> and <see cref="SetCompleted(int, bool)"/> but uses one interlocked operation instead of two.
		/// </summary>
		internal bool TrySetCompleted(int status, bool completedSynchronously)
		{
			Debug.Assert(!IsDisposed);
			Debug.Assert(status > StatusRunning);

			status |= _flagCompleted | _flagCompletionReserved;

			if (completedSynchronously)
			{
				status |= _flagSynchronous;
			}

			do
			{
				var flags = _flags;

				if ((flags & (_flagCompletionReserved | _flagCompleted)) != 0)
				{
					return false;
				}

				var newFlags = (flags & ~_statusMask) | status;

				if (Interlocked.CompareExchange(ref _flags, newFlags, flags) == flags)
				{
					NotifyCompleted((AsyncOperationStatus)status);
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Initiates operation completion. Should only be used in pair with <see cref="SetCompleted(int, bool)"/>.
		/// </summary>
		internal bool TryReserveCompletion()
		{
			Debug.Assert(!IsDisposed);

			do
			{
				var flags = _flags;

				if ((flags & (_flagCompletionReserved | _flagCompleted)) != 0)
				{
					return false;
				}

				if (Interlocked.CompareExchange(ref _flags, flags | _flagCompletionReserved, flags) == flags)
				{
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Attempts to add a new flag value.
		/// </summary>
		internal bool TrySetFlag(int newFlag)
		{
			do
			{
				var flags = _flags;

				if ((flags & (newFlag | _flagCompletionReserved | _flagCompleted)) != 0)
				{
					return false;
				}

				if (Interlocked.CompareExchange(ref _flags, flags | newFlag, flags) == flags)
				{
					return true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Unconditionally sets the operation status to one of <see cref="AsyncOperationStatus.RanToCompletion"/>/<see cref="AsyncOperationStatus.Canceled"/>/<see cref="AsyncOperationStatus.Faulted"/>.
		/// Should only be called if <see cref="TryReserveCompletion"/> call succeeded.
		/// </summary>
		internal void SetCompleted(int status, bool completedSynchronously)
		{
			Debug.Assert(!IsDisposed);
			Debug.Assert(status > StatusRunning);
			Debug.Assert((_flags & _flagCompletionReserved) != 0);
			Debug.Assert((_flags & _statusMask) < StatusRanToCompletion);

			var oldFlags = _flags & ~_statusMask;
			var newFlags = status | _flagCompleted;

			if (completedSynchronously)
			{
				newFlags |= _flagSynchronous;
			}

			// Set completed status. After this line IsCompleted will return true.
			Interlocked.Exchange(ref _flags, oldFlags | newFlags);

			// Invoke completion callbacks.
			NotifyCompleted((AsyncOperationStatus)status);
		}

		/// <summary>
		/// Copies state of the specified operation.
		/// </summary>
		internal void CopyCompletionState(IAsyncOperation patternOp, bool completedSynchronously)
		{
			if (!TryCopyCompletionState(patternOp, completedSynchronously))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attemts to copy state of the specified operation.
		/// </summary>
		internal bool TryCopyCompletionState(IAsyncOperation patternOp, bool completedSynchronously)
		{
			if (patternOp.IsCompletedSuccessfully)
			{
				return TrySetCompleted(completedSynchronously);
			}
			else if (patternOp.IsFaulted || patternOp.IsCanceled)
			{
				return TrySetException(patternOp.Exception, completedSynchronously);
			}

			return false;
		}

		/// <summary>
		/// Unconditionally reports the operation progress.
		/// </summary>
		internal void ReportProgress()
		{
			OnProgressChanged();
			InvokeProgressCallbacks();
		}

		/// <summary>
		/// Rethrows the specified exception.
		/// </summary>
		internal static bool TryThrowException(Exception e)
		{
			if (e != null)
			{
#if NET35
				throw e;
#else
				ExceptionDispatchInfo.Capture(e).Throw();
#endif
			}

			return false;
		}

		/// <summary>
		/// Returns a <see cref="SynchronizationContext"/> for the given options.
		/// </summary>
		internal static SynchronizationContext GetSynchronizationContext(AsyncCallbackOptions options)
		{
			SynchronizationContext syncContext;

			if ((options & AsyncCallbackOptions.ExecuteOnDefaultContext) != 0)
			{
				syncContext = _defaultContext;
			}
			else if ((options & AsyncCallbackOptions.ExecuteSynchronously) != 0)
			{
				syncContext = null;
			}
			else
			{
				syncContext = SynchronizationContext.Current;
			}

			return syncContext;
		}

		#endregion

		#region IAsyncOperation

		/// <summary>
		/// Gets a unique ID for the operation instance.
		/// </summary>
		/// <value>Unique non-zero identifier of the operation instance.</value>
		public int Id
		{
			get
			{
				if (_id == 0)
				{
					var newId = GetNewId();
					Interlocked.CompareExchange(ref _id, newId, 0);
				}

				return _id;
			}
		}

		/// <summary>
		/// Gets the operation progress in range [0, 1].
		/// </summary>
		/// <value>Progress of the operation in range [0, 1].</value>
		public float Progress
		{
			get
			{
				var status = _flags & _statusMask;

				if (status == StatusRanToCompletion)
				{
					return 1;
				}
				else if (status < StatusRunning)
				{
					return 0;
				}

				return GetProgress();
			}
		}

		/// <summary>
		/// Gets the operation status identifier.
		/// </summary>
		/// <value>Identifier of the operation status.</value>
		[DebuggerHidden]
		public AsyncOperationStatus Status => (AsyncOperationStatus)(_flags & _statusMask);

		/// <summary>
		/// Gets an exception that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <see langword="null"/>.
		/// </summary>
		/// <value>An exception that caused the operation to end prematurely.</value>
		[DebuggerHidden]
		public Exception Exception => (_flags & _flagCompleted) != 0 ? _exception : null;

		/// <summary>
		/// Gets a value indicating whether the operation completed successfully (i.e. with <see cref="AsyncOperationStatus.RanToCompletion"/> status).
		/// </summary>
		/// <value>A value indicating whether the operation completed successfully.</value>
		[DebuggerHidden]
		public bool IsCompletedSuccessfully => (_flags & _statusMask) == StatusRanToCompletion;

		/// <summary>
		/// Gets a value indicating whether the operation completed due to an unhandled exception (i.e. with <see cref="AsyncOperationStatus.Faulted"/> status).
		/// </summary>
		/// <value>A value indicating whether the operation has failed.</value>
		[DebuggerHidden]
		public bool IsFaulted => (_flags & _statusMask) == StatusFaulted;

		/// <summary>
		/// Gets a value indicating whether the operation completed due to being canceled (i.e. with <see cref="AsyncOperationStatus.Canceled"/> status).
		/// </summary>
		/// <value>A value indicating whether the operation was canceled.</value>
		[DebuggerHidden]
		public bool IsCanceled => (_flags & _statusMask) == StatusCanceled;

		#endregion

		#region IAsyncContinuation

		/// <summary>
		/// Invokes the operation-specific continuation logic. Default implementation attempts to run the operation is <paramref name="op"/> has succeeded;
		/// otherwise the operation transitions to failed state.
		/// </summary>
		/// <param name="op">The completed antecedent operation.</param>
		public virtual void Invoke(IAsyncOperation op)
		{
			if (op.IsCompletedSuccessfully)
			{
				TrySetRunning();
			}
			else if (op.IsCompleted)
			{
				TrySetException(op.Exception);
			}
		}

		#endregion

		#region IAsyncCancellable

		/// <summary>
		/// Initiates cancellation of an asynchronous operation.
		/// </summary>
		/// <remarks>
		/// There is no guarantee that this call will actually cancel the operation or that the operation will be cancelled immidiately.
		/// <see cref="AsyncCreationOptions.SuppressCancellation"/> can be used to suppress this method for a specific operation instance.
		/// </remarks>
		public void Cancel()
		{
			if ((_flags & _flagSuppressCancellation) != 0)
			{
				return;
			}

			if (TrySetFlag(_flagCancellationRequested))
			{
				OnCancel();
			}
		}

		#endregion

		#region IAsyncResult

		/// <summary>
		/// Gets a <see cref="WaitHandle"/> that is used to wait for an asynchronous operation to complete.
		/// </summary>
		/// <remarks>
		/// The handle is lazily allocated on the first property access. Make sure to call <see cref="Dispose()"/> when
		/// the operation instance is not in use.
		/// </remarks>
		/// <value>A <see cref="WaitHandle"/> that is used to wait for an asynchronous operation to complete.</value>
		/// <seealso cref="Dispose()"/>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				ThrowIfDisposed();

				if (_waitHandle == null)
				{
					var done = IsCompleted;
					var mre = new ManualResetEvent(done);

					if (Interlocked.CompareExchange(ref _waitHandle, mre, null) != null)
					{
						// Another thread created this object's event; dispose the event we just created.
#if NET35
						mre.Close();
#else
						mre.Dispose();
#endif
					}
					else if (!done && IsCompleted)
					{
						// We published the event as unset, but the operation has subsequently completed;
						// set the event state properly so that callers do not deadlock.
						_waitHandle.Set();
					}
				}

				return _waitHandle;
			}
		}

		/// <summary>
		/// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
		/// </summary>
		/// <value>A user-defined object that qualifies or contains information about an asynchronous operation.</value>
		[DebuggerHidden]
		public object AsyncState => _asyncState;

		/// <summary>
		/// Gets a value indicating whether the asynchronous operation completed synchronously.
		/// </summary>
		/// <remarks>
		/// For the vast majority of cases this is <see langword="false"/>. Do not rely on this vlaue.
		/// </remarks>
		/// <value><see langword="true"/> if the asynchronous operation completed synchronously; otherwise, <see langword="false"/>.</value>
		/// <seealso cref="IsCompleted"/>
		[DebuggerHidden]
		public bool CompletedSynchronously => (_flags & _flagSynchronous) != 0;

		/// <summary>
		/// Gets a value indicating whether the asynchronous operation has completed.
		/// </summary>
		/// <value><see langword="true"/> if the operation is complete; otherwise, <see langword="false"/>.</value>
		/// <seealso cref="CompletedSynchronously"/>
		[DebuggerHidden]
		public bool IsCompleted => (_flags & _flagCompleted) != 0;

		#endregion

		#region IEnumerator

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <remarks>
		/// Not implemented. Always returns <see langword="null"/>.
		/// </remarks>
		/// <value>
		/// The current element in the collection.
		/// </value>
		object IEnumerator.Current => null;

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <remarks>
		/// Checks whether the operation is completed. Returns <see langword="false"/> if it is; otherwise, <see langword="true"/>.
		/// </remarks>
		/// <returns>Returns <see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
		bool IEnumerator.MoveNext() => (_flags & _statusMask) <= StatusRunning;

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <remarks>
		/// Not implemented. Always throws <see cref="NotSupportedException"/>.
		/// </remarks>
		void IEnumerator.Reset() => throw new NotSupportedException();

		#endregion

		#region IDisposable

		/// <summary>
		/// Disposes the <see cref="AsyncResult"/>, releasing all of its unmanaged resources. This call is only required if
		/// <see cref="AsyncWaitHandle"/> was accessed; otherwise it is safe to ignore this method.
		/// </summary>
		/// <remarks>
		/// Unlike most of the members of <see cref="AsyncResult"/>, this method is not thread-safe.
		/// Also, <see cref="Dispose()"/> may only be called on an <see cref="AsyncResult"/> that is in one of
		/// the final states: <see cref="AsyncOperationStatus.RanToCompletion"/>, <see cref="AsyncOperationStatus.Faulted"/> or
		/// <see cref="AsyncOperationStatus.Canceled"/>.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if the operation is not completed.</exception>
		/// <seealso cref="Dispose(bool)"/>
		public void Dispose()
		{
			if (!IsCompleted)
			{
				throw new InvalidOperationException(Messages.FormatError_OperationIsNotCompleted());
			}

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region implementation

		private string DebuggerDisplay
		{
			get
			{
				var status = Status;
				var result = string.Format("Id = {0}, Status = {1}", Id.ToString(CultureInfo.InvariantCulture), status.ToString());

				if (status == AsyncOperationStatus.Running)
				{
					result += " (" + ((int)(GetProgress() * 100)).ToString(CultureInfo.InvariantCulture) + "%)";
				}
				else if (status == AsyncOperationStatus.Faulted || status == AsyncOperationStatus.Canceled)
				{
					result += " (" + (_exception != null ? _exception.GetType().Name : "null") + ')';
				}

				if (IsDisposed)
				{
					result += ", Disposed";
				}

				return result;
			}
		}

		private AsyncResult(int flags)
		{
			var status = flags & _statusMask;

			if (status == StatusFaulted)
			{
				_exception = new Exception();
			}
			else if (status == StatusCanceled)
			{
				_exception = new OperationCanceledException();
			}

			if (status > StatusRunning)
			{
				_callback = _callbackCompletionSentinel;
				_flags = flags | _flagCompletedSynchronously;
			}
			else
			{
				_flags = flags;
			}
		}

		private void NotifyCompleted(AsyncOperationStatus status)
		{
			try
			{
				OnProgressChanged();
				OnStatusChanged(status);
				OnCompleted();
			}
			finally
			{
				_waitHandle?.Set();
				InvokeCallbacks();
			}
		}

		private void SpinUntilCompleted()
		{
#if NET35

			while ((_flags & _flagCompleted) == 0)
			{
				Thread.SpinWait(1);
			}

#else

			var sw = new SpinWait();

			while ((_flags & _flagCompleted) == 0)
			{
				sw.SpinOnce();
			}

#endif
		}

		#endregion
	}
}
