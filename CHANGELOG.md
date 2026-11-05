# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

### Added

- Added a benchmark sample under `Samples~/Benchmark`. It covers allocation, capacity and pool growth, and writes one JSON file per scenario run.
- Added the benchmark result schema, evidence rules and summary script under `Documentation~/benchmarks/`.
- Added a `UITweens.UpdateLoop` profiler marker around the tween update, including orphan cleanup.
- Added `TweenManager.LastUpdateLoopNanoseconds`, available with the `UITWEENS_BENCHMARK` define, so Release builds can time the tween update.

### Documentation

- Removed the unpublished 350+ tweens and zero-GC claims until a reproducible benchmark exists.
- Said that the package isn't used in production or in a shipped game yet.

### Changed

- Tweens add loop and advanced callback data only when they use it, and position tweens are tracked once. This lowers the cost of creating and updating tweens.
- `DoCancel` and other per-component tween lookups no longer scan every tracked tween, and replaced tweens are cancelled with less work.
- Test logs use PASS and FAIL instead of emojis.

### Fixed

- `DoCancel` now cancels every tween on the component. It used to cancel one and stop tracking the rest, so they kept playing and couldn't be cancelled.
- Starting a tween in `Start()` on the first frame no longer throws a `NullReferenceException`. `TweenManager` now finishes its setup when the first tween needs it.
- `CubicInOut` no longer jumps at the halfway point.

### Documentation

- Added an animated easing reference.

## [0.3.0] - 2026-11-03

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

- The install steps now use LeoECS `2022.2.22`, the last release under the standard MIT license. Later releases use a different license.
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
