# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

### Added
- Added unit tests for `AsyncResult`.
- Added `FromWebRequest` helpers.
- Added `AsyncContinuationOptions` support to continuations.
- Added support for single-threaded mode via `UNITYFX_ST` define (no locks or interlocked oprations are used in this mode).
- Added [AppVeyor CI](https://ci.appveyor.com/project/Arvtesh/unityfx-async) support.
- Added [GitVersion](https://gitversion.readthedocs.io/en/latest/) support.

### Changed
- Significantly changed `ContinueWith`/`WhenAny`/`WhenAll` interface for both `AsyncResult` and `AsyncFactory` to match [Task](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx) interface as close as possible.
- Renamed `AsyncOperationStatus` values to match [TastStatus](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskstatus(v=vs.110).aspx).
- Renamed `AsyncContinuationOptions` values to match [TastContinuationOptions](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskcontinuationoptions(v=vs.110).aspx).
- Changed project structure: all [Asset Store](https://www.assetstore.unity3d.com/) related stuff is moved to `AssetStore` folder.
- Implemented lazy initialization for all internal data allocated by the library.

### Removed
- Removed `AsyncResult` helpers having `MonoBehaviour` arguments to reduce code complexity (duplication). Use `AsyncFactory` for creation of operations on a specific `MonoBehaviour`.
- Removed `AsyncScheduler.Default` property.

### Fixed
- Fixed `Progress` returning `-1` when the operation is disposed.

## [0.3.1] - 2017-08-01

### Added
- Initial release.

