# What is this?

*UnityFx.Async* is a set of of classes and interfaces that extend [Unity3d](https://unity3d.com) asynchronous operations and can be used very much like [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) in .NET. At its core library defines a container (`AsyncResult`) for an asynchronous operation state and result value (aka `promise` or `future`). In many aspects it mimics [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). For example, any `AsyncResult` instance can have any number of continuations (added either explicitly via `TryAddCompletionCallback` call or implicitly using `async`/`await` keywords). These continuations can be invoked on a captured [SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext) (if any). The class inherits [IAsyncResult](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncresult) (just like [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)) and can be used for [Asynchronous Programming Model (APM)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm) implementation.

Quick [Unity3d](https://unity3d.com) example:
```csharp
IEnumerator Foo()
{
	yield return AsyncResult.Delay(10);
	// do something 10ms later
}
```
Or using .NET 4.6 and higher:
```csharp
async Task Foo()
{
	await AsyncResult.Delay(10);
	// do something 10ms later
}
```
Processing a result of asynchronous operation:
```csharp
void Foo(IAsyncOperation<int> op)
{
	// The callback will be called even if the operation is already completed
	op.AddCompletionCallback(o =>
	{
		if (o.IsCompletedSuccessfully)
		{
			Debug.Log("Result: " + (o as IAsyncOperation<int>).Result);
		}
		else
		{
			Debug.LogException(o.Exception);
		}
	});
}
```
Wrapping a [Task&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1) with `AsyncResult` promise:
```csharp
IAsyncOperation<int> Foo(Task<int> task)
{
	var result = new AsyncCompletionSource<int>();

	task.ContinueWith(t =>
	{
		if (t.IsFaulted)
		{
			result.SetException(task.Exception);
		}
		else if (t.IsCanceled)
		{
			result.SetCanceled();
		}
		else
		{
			result.SetResult(t.Result);
		}
	});

	return result;
}
```
Please note that while `AsyncResult` is designed as a lightweight and portable [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) alternative, it's NOT a replacement for [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). It is recommended to use [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) when possible and only switch to `AsyncResult` if one of the following applies:
* .NET 3.5 compatibility is required.
* Operations should be used in [Unity3d](https://unity3d.com) coroutines.
* Memory usage is a concern.
* You follow [Asynchronous Programming Model (APM)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm) and need [IAsyncResult](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncresult) implementation.

# Why do I need this?

## Unity3d API issues
While Unity3d is a great engine, there are quite a few places where its API is not ideal. Asynchronous operations and coroutines management are the examples. While the concept of coroutines itself is great for frame-based applications, current Unity implementation is very basic and is not too consistent:
- There is no single base class/interface for yieldable entities. For example [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) and [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) both inherit [YieldInstruction](https://docs.unity3d.com/ScriptReference/YieldInstruction.html), while [CustomYieldInstruction](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html) and [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) do not.
- Running a coroutine requires [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) instance which is not always convenient.
- Unity3d built-in asynchronous operations provide very little control after they have been started, [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) for example doesn't even provide a way to determine if it is completed.
- There is no standard way to return a coroutine result value. While some of the [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html)-derived classes define operation results, [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) uses completely different approach.
- There is no easy way of chaining coroutines, waiting for completion of a coroutine group etc.
- Error handling is problematic when using coroutines, because `yield return` statements cannot be surrounded with a try-catch block and there is no straightforward way or returning data from a coroutine.

## UnityFx.Async features
- **Single base interface** for all kinds of library asyncronous operations: [IAsyncResult](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncresult). Note that it is also the base interface for the .NET [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task).
- **No `MonoBehaviour` needed**: operations defined in the library do not need a `MonoBehaviour` instance to run on.
- **Extended control** over the operations: `IAsyncOperation` interface mimics .NET [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) as much as possible.
- **Operation result** can be returned with the generic `IAsyncOperation<T>` interface (again very similar to [Task<T>](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1).
- **Chaning of operations** can be easily achieved with a `ContinueWith` methods for `IAsyncOperation` interface.
- **Yieldable/awaitable** implementation of the `IAsyncOperation` interface is provided to allow easy library extension.
