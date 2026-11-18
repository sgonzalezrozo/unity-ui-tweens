# Performance

[← Back to README](../README.md)

Measured in IL2CPP Release builds on four phones, with Unity `6000.3.6f1`. Each scenario ran 5 times, with 120 warm-up frames and 600 measured frames per run. Values are medians of the runs. "Update loop" is UI Tweens' own update time per frame.

| Metric | Pixel 5 (mid-range Android) | Galaxy A02s (low-end Android) | iPhone 13 mini (2021) | iPhone Air (2025) |
|---|---|---|---|---|
| Update loop, 10 tweens, median / p95 | 0.050 / 0.065 ms | 0.084 / 0.134 ms | 0.021 / 0.054 ms | 0.009 / 0.014 ms |
| Update loop, 100 tweens, median / p95 | 0.180 / 0.266 ms | 0.464 / 0.668 ms | 0.113 / 0.122 ms | 0.059 / 0.108 ms |
| Update loop, 250 tweens, median / p95 | 0.370 / 0.576 ms | 1.187 / 1.655 ms | 0.293 / 0.319 ms | 0.160 / 0.195 ms |
| Update loop, 1,000 tweens, median / p95 | 0.971 / 1.132 ms | 3.702 / 4.463 ms | 1.021 / 1.188 ms | 0.583 / 0.671 ms |
| Replace 500 tweens every frame: update loop / main thread | 0.92 / 4.33 ms | 3.31 / 11.59 ms | 1.04 / 4.96 ms | 0.57 / 2.85 ms |
| Frame rate while replacing 500 tweens every frame | 90 fps (the cap) | 60 fps (the cap) | 60 fps (the cap) | 60 fps (the cap) |
| Create one tween | 0.021 ms | 0.040 ms | 0.008 ms | 0.004 ms |
| Startup: setup + first tween, one launch | 4.91 + 1.07 ms | 9.93 + 1.67 ms | 9.24 + 1.85 ms | 5.77 + 1.00 ms |
| Garbage collections, all 17 scenarios | 0 | 0 | 0 | 0 |

- **No garbage collections ran in any of the 17 scenarios.** The median heap growth was 0 KB in every scenario. The only growth was in the first of the 5 runs: the capturing-callback scenario, where the test's own lambda creates a closure (up to 84 KB), and 4 KB once in a game-like burst on the A02s.
- **The update loop excludes Unity's own UI work.** Redrawing the moving UI costs the same with any tween library, and it's most of the main-thread time while tweens run.
- **These numbers are from commit `fe3c748`, between 0.3.0 and 0.4.0.** Version 0.4.0 added `IgnoreTimeScale`, Yoyo loops and `WhenDisabled` to the update and hasn't been measured yet. The allocation tests in `Tests/AllocationTests.cs` check that the current code doesn't allocate.

The raw results for every run are in [`benchmarks/results/raw/`](benchmarks/results/raw/), and the medians in [`results/summary/summary.json`](benchmarks/results/summary/summary.json). [`benchmarks/README.md`](benchmarks/README.md) explains the format. Numbers from your own project will differ, so measure it.

UI Tweens pools tween entities to reduce allocations. Your own code can still allocate; see [Callbacks without allocations](code-examples.md#callbacks-without-allocations).
