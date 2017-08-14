# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

### Added
- Added unit tests for `AsyncResult`.
- Added `FromWebRequest` helpers.
- Added [AppVeyor CI](https://ci.appveyor.com/project/Arvtesh/unityfx-async) support.
- Added [GitVersion](https://gitversion.readthedocs.io/en/latest/) support.

### Changed
- Renamed `AsyncOperationStatus` values to match [TastStatus](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskstatus(v=vs.110).aspx).
- Changed project structure: all [Asset Store](https://www.assetstore.unity3d.com/) related stuff is moved to `AssetStore` folder.
- Implemented lazy initialization for all internal data allocated by the library.

### Removed
- Removed `AsyncScheduler.Default` property.

### Fixed
- Fixed `Progress` returning `-1` when the operation is disposed.

## [0.3.1] - 2017-08-01

### Added
- Initial release.

