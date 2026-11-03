# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

### Added

- `OnFloatUpdate`, `OnVectorUpdate`, `TweenHandle.FloatValue` and `TweenHandle.VectorValue`, for reading custom tween values.

### Fixed

- `Then()` and `Sequential()` now wait for the previous tween, and a waiting tween reads its start value when it starts.
- `DeclareOnly()` now holds a tween until it's passed to `Sequential()`.
- Cancelling a tween now cancels the tweens waiting for it.
- `CustomFloat` and `CustomVector` now run. They were never updated, so they never finished.
- `OnUpdate` callbacks now run.
- Starting a tween from a callback no longer throws an exception, and `OnComplete` now runs after the final value is applied.
- Exceptions in callbacks are now logged, instead of being hidden.
- Finished tweens are now removed from the active set, which kept growing.
- Tweens now cancel themselves when their target is destroyed.
- `BounceOut`, and the bounce curves built on it, now use the correct constant and end at 1.
- Delayed tweens with an explicit start value, such as `DOColorFromTo`, now keep it.

### Changed

- **Breaking:** `TweenBuilder` and `CancelTweenBuilder` are now structs, so creating or cancelling a tween doesn't allocate.

### Documentation

- Added method signatures, recipes, pool size documentation, an architecture diagram and a comparison with other libraries.
- Added contributing, security and issue template files, and a Quick Start sample.
- Removed internal development notes from code comments, and added XML documentation to the main APIs.

## [0.2.0] - Unreleased

### Added

- A `Tests` assembly for the package's test components.

### Changed

- **Breaking:** Renamed the namespaces from `Tools.UITweens.Runtime.Scripts` to `UITweens`.
- **Breaking:** Renamed the assemblies to `UITweens` and `UITweens.Editor`.

## [0.1.0] - Unreleased

First pre-release for private testing.
