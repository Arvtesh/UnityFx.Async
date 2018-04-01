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
- Extensibility. The library operations are designed to be inherited (if needed).
- Thread-safe. The library classes can be safely used from different threads (unless explicitly stated otherwise).
- [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)-like interface and behaviour. In many cases library classes can be used much like corresponding [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) entities.
- [Unity3d](https://unity3d.com) compatibility. This includes possibility to <c>yield</c> any operations in coroutines and net35-compilance.

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
The below listed topics are just a quict summary of the problems and the proposed solutions. For more details on the topic please see this [excellent article](http://www.what-could-possibly-go-wrong.com/promises-for-game-development/).
### Why callbacks are bad
TODO

### Why coroutines are bad
TODO

### Promises to the Rescue
TODO

## Code Example
Typical use-case of the library is wrapping [Unity3d](https://unity3d.com) web requests in [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) manner:
```csharp
public IAsyncOperation<Texture2D> LoadTextureAsync(string textureUrl)
{
    var result = new AsyncCompletionSource<Texture2D>();
    StartCoroutine(LoadTextureInternal(result, textureUrl));
    return result.Operation;
}

private IEnumerator LoadTextureInternal(IAsyncCompletionSource<Texture2D> op, string textureUrl)
{
    var www = UnityWebRequestTexture.GetTexture(textureUrl);
    yield return www.Send();

    if (www.isNetworkError || www.isHttpError)
    {
        op.SetException(new Exception(www.error));
    }
    else
    {
        op.SetResult(((DownloadHandlerTexture)www.downloadHandler).texture);
    }
}
```
Once that is done we can use `LoadTextureAsync()` result in many ways. For example we can yield it in Unity coroutine to wait for its completion:
```csharp
IEnumerator WaitForLoadOperationInCoroutine(string textureUrl)
{
    var op = LoadTextureAsync(textureUrl);
    yield return op;

    if (op.IsCompletedSuccessfully)
    {
        Debug.Log("Yay!");
    }
    else if (op.IsFaulted)
    {
        Debug.LogException(op.Exception);
    }
    else if (op.IsCanceled)
    {
        Debug.LogWarning("The operation was canceled.");
    }
}
```
With Unity 2017+ and .NET 4.6 scripting backend it can be used just like a [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). The await continuation is scheduled on the captured [SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext) (if any):
```csharp
async Task WaitForLoadOperationWithAwait(string textureUrl)
{
    try
    {
        var texture = await LoadTextureAsync(textureUrl);
        Debug.Log("Yay! The texture is loaded!");
    }
    catch (OperationCanceledException)
    {
        Debug.LogWarning("The operation was canceled.");
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```
An operation can have any number of completion callbacks registered:
```csharp
void WaitForLoadOperationInCompletionCallback(string textureUrl)
{
    LoadTextureAsync(textureUrl).AddCompletionCallback(op =>
    {
        if (op.IsCompletedSuccessfully)
        {
            var texture = (op as IAsyncOperation<Texture2D>).Result;
            Debug.Log("Yay!");
        }
        else if (op.IsFaulted)
        {
            Debug.LogException(op.Exception);
        }
        else if (op.IsCanceled)
        {
            Debug.LogWarning("The operation was canceled.");
        }
    });
}
```
Also one can access/wait for operations from other threads:
```csharp
void WaitForLoadOperationInAnotherThread(string textureUrl)
{
    var op = LoadTextureAsync(textureUrl);

    ThreadPool.QueueUserWorkItem(
        args =>
        {
            try
            {
                var texture = (args as IAsyncOperation<Texture2D>).Join();

                // The texture is loaded
            }
            catch (OperationCanceledException)
            {
                // The operation was canceled
            }
            catch (Exception)
            {
                // Load failed
            }
        },
        op);
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

## Performance

The tables below contains comparison of performance to several other popular frameworks (NOTE: this section needs performing more precise tests, the results below might not be accurate enough):

Stat | UnityFx.Async | [C-Sharp-Promise](https://github.com/Real-Serious-Games/C-Sharp-Promise) | [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)
-|-|-|-
Operation data size for 32-bit systems (in bytes) | 28+ | 36+ | 40+
Number of allocations per continuation (`ContinueWith`/`Then`) | 1+ | 5+ | 2+


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
* [C-Sharp-Promise](https://github.com/Real-Serious-Games/C-Sharp-Promise) - another great C# promise library with excellent documentation.
* Everyone who ever commented or left any feedback on the project. It's always very helpful.
