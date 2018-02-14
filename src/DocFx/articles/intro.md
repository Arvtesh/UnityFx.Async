# What is this?

*UnityFx.Async* is a set of of classes and interfaces that extend [Unity3d](https://unity3d.com) asynchronous operations and can be used very much like [TAP](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) in .NET. At its core library defines a container (`AsyncResult`) for an asynchronous operation state and result value (aka `promise` or `future`). The .NET analog is [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task).

Quick [Unity3d](https://unity3d.com) example:
```csharp
var op = AsyncResult.Delay(10);
yield return op;

if (op.IsCompletedSuccessfully)
{
	// TODO
}
```
Or using .NET 4.6 and higher:
```csharp
var op = AsyncResult.Delay(10);
await op;

if (op.IsCompletedSuccessfully)
{
	// TODO
}
```

You can use `AsyncResult` class as a cheap and portable replacement of [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). Most of the functionality provided by the library can be used in .NET 3.5 (obviously async/await related stuff requires .NET 4.6 or higher).

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
- **No `MonoBehaviour` needed**: operations defined in the library can be created without specifying a `MonoBehaviour` instance to run on.
- **Extended control** over the operations: `IAsyncOperation` interface mimics .NET [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) as much as possible.
- **Operation result** can be returned with the generic `IAsyncOperation<T>` interface (again very similar to [Task<T>](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1).
- **Chaning of operations** can be easily achieved with a `ContinueWith` methods for `IAsyncOperation` interface.
- **Yieldable/awaitable** implementation of the `IAsyncOperation` interface is provided to allow easy library extension.