# What is this?

*UnityFx.Async* is a set of of classes and interfaces that extend [Unity3d](https://unity3d.com) asynchronous operations and can be used very much like [TPL](https://msdn.microsoft.com/en-us/library/dd460717(v=vs.110).aspx) in .NET. Basically it defines a generic container of an asynchronous operation result that can be accessed after the operation completes (aka `promise` or `future`).

Quick example:
```csharp
var op = AsyncResult.FromWebRequest(UnityWebRequest.Get("https://www.google.com"));
yield return op;

if (op.IsCompletedSuccessfully)
{
	// TODO
}
```

# Why do I need this?

## Unity3d API issues
While Unity3d is a great engine, there are quite a few places where its API is not ideal. Asynchronous operations and coroutines management are the examples. While the concept of coroutines itself is great for frame-based applications, current Unity implementation is not consistent at least:
- There is no single base class/interface for yieldable entities. For example [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) and [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) both inherit [YieldInstruction](https://docs.unity3d.com/ScriptReference/YieldInstruction.html), while [CustomYieldInstruction](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html) and [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) do not.
- Running a coroutine requires [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) instance which is not always convenient.
- Unity3d built-in asynchronous operations provide very little control after they have been started, [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) for example doesn't even provide a way to determine if it is completed.
- There is no standard way to return a coroutine result value. While some of the [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html)-derived classes define operation results, [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) uses completely inconsistent way of doing this.
- There is no easy way of chaining coroutines, waiting for completion of a coroutine group etc.
- Coroutines can't handle exceptions, because `yield return` statements cannot be surrounded with a try-catch block.

## UnityFx.Async features
- **Single base interface** for all kinds of library asyncronous operations: [IAsyncResult](https://msdn.microsoft.com/en-us/library/system.iasyncresult(v=vs.110).aspx). Note that it is also the base interface for the .NET [Task](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task(v=vs.110).aspx). This also means one can combine .NET asynchronous operations and the library operations.
- **No `MonoBehaviour` needed**: operations defined in the library can be created without specifying a `MonoBehaviour` instance to run on. There is an abstract `AsyncScheduler` class that can be used to implement custom coroutine runner logic.
- **Extended control** over the operations: `IAsyncOperation` interface mimics .NET [Task](https://msdn.microsoft.com/en-us/library/system.threading.tasks.task(v=vs.110).aspx) as much as possible.
- **Operation result** can be returned with the generic `IAsyncOperation<T>` interface (again very similar to [Task<T>](https://msdn.microsoft.com/en-us/library/dd321424(v=vs.110).aspx)).
- **Chaning of operations** can be easily achieved with a set of extension methods for `IAsyncOperation` interface.
- **Error/exception handling** is provided out-of-the-box for all predefined operations. Any exception thrown inside an operation update loop immmediately finish its execution with an error.
- **Cancellation** of an `IAsyncOperation` instance is achieved via usage of [CancellationToken](https://msdn.microsoft.com/en-us/library/system.threading.cancellationtoken(v=vs.110).aspx) values.
- **Yieldable/awaitable** implementation of the `IAsyncOperation` interface is provided to allow easy library extension.