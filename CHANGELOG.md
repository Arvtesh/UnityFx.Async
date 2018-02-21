# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

-----------------------
## [0.8.0] - unreleased

### Added
- Added `IAsyncCompletionSource.Operation` property to match `TaskCompletionSource` interface.
- Added new constructor to `AsyncResult`.
- Added `AsyncResult.Start` method to match `Task` interface.
- Added `AsyncResult.OnStarted` virtual method.
- Added `WhenAll`/`WhenAny` static helpers for `AsyncResult`.
- Added `ConfigureAwait` extensions for `IAsyncOperation`.

### Changed
- Modified `AsyncResultAwaiter` implementation to throw if the operation was canceled or faulted (to match `TaskAwaiter` behaviour).
- Implemented `AsyncCompletionSource` as a sealed analog of `TaskCompletionSource`.
- Removed public completion methods from `AsyncResult` (moved them to `AsyncCompletionSource`).
- Made `SpinUntilCompleted` an extension method (was `AsyncResult` instance method).
- Changed `IAsyncOperation.Exception` type to `AggregateException` to match `Task`.
- Changed `IAsyncOperationEvents.Completed` event behaviour to always execute handler (event if it was registered after the comperation has copleted).

### Fixed
- `AsyncResultQueue` now does not remove uncompleted operations from the queue.

-----------------------
## [0.7.1] - 2018-02-14

### Added
- Added possibility to store multiple exceptions in `AsyncResult`.
- Added `AggregateException` class for `net35` target.
- Added `IsEmpty` property to `AsyncResultQueue`.

### Changed
- `AsyncResult` implemenatino is changed to prevent returning null operation result when the operation is completed in some cases.

-----------------------
## [0.7.0] - 2018-02-10

### Added
- Added project documentation site (`docs` folder).
- Added unit tests project.
- Added continuations support.
- Added [AppVeyor CI](https://ci.appveyor.com/project/Arvtesh/unityfx-async) support.
- Added [NuGet](https://www.nuget.org/packages/UnityFx.Async/) deployment.
- Added [GitVersion](https://gitversion.readthedocs.io/en/latest/) support.

### Changed
- Renamed `AsyncOperationStatus` values to match [TastStatus](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskstatus(v=vs.110).aspx).

-----------------------
## [0.3.1] - 2017-08-01

### Added
- Initial release.

