# Benchmark sample

Measures allocations, capacity and pool growth, and writes one JSON file per scenario run.

## Run it

1. Open the `TweenBenchmark` scene, and set **Commit Hash** on `TweenBenchmarkRunner` to the commit under test. Find it with `git rev-parse --short HEAD`. The run stops if the hash is empty or a placeholder.
2. To try it first, set **Runs Per Scenario** to 1 and **Max Capacity Tweens** to 1000, then press Play. The Editor caps the run at 60 fps. Results from the Editor are marked as not valid for publishing.

## Run it on a device

Results for publishing come from IL2CPP builds on a real device. Add the scene to Build Profiles as the only scene, then make two builds:

| Build | Settings | Measures |
| --- | --- | --- |
| Allocations | Development Build | GC allocations and tween update time |
| Timings | Release, with the `UITWEENS_BENCHMARK` scripting define | Tween update time and frame time |

`GC Allocated In Frame` isn't available in Release, and `ProfilerMarker` is compiled out of Release. The define adds a Stopwatch around the tween update, so Release builds can time it. Without the define, a Release build reports frame time only.

Start the app and leave it running. The screen stays on and shows progress. It takes about 1.6 hours at 60 fps with the default settings, and longer on a slower device. To reduce that, lower **Runs Per Scenario** or turn off **Include Canvas Per Target Mode**.

Frame time can't go below the display's refresh interval on mobile, so use the tween update time to judge capacity. For the full per-frame cost, including uGUI, turn on **Project Settings > Player > Other Settings > Frame Timing Stats** and subtract `capacity-0` from `capacity-N` in `cpu-main-thread` and `cpu-render-thread`.

`tween-update-loop` covers the tween update. `tween-create-per-frame` covers the scenario's own calls that start and cancel tweens. For scenarios that create tweens, add the two together.

Canvas-per-target mode skips scenarios above 1000 targets. Leave it off in the Editor, because it froze the Editor on an Intel Mac with an AMD GPU.

## Results

Each file is written as soon as its scenario ends, to:

`Application.persistentDataPath/UITweensBenchmarks/`

Copy the files to `Documentation~/benchmarks/results/raw/` in the package repository, then run `tools/summarize_results.py` from `Documentation~/benchmarks/`.

Each file follows `Documentation~/benchmarks/benchmark-result.schema.json`.

Pool growth runs once at the start of a launch, because the pool never shrinks. To repeat it, close the app and start it again.
