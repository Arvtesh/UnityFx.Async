# UnityFx.Async

Channel  | UnityFx.Async |
---------|---------------|
AppVeyor | [![Build status](https://ci.appveyor.com/api/projects/status/hfmq9vow53al7tpd/branch/master?svg=true)](https://ci.appveyor.com/project/Arvtesh/unityfx-async/branch/master) [![AppVeyor tests](https://img.shields.io/appveyor/tests/Arvtesh/unityFx-async.svg)](https://ci.appveyor.com/project/Arvtesh/unityfx-async/build/tests)
NuGet | [![NuGet](https://img.shields.io/nuget/v/UnityFx.Async.svg)](https://www.nuget.org/packages/UnityFx.Async) [![NuGet](https://img.shields.io/nuget/vpre/UnityFx.Async.svg)](https://www.nuget.org/packages/UnityFx.Async)
Github | [![GitHub release](https://img.shields.io/github/release/Arvtesh/UnityFx.Async.svg?logo=github)](https://github.com/Arvtesh/UnityFx.Async/releases)
Unity Asset Store | [![Asynchronous operations for Unity](https://img.shields.io/badge/tools-v0.8.2-green.svg)](https://assetstore.unity.com/packages/tools/asynchronous-operations-for-unity-96696)

## Synopsis

*UnityFx.Async* is a set of of classes and interfaces that extend [Unity3d](https://unity3d.com) asynchronous operations and can be used very much like [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) in .NET or [Promises](https://developers.google.com/web/fundamentals/primers/promises) in Javascript. The library at its core defines a container ([AsyncResult](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResult.html)) for state and result value of an asynchronous operation (aka `promise` or `future`). In many aspects it mimics [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) (for example, it can be used with `async`/`await` operators, supports continuations and synchronization context capturing).

Library is designed as a lightweight [Unity3d](https://unity3d.com)-compatible [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) alternative (not a replacement though). Main design goals are:
- Minimum object size and number of allocations.
- Extensibility. The library operations are designed to be inherited.
- Thread-safe. The library classes can be safely used from different threads (unless explicitly stated otherwise).
- [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)-like interface and behaviour. In many cases library classes can be used much like corresponding [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) entities.
- [Unity3d](https://unity3d.com) compatibility. This includes possibility to <c>yield</c> operations in coroutines and net35-compilance.

The table below summarizes differences berween *UnityFx.Async* and other popular asynchronous operation frameworks:

| Stat | UnityFx.Async | [C-Sharp-Promise](https://github.com/Real-Serious-Games/C-Sharp-Promise) | [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) |
| :--- | :---: | :---: | :---: |
| Thread-safe | ✔️ | - | ✔️ |
| Can capture synchronization context | ✔️ | - | ✔️ |
| .NET 3.5 compilance | ✔️ | ✔️ | -️️ |
| Supports continuations | ✔️ | ✔️ | ✔️ |
| Supports Unity coroutines | ️️✔️ | - | - |
| Supports `async` / `await` | ✔️ | - | ✔️ |
| Supports `promise`-like continuations | ✔️ | ✔️ | - |
| Supports child operations | - | - | ✔️ |
| Minimum operation data size for 32-bit systems (in bytes) | 28+ | 36+ | 40+ |
| Minimum number of allocations per continuation | 1+ | 5+ | 2+ |

## Getting Started
### Prerequisites
You may need the following software installed in order to build/use the library:
- [Microsoft Visual Studio 2017](https://www.visualstudio.com/vs/community/).
- [Unity3d](https://store.unity.com/).

### Getting the code
You can get the code by cloning the github repository using your preffered git client UI or you can do it from command line as follows:
```cmd
git clone https://github.com/Arvtesh/UnityFx.Async.git
git submodule -q update --init
```
### Getting binaries
The binaries are available as a [NuGet package](https://www.nuget.org/packages/UnityFx.Async). See [here](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) for instructions on installing a package via nuget. One can also download them directly from [Github releases](https://github.com/Arvtesh/UnityFx.Async/releases). Unity3d users can import corresponding [Unity Asset Store package](https://assetstore.unity.com/packages/tools/asynchronous-operations-for-unity-96696) from the editor.

## Understanding the concepts
The below listed topics are just a quick summary of problems and the proposed solutions. For more details on the topic please see useful links at the end of this document.
### Callback hell
Getting notified of an asynchronous operation completion via callbacks is the most common (as well as low-level) approach. It is very simple and obvious at first glance:
```csharp
InitiateSomeAsyncOperation(
    result =>
    {
        // Success handler
    },
    e =>
    {
        // Error handler
    });
```
Now let's chain several operations:
```csharp
InitiateSomeAsyncOperation(
    result =>
    {
        InitiateAsyncOperation2(result,
            result2 =>
            {
                InitiateAsyncOperation3(result2,
                    result3 =>
                    {
                        // ...
                    },
                    e =>
                    {
                        // Error handler 3
                    });
            },
            e =>
            {
                // Error handler 2
            });
    },
    e =>
    {
        // Error handler
    });
```
Doesn't look that simple now, right? And that's just the async method calls without actual result processing and error handling. Production code would have `try` / `catch` blocks in each handler  and much more result processing code. The code complexity (and maintainability problems as a result) produced by extensive callback usage is exactly what is called a [callback hell](http://callbackhell.com/).

### Unity coroutines - another way to shoot yourself in the foot
Coroutines are another popular approach of programming asynchronous operations available for Unity users by default. While it allows convenient way of operation chaining there are quite a lot of drawbacks that make it not suited well for large applications:
* Coroutines cannot return result values (since the return type must be `IEnumerator`).
* Coroutines can't handle exceptions, because `yield return` statements cannot be surrounded with a `try`-`catch` construction. This makes error handling a pain.
* Coroutine require a `MonoBehaviour` to run.
* There is no way to wait for a coroutine other than yield.
* There is no way to get coroutine state information.

That said, here is the previous example rewrited using coroutines:
```csharp
var result = new MyResultType();
yield return InitiateSomeAsyncOperation(result);

var result2 = new MyResultType2();
yield return InitiateAsyncOperation2(result, result2);

var result3 = new MyResultType3();
yield return InitiateAsyncOperation3(result2, result3);

/// ...
/// No way to handle exceptions here
```
As you can see while coroutines allow more streamlined coding, we have to wrap result values into custom classes and there is no easy way of error handling.

### Promises to the rescue
Promises are a design pattern to structure asynchronous code as a sequence of chained (not nested!) operations. This concept was introduces for JS and has even become a [standard](https://promisesaplus.com/) since then. At low level a promise is an object containing a state (Running, Resolved or Rejected), a result value and (optionally) success/error callbacks. At high level the point of promises is to give us functional composition and error handling is the async world.

Let's rewrite the last callback hell sample using promises:
```csharp
InitiateSomeAsyncOperation()
    .Then(result => InitiateAsyncOperation2(result))
    .Then(result2 => InitiateAsyncOperation3(result2))
    .Then(result3 => /* ... */)
    .Catch(e => /* Shared error handler */);
```
This does exaclty the same job as the callbacks sample, but it's much more readable.

That said promises are still not an ideal solution (at least for C#). They still require quite a lot of filler code at rely heavily on delegate usage.

### Asynchronous programming with async and await
C# 5.0/.NET 4.5 introduced a new appoach to asynchronous programming. By using `async` and `await` one can write an asynchronous methods almost as synchronous methods. The following example shows implementation of the callback hell method with this technique:
```csharp
try
{
    var result = await InitiateSomeAsyncOperation();
    var result2 = await InitiateAsyncOperation2(result);
    var result3 = await InitiateAsyncOperation3(result2);
    // ...
}
catch (Exception e)
{
    // Error handling code
}
```
In fact the only notable difference from synchronous implementation is usage of the mentioned `async` and `await` keywords. It's worth mentioning that there is lots of hidden work done by both the C# compliter and asynchronous operation to allow this.

## Using the library
Reference the DLL and import the namespace:
```csharp
using UnityFx.Async;
```
Create an operation instance like this:
```csharp
var op = new AsyncCompletionSource<string>();
```
The type of the operation should reflect its result type. In this case we have created a special kind of operation - a completion source that incapsulated both producer and consumer interfaces (consumer side is represented via `IAsyncOperation` / `IAsyncOperation<TResult>` interfaces and producer side is `IAsyncCompletionSource` / `IAsyncCompletionSource<TResult>`, `AsyncCompletionSource` implements both of the interfaces).

Upon completion of an asynchronous operation transition it to one of the final states (`RanToCompletion`, `Faulted` or `Canceled`):
```csharp
op.SetResult(resultValue);
```
Or, if the operation has failed:
```csharp
op.SetException(ex);
```

To see it in context, here is an example of a function that downloads text from URL using [UnityWebRequest](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html):
```csharp
public IAsyncOperation<string> DownloadTextAsync(string url)
{
    var result = new AsyncCompletionSource<string>();
    StartCoroutine(DownloadTextInternal(result, url));
    return result;
}

private IEnumerator DownloadTextInternal(IAsyncCompletionSource<string> op, string url)
{
    var www = UnityWebRequest.Get(url);
    yield return www.Send();

    if (www.isNetworkError || www.isHttpError)
    {
        op.SetException(new Exception(www.error));
    }
    else
    {
        op.SetResult(www.downloadHandler.text);
    }
}
```

### Waiting for an operation to complete
The simpliest way to get notified of an operation completion is registering a completion handler to be invoked when the operation succeeds (the JS promise-like way):
```csharp
DownloadTextAsync("http://www.google.com")
    .Then(text => Debug.Log(text));
```
The above code downloads content of Google's front page and prints it to Unity console. To make this example closer to real life applications let's add simple error handling code to it:
```csharp
DownloadTextAsync("http://www.google.com")
    .Then(text => Debug.Log(text))
    .Catch(e => Debug.LogException(e));
```
One can also yield the operation in Unity coroutine:
```csharp
var op = DownloadTextAsync("http://www.google.com");
yield return op;

if (op.IsCompletedSuccessfully)
{
    Debug.Log(op.Result);
}
else if (op.IsFaulted)
{
    Debug.LogException(op.Exception);
}
else if (op.IsCanceled)
{
    Debug.LogWarning("The operation was canceled.");
}
```
With Unity 2017+ and .NET 4.6 it can be used just like a [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). The await continuation is scheduled on a captured [SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext) (if any):
```csharp
try
{
    var text = await DownloadTextAsync("http://www.google.com");
    Debug.Log(text);
}
catch (OperationCanceledException)
{
    Debug.LogWarning("The operation was canceled.");
}
catch (Exception e)
{
    Debug.LogException(e);
}
```
Or, you can just block current thread while waiting (don't do that from UI thread!):
```csharp
try
{
    using (var op = DownloadTextAsync("http://www.google.com"))
    {
        var text = op.Join();
        Debug.Log(text);
    }
}
catch (Exception e)
{
    Debug.LogException(e);
}
```

### Chaining asynchronous operations
Multiple asynchronous operations can be chained one after other using `Then` / `Rebind` / `ContinueWith` / `Catch` / `Finally`:
```csharp
DownloadTextAsync("http://www.google.com")
    .Then(text => ExtractFirstParagraph(text))
    .Then(firstParagraph => Debug.Log(firstParagraph))
    .Catch(e => Debug.LogException(e))
    .Finally(() => Debug.Log("Done"));
```
The chain of processing ends as soon as an exception occurs. In this case when an error occurs the `Catch` handler would be called.

`Then` continuations get executed only if previous operation in the chain completed successfully. Otherwise they are skipped. Also note that `Then` expects the handler return value to be another operation.

`Rebind` is a special kind of continuation for transforming operation result to a different type:
```csharp
DownloadTextAsync("http://www.google.com")
    .Then(text => ExtractFirstUrl(text))
    .Rebind(url => new Url(url));
```
`ContinueWith` and `Finally` delegates get called independently of the antecedent operation result. `ContinueWith` also define overloads accepting `AsyncContinuationOptions` argument that allows to customize its behaviour:
```csharp
DownloadTextAsync("http://www.google.com")
    .ContinueWith(op => Debug.Log("1"))
    .ContinueWith(op => Debug.Log("2"), AsyncContinuationOptions.NotOnCanceled)
    .ContinueWith(op => Debug.Log("3"), AsyncContinuationOptions.OnlyOnFaulted);
```
That said with .NET 4.6 the recommented approach is using `async` / `await`:
```csharp
try
{
    var text = await DownloadTextAsync("http://www.google.com");
    var firstParagraph = await ExtractFirstParagraph(text);
    Debug.Log(firstParagraph);
}
catch (Exception e)
{
    Debug.LogException(e);
}
finally
{
    Debug.Log("Done");
}
```

### Synchronization context capturing
The default behaviour of all library methods is to capture current [SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext) and try to schedule continuations on it. If there is no synchronization context attached to current thread, continuations are executed on a thread that initiated an operation completion. The same behaviour applies to `async` / `await` implementation unless explicitly overriden with `ConfigureAwait`:
```csharp
// thread1
await DownloadTextAsync("http://www.google.com");
// Back on thread1
await DownloadTextAsync("http://www.yahoo.com").ConfigureAwait(false);
// Most likely some other thread
```

### Completion callbacks
All operations defined in the library support adding of completion callbacks that are executed when an operation completes:
```csharp
var op = DownloadTextAsync("http://www.google.com");
op.Completed += o => Debug.Log("1");
op.AddCompletionCallback(o => Debug.Log("2"));
```
Unlike `ContinueWith`-like stuff completion callbacks cannot be chained and do not handle exceptions automatically. Throwing an exception from a completion callback results in unspecified behavior.

There is also a low-level continuation interface (`IAsyncContinuation`):
```csharp
class MyContinuation : IAsyncContinuation
{
    public void Invoke(IAsyncOperation op) => Debug.Log("Done");
}

// ...

var op = DownloadTextAsync("http://www.google.com");
op.AddContinuation(new MyContinuation());
```

### Disposing of operations
All operations implement [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable) interface. So strictly speaking users should call `Dispose` when an operation is not in use. That said library implementation only requires this if `AsyncWaitHandle` was accessed (just like [tasks](https://blogs.msdn.microsoft.com/pfxteam/2012/03/25/do-i-need-to-dispose-of-tasks/)).

Please note that `Dispose` implementation is NOT thread-safe and can only be called after an operation has completed (the same restrictions apply to [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)).

### Completed asynchronous operations
There are a number of helper methods for creating completed operations:
```csharp
var op1 = AsyncResult.CompletedOperation;
var op2 = AsyncResult.FromResult(10);
var op3 = AsyncResult.FromException(new Exception());
var op4 = AsyncResult.FromCanceled();
```

### Convertions
Library defines convertion methods between `IAsyncOperation` and `Task`, `IObservable`, `UnityWebRequest`, `AsyncOperation`, `WWW`:
```csharp
var task = op.ToTask();
var observable = op.ToObservable();

var op1 = task.ToAsync();
var op2 = observable.ToAsync();
var op3 = unityWebRequest.ToAsync();
var op4 = unityAsyncOperation.ToAsync();
var op5 = unityWWW.ToAsync();
```

### Creating own asynchronous operations
Most common way of creating own asynchronous operation is instantiating `AsyncCompletionSource` instance and call `SetResult` / `SetException` / `SetCanceled` when done. Still there are cases when more control is required. For this purpose the library provides two public extendable implementations for asynchronous operations:
* `AsyncResult`: an operation without a result value.
* `AsyncResult<TResult>`: an asynchronous operation with a generic result value.

The sample code below demostrates creating a delay operation:
```csharp
public class TimerDelayResult : AsyncResult
{
    private readonly Timer _timer;

    public TimerDelayResult(int millisecondsDelay)
        : base(AsyncOperationStatus.Running)
    {
        _timer = new Timer(TimerCompletionCallback, this, millisecondsDelay, Timeout.Infinite);
    }

    protected override void OnCompleted()
    {
        _timer.Dispose();
        base.OnCompleted();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void TimerCompletionCallback(object state)
    {
        (state as TimerDelayResult).TrySetCompleted(false);
    }
}
```

## Comparison to .NET Tasks
The comparison table below shows how *UnityFx.Async* entities relate to [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task):

TPL | UnityFx.Async | Notes
----|---------------|------
[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) | [AsyncResult](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResult.html), [IAsyncOperation](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncOperation.html) | Represents an asynchronous operation.
[Task&lt;TResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1) | [AsyncResult&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResult-1.html), [IAsyncOperation&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncOperation-1.html) | Represents an asynchronous operation that can return a value.
[TaskStatus](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskstatus) | [AsyncOperationStatus](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncOperationStatus.html) | Represents the current stage in the lifecycle of an asynchronous operation.
[TaskCreationOptions](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcreationoptions) | - | Specifies flags that control optional behavior for the creation and execution of asynchronous operations.
[TaskContinuationOptions](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcontinuationoptions) | [AsyncContinuationOptions](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncContinuationOptions.html) | Specifies the behavior for an asynchronous operation that is created by using continuation methods (`ContinueWith`).
[TaskCanceledException](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcanceledexception) | - | Represents an exception used to communicate an asynchronous operation cancellation.
[TaskCompletionSource&lt;TResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1) | [AsyncCompletionSource](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncCompletionSource.html), [IAsyncCompletionSource](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncCompletionSource.html), [AsyncCompletionSource&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncCompletionSource-1.html), [IAsyncCompletionSource&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncCompletionSource-1.html) | Represents the producer side of an asyncronous operation unbound to a delegate.
[TaskScheduler](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler) | - | Represents an object that handles the low-level work of queuing asynchronous operations onto threads.
[TaskFactory](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory), [TaskFactory&lt;TResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory-1) | - | Provides support for creating and scheduling asynchronous operations.
&#45; | [AsyncResultQueue&lt;T&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResultQueue-1.html) | A FIFO queue of asynchronous operations executed sequentially.

Please note that the library is NOT a replacement for [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) or [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl). As a general rule it is recommended to use [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) and only switch to *UnityFx.Async* if one of the following applies:
- .NET 3.5/[Unity3d](https://unity3d.com) compatibility is required.
- Memory usage is a concern ([Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) tend to do quite a lot of allocations).
- An extendable [IAsyncResult](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncresult) implementation is needed.

## Future work
* Progress reporting (via [IProgress](https://docs.microsoft.com/en-us/dotnet/api/system.iprogress-1)).
* Cancellation support (via [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)).

## Motivation
The project was initially created to help author with his [Unity3d](https://unity3d.com) projects. Unity's [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) and the like can only be used in coroutines, cannot be extended and mostly do not return result or error information, .NET 3.5 does not provide much help either and even with .NET 4.6 support compatibility requirements often do not allow using [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). When I caught myself writing the same asynchronous operation wrappers in each project I decided to share my experience for the best of human kind.

## Documentation
Please see the links below for extended information on the product:
- [Unity forums](https://forum.unity.com/threads/asynchronous-operations-for-unity-free.522989/).
- [Documentation](https://arvtesh.github.io/UnityFx.Async/articles/intro.html).
- [API Reference](https://arvtesh.github.io/UnityFx.Async/api/index.html).
- [CHANGELOG](CHANGELOG.md).

## Useful links
- [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap).
- [Asynchronous programming with async and await (C#)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).
- [.NET Task reference source](https://referencesource.microsoft.com/#mscorlib/System/threading/Tasks/Task.cs).
- [Promise pattern](https://en.wikipedia.org/wiki/Futures_and_promises).
- [Promises for Game Development](http://www.what-could-possibly-go-wrong.com/promises-for-game-development/).
- [Promises/A+ Spec](https://promisesaplus.com/).
- [Unity coroutines](https://docs.unity3d.com/Manual/Coroutines.html).

## Contributing

Please see [contributing guide](CONTRIBUTING.md) for details.

## Versioning

The project uses [SemVer](https://semver.org/) versioning pattern. For the versions available, see [tags in this repository](https://github.com/Arvtesh/UnityFx.Async/tags).

## License

Please see the [![license](https://img.shields.io/github/license/Arvtesh/UnityFx.Async.svg)](LICENSE.md) for details.

## Acknowledgments
Working on this project is a great experience. Please see below list of sources of my inspiration (in no particular order):
* [.NET reference source](https://referencesource.microsoft.com/mscorlib/System/threading/Tasks/Task.cs.html). A great source of knowledge and good programming practices.
* [C-Sharp-Promise](https://github.com/Real-Serious-Games/C-Sharp-Promise). Another great C# promise library with excellent documentation.
* [UniRx](https://github.com/neuecc/UniRx). A deeply reworked [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET) port to Unity.
* Everyone who ever commented or left any feedback on the project. It's always very helpful.
