# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

### Added

- Added `TweenManager.Configure` and `TweenManager.Prewarm`.
- Added `OnComplete(state, callback)`, which runs a callback with a value and doesn't allocate a closure.
- Added a benchmark sample under `Samples~/Benchmark`. It covers allocation, capacity and pool growth, and writes one JSON file per scenario run.
- Added the benchmark result schema, evidence rules and summary script under `Documentation~/benchmarks/`.
- Added a `UITweens.UpdateLoop` profiler marker around the tween update, including orphan cleanup.
- Added `TweenManager.LastUpdateLoopNanoseconds`, available with the `UITWEENS_BENCHMARK` define, so Release builds can time the tween update.

### Documentation

- Removed the unpublished 350+ tweens and zero-GC claims until a reproducible benchmark exists.
- Said that the package isn't used in production or in a shipped game yet.

### Changed

- **Breaking:** removed `UITweenPoolConfig` and its asset lookup. Use `TweenManager.Configure` instead.
- The pool is no longer created during startup. 50 entities are prepared in small batches afterward, and it adds 12 on the next update when 2 or fewer are left. The old minimum of 64 is gone.
- Setup waits 30 frames after the first scene loads, unless a tween starts earlier.
- Startup now runs in five stages on separate frames, so it doesn't land on one frame. Added `TweenManager.IsReady`. A tween that starts earlier finishes the remaining stages at once.
- **Breaking:** `TweenManager` creates itself in code, so **Tools > UI Tweens > Setup** and the `TweenManager` prefab are no longer needed. You can delete the prefab.
- LeoECS filters are created the first time a tween type is used, systems run without reflection, and the ECS world starts smaller. This lowers startup time and memory.
- The update loop reads the active tweens directly instead of copying them each frame, and tween tracking uses a cheaper key.
- **Breaking:** `DeclareOnly()` returns the `TweenDeclaration` struct, and `TweenWait` is a struct. `Sequential()` no longer allocates with up to four steps.
- Entities start with room for 12 components, so tweens with sequences or callbacks don't resize them.
- Finished tweens return to a pool for their property type, so reuse keeps their components. The pool grows only when more tweens run at once than before. Old handles stop working when a tween ends.
- Systems update only the list of active tweens, so tweens waiting in the pool cost nothing each frame, even after a large burst. LeoECS filters are no longer used, which also removes their startup and per-tween cost.
- **Breaking:** the clock, control, result, target, apply flags and delay components are now fields of one `TweenCoreComponent`. Each ECS component type costs extra the first time it's used, so this lowers startup and first-tween cost.
- **Breaking:** removed `TweenManager.RegisterTransform`, `RegisterCanvasGroup` and `RegisterImage`. Cleanup now reads each tween's own target, so tweens no longer register objects separately.
- Tweens add loop and advanced callback data only when they use it, and position tweens are tracked once. This lowers the cost of creating and updating tweens.
- `DoCancel` and other per-component tween lookups no longer scan every tracked tween, and replaced tweens are cancelled with less work.
- Test logs use PASS and FAIL instead of emojis.

### Fixed

- `DoPause`, `DoPlay` and `DoTogglePause` now act on every tween on the component. They used to affect only one.
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
