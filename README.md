# UnityFx.Async

Channel  | UnityFx.Async |
---------|---------------|
AppVeyor | [![Build status](https://ci.appveyor.com/api/projects/status/hfmq9vow53al7tpd/branch/master?svg=true)](https://ci.appveyor.com/project/Arvtesh/unityfx-async/branch/master)
NuGet    | [![NuGet](https://img.shields.io/nuget/v/UnityFx.Async.svg)](https://www.nuget.org/packages/UnityFx.Async)
Github   | [![GitHub release](https://img.shields.io/github/release/Arvtesh/UnityFx.Async.svg)](https://github.com/Arvtesh/UnityFx.Async/releases)

## Synopsis

*UnityFx.Async* is a set of of classes and interfaces that extend [Unity3d](https://unity3d.com) asynchronous operations and can be used very much like [Task-based Asynchronous Pattern (TAP)](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) in .NET. At its core library defines a container ([AsyncResult](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResult.html)) for an asynchronous operation state and result value (aka `promise` or `future`). In many aspects it mimics [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) (for example, it can be used with `async`/`await` operators, supports continuations).

Library is designed as a lightweight and portable [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) alternative (not a replacement though) for [Unity3d](https://unity3d.com). The comparison table below shows how *UnityFx.Async* entities relate to [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)-like [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) stuff:

TPL | UnityFx.Async | Notes
----|---------------|------
[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) | [AsyncResult](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResult.html), [IAsyncOperation](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncOperation.html) | Represents an asynchronous operation.
[Task&lt;TResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1) | [AsyncResult&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResult-1.html), [IAsyncOperation&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncOperation-1.html) | Represents an asynchronous operation that can return a value.
[TaskStatus](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskstatus) | [AsyncOperationStatus](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncOperationStatus.html) | Represents the current stage in the lifecycle of an asynchronous operation.
[TaskCreationOptions](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcreationoptions) | - | Specifies flags that control optional behavior for the creation and execution of asynchronous operations.
[TaskContinuationOptions](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcontinuationoptions) | TODO | Specifies the behavior for an asynchronous operation that is created by using continuation methods (`ContinueWith`).
[TaskCanceledException](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcanceledexception) | - | Represents an exception used to communicate an asynchronous operation cancellation.
[TaskCompletionSource&lt;TResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskcompletionsource-1) | [AsyncCompletionSource](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncCompletionSource.html), [IAsyncCompletionSource](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncCompletionSource.html), [AsyncCompletionSource&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncCompletionSource-1.html), [IAsyncCompletionSource&lt;TResult&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.IAsyncCompletionSource-1.html) | Represents the producer side of an asyncronous operation unbound to a delegate.
[TaskScheduler](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler) | - | Represents an object that handles the low-level work of queuing asynchronous operations onto threads.
[TaskFactory](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory), [TaskFactory&lt;TResult&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory-1) | - | Provides support for creating and scheduling asynchronous operations.
- | [AsyncResultQueue&lt;T&gt;](https://arvtesh.github.io/UnityFx.Async/api/netstandard2.0/UnityFx.Async.AsyncResultQueue-1.html) | A FIFO queue of asynchronous operations executed sequentially.

## Code Example
Typical use-case of the library is wrapping [Unity3d](https://unity3d.com) web requests in [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) manner:
```csharp
public IAsyncOperation<Texture2D> LoadTextureAsync(string textureUrl)
{
    var result = new AsyncCompletionSource<Texture2D>();
    StartCoroutine(LoadTextureInternal(result, textureUrl));
    return result.Operation;
}

private IEnumerator LoadTextureInternal(AsyncCompletionSource<Texture2D> op, string textureUrl)
{
    var www = UnityWebRequest.GetTexture(textureUrl);
    yield return www.SendWebRequest();

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
Once that is done we can use `LoadTextureAsync()` in many ways:
```csharp
// Unity3d coroutines.
IEnumerator TestLoadTextureAsync()
{
    var op = LoadtextureAsync("http://my_texture_url.jpg");
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

// async/await sample. This code requires Unity 2017+.
async Task TestLoadTextureAsync2()
{
    try
    {
        var texture = await LoadtextureAsync("http://my_texture_url.jpg");
        Debug.Log("Yay! The texture is loaded!");
    }
    catch (OperationCanceledException e)
    {
        Debug.LogWarning("The operation was canceled.");
    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}

// Completion callbacks.
void TestLoadTextureAsync3()
{
    LoadTextureAsync("http://my_texture_url.jpg").AddCompletionCallback(op =>
    {
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
    });
}

// Waiting the operation completion on another thread.
void TestLoadTextureAsync4()
{
    var op = LoadTextureAsync("http://my_texture_url.jpg");

    ThreadPool.QueueUserWorkItem(
        args =>
        {
            try
            {
                var texture = (args as IAsyncOperation<Texture>).Join();
                // The texture is loaded
            }
            catch (OperationCanceledException e)
            {
                // The operation was canceled
            }
            catch (Exception e)
            {
                // Load failed
            }
        },
        op);
}
```

## Motivation
The project was initially created to help author with his [Unity3d](https://unity3d.com) projects. Unity's [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) and the like can only be used in coroutines, cannot be extended and mostly do not return result or error information, .NET 3.5 does not provide much help either and even with .NET 4.6 support introduction compatibility requirements ofetn do not allow using [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task). When I caught myself writing the same asynchronous opertion wrappers in each project I decided to start this project for the best of human kind.

## Documentation
Please see the links below for extended information on the product:
- [Documentation](https://arvtesh.github.io/UnityFx.Async/articles/intro.html).
- [API Reference](https://arvtesh.github.io/UnityFx.Async/api/index.html).

See [CHANGELOG](CHANGELOG.md) for information on recent changes.

## Software requirements

- [Microsoft Visual Studio 2017](https://www.visualstudio.com/vs/community/)
- [Unity3d](https://store.unity.com/)

## Contributing

Please see [contributing guide](CONTRIBUTING.md) for details.

## License

Please see the [![license](https://img.shields.io/github/license/Arvtesh/UnityFx.Async.svg)](LICENSE.md) for details.