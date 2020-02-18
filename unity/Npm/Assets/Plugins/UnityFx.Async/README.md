# UnityFx.Async
Requires Unity 2018.3 or higher.

## SUMMARY
Lightweight Task-like asynchronous operations (promises) for Unity3d.

Library is designed as a lightweight [Unity3d](https://unity3d.com)-compatible [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) alternative. Main design goals are:
- Minimum object size and number of allocations.
- Extensibility. The library entities are designed to be easily extensible.
- Thread-safe. The library classes can be safely used from different threads (unless explicitly stated otherwise).
- [Promises](https://developers.google.com/web/fundamentals/primers/promises) support. All asyncronous operations in library support promise-like programming.
- [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)-like interface and behaviour. In many cases library classes can be used much like corresponding [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) entities.
- [Unity3d](https://unity3d.com)-specific features and compatibility. This includes possibility to <c>yield</c> operations in coroutines, `net35`-compilance, extensions of Unity asynchronous operations etc.

The table below summarizes differences berween *UnityFx.Async* and other popular asynchronous operation frameworks:

| Stat | UnityFx.Async | [C-Sharp-Promise](https://github.com/Real-Serious-Games/C-Sharp-Promise) | [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) |
| :--- | :---: | :---: | :---: |
| Thread-safe | ✔️ | - | ✔️ |
| .NET 3.5 compilance | ✔️ | ✔️ | - |
| Supports [SynchronizationContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext) capturing | ✔️ | - | ✔️ |
| Supports continuations | ✔️ | ✔️ | ✔️ |
| Supports Unity coroutines | ️✔️ | - | - |
| Supports [async / await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/index) | ✔️ | - | ✔️ |
| Supports [promise](https://www.promisejs.org/)-like continuations | ✔️ | ✔️ | - |
| Supports cancellation | ✔️ | - | ✔️ |
| Supports progress reporting | ✔️ | ✔️ | ✔️ |
| Supports child operations | - | - | ✔️ |
| Supports [Task-like types](https://github.com/dotnet/roslyn/blob/master/docs/features/task-types.md) (requires C# 7.2) | ✔️ | - | ✔️ |
| Supports [ExecutionContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.executioncontext) flow | - | - | ✔️ |
| Minimum allocations per continuation | ~1 | 5+ | 2+ |

**NOTE**: As the table states [ExecutionContext](https://docs.microsoft.com/en-us/dotnet/api/system.threading.executioncontext) flow is NOT supported. Please use [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) if you need it.

## USAGE
Npm package is available at [npmjs.com](https://www.npmjs.com/package/com.unityfx.async). To use it, add the following line to dependencies section of your `manifest.json`. Unity should download and link the package automatically:
```json
{
  "scopedRegistries": [
    {
      "name": "Arvtesh",
      "url": "https://registry.npmjs.org/",
      "scopes": [
        "com.unityfx"
      ]
    }
  ],
  "dependencies": {
    "com.unityfx.async": "1.1.0"
  }
}
```

## USEFUL LINKS
* [Github project](https://github.com/Arvtesh/UnityFx.Async)
* [NuGet package](https://www.nuget.org/packages/UnityFx.Async)
* [Npm package](https://www.npmjs.com/package/com.unityfx.async)
* [AppVeyor](https://ci.appveyor.com/project/Arvtesh/unityfx-async)
* [Unity Asset Store](https://assetstore.unity.com/packages/tools/asynchronous-operations-for-unity-96696)
* [Unity Forums](https://forum.unity.com/threads/asynchronous-operations-for-unity-free.522989/)
* [Documentation](https://github.com/Arvtesh/UnityFx.Async/blob/master/README.md)
* [License](https://github.com/Arvtesh/UnityFx.Async/blob/master/LICENSE.md)
* [Support](mailto:arvtesh@gmail.com)
