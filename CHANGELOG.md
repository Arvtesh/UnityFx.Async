# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

### Added
- Unit tests for `AsyncResult` added.

### Changed
- Renamed `AsyncOperationStatus` values to match [TastStatus](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskstatus(v=vs.110).aspx).
- Changed project structure: all Asset Store related stuff is moved to `AssetStore` folder.

### Fixed
- Fixed `Progress` returning -1 when the operation is disposed.

## [0.3.1] - 2017-08-01

### Added
- Initial release.

