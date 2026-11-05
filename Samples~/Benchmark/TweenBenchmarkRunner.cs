using System;
using System.Collections;
using System.Collections.Generic;
using UITweens;
using UITweens.Extensions;
using UITweens.Math;
using UITweens.Services;
using Unity.Profiling;
using UnityEngine;

public sealed class TweenBenchmarkRunner : MonoBehaviour
{
    private const int AllocationTargetCount = 400;
    private const int MinMobileFrameRate = 120;
    private const int MaxCanvasPerTargetCount = 1000;
    private const string SharedCanvasVariant = "shared-canvas";
    private const string CanvasPerTargetVariant = "canvas-per-target";

    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool includeCanvasPerTargetMode = true;
    [SerializeField] private int warmupFrames = 120;
    [SerializeField] private int measuredFrames = 600;
    [SerializeField] private int runsPerScenario = 20;
    [SerializeField] private int poolStressCount = 500;
    [Tooltip("Highest capacity step to run. Lower it for a quick test in the Editor.")]
    [SerializeField] private int maxCapacityTweens = 5000;
    [Tooltip("Commit of the package under test. Find it with: git rev-parse --short HEAD")]
    [SerializeField] private string commitHash = "";
    [SerializeField] private int seed = 14051991;
    [SerializeField] private string notes = "";

    // 0 and 10 show the fixed cost each frame, which every user pays.
    private static readonly int[] CapacitySteps = { 0, 10, 50, 100, 150, 350, 1000, 2500, 5000 };
    private static readonly Action NoCaptureCallback = StaticNoCaptureCallback;

    // The pool never shrinks, so growth can only be measured once per process.
    private static bool poolGrowthMeasured;

    // Statics survive Play sessions when Domain Reload is off, but each session gets a new pool.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPoolGrowthState()
    {
        poolGrowthMeasured = false;
    }

    private TweenBenchmarkReport report;
    private TweenBenchmarkTargets targets;
    private TweenBenchmarkStatus status;
    private Camera clearCamera;
    private readonly System.Diagnostics.Stopwatch createStopwatch = new();
    private readonly FrameTiming[] frameTimings = new FrameTiming[1];
    private string validatedCommitHash;

    private readonly struct Scenario
    {
        public readonly string Name;
        public readonly int TargetCount;
        public readonly Action<int, TweenSpawner, TweenBenchmarkTargets> FrameAction;
        public readonly Action<TweenBenchmarkTargets> SetupAction;

        public Scenario(
            string name,
            int targetCount,
            Action<int, TweenSpawner, TweenBenchmarkTargets> frameAction,
            Action<TweenBenchmarkTargets> setupAction = null)
        {
            Name = name;
            TargetCount = targetCount;
            FrameAction = frameAction;
            SetupAction = setupAction;
        }
    }

    private void Start()
    {
        if (!runOnStart)
            return;

        StartCoroutine(RunAll());
    }

    [ContextMenu("Run Benchmark")]
    public void RunBenchmarkNow()
    {
        StopAllCoroutines();
        StartCoroutine(RunAll());
    }

    private IEnumerator RunAll()
    {
        EnsureClearCamera();
        status ??= new TweenBenchmarkStatus(transform);

        if (!TweenBenchmarkReport.TryNormalizeCommitHash(commitHash, out validatedCommitHash))
        {
            const string message = "Benchmark stopped: set Commit Hash on TweenBenchmarkRunner to a real commit.";
            status.Show(message);
            Debug.LogError($"[UITweens.Benchmark] {message}");
            yield break;
        }

        int previousVSyncCount = QualitySettings.vSyncCount;
        int previousTargetFrameRate = Application.targetFrameRate;
        int previousSleepTimeout = Screen.sleepTimeout;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = GetBenchmarkFrameRate();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        try
        {
            UnityEngine.Random.InitState(seed);
            report = new TweenBenchmarkReport();
            targets = new TweenBenchmarkTargets(transform);

            status.Show("Waiting for TweenManager...");
            yield return WaitForTweenManager();

            yield return MeasurePoolGrowth();

            List<Scenario> scenarios = BuildScenarios();
            int variantCount = includeCanvasPerTargetMode ? 2 : 1;
            int total = CountScenarioRuns(scenarios, variantCount);
            int completed = 0;

            // Each run visits every scenario once, so device heat spreads across all of them.
            for (int runIndex = 1; runIndex <= runsPerScenario; runIndex++)
            {
                for (int variantIndex = 0; variantIndex < variantCount; variantIndex++)
                {
                    bool separateCanvasPerTarget = variantIndex == 1;
                    string variant = separateCanvasPerTarget ? CanvasPerTargetVariant : SharedCanvasVariant;

                    for (int i = 0; i < scenarios.Count; i++)
                    {
                        if (!ShouldRun(scenarios[i], separateCanvasPerTarget))
                            continue;

                        completed++;
                        status.Show($"Running {completed}/{total}\n{scenarios[i].Name}\n{variant}, run {runIndex}/{runsPerScenario}");
                        yield return MeasureScenario(scenarios[i], variant, separateCanvasPerTarget, runIndex);
                    }
                }
            }

            string finished = $"Finished: {report.SavedCount} files saved to\n{report.Folder}";
            status.Show(finished);
            Debug.Log($"[UITweens.Benchmark] {finished}");
        }
        finally
        {
            QualitySettings.vSyncCount = previousVSyncCount;
            Application.targetFrameRate = previousTargetFrameRate;
            Screen.sleepTimeout = previousSleepTimeout;
            targets?.DestroyAll();
        }
    }

    // Overlay canvases don't clear the screen, so without a camera each frame draws over the last.
    private void EnsureClearCamera()
    {
        if (clearCamera != null)
            return;

        GameObject cameraObject = new("Benchmark Clear Camera", typeof(Camera));
        cameraObject.transform.SetParent(transform, false);

        clearCamera = cameraObject.GetComponent<Camera>();
        clearCamera.clearFlags = CameraClearFlags.SolidColor;
        clearCamera.backgroundColor = Color.black;
        clearCamera.cullingMask = 0;
        clearCamera.depth = -100f;
    }

    // Mobile platforms ignore vSyncCount, and -1 means a fixed 30 fps there. Editor runs are only
    // for checking that the benchmark works, so they stay capped to go easy on the GPU.
    private static int GetBenchmarkFrameRate()
    {
        if (Application.isEditor)
            return 60;

        if (!Application.isMobilePlatform)
            return -1;

        // Rendering can't exceed the display's refresh rate, so a high target is safe.
        return Mathf.Max(MinMobileFrameRate, Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value));
    }

    // Thousands of canvases measure uGUI rather than tweens, and they froze a Mac's GPU driver.
    private static bool ShouldRun(Scenario scenario, bool separateCanvasPerTarget)
        => !separateCanvasPerTarget || scenario.TargetCount <= MaxCanvasPerTargetCount;

    private int CountScenarioRuns(List<Scenario> scenarios, int variantCount)
    {
        int count = 0;
        for (int variantIndex = 0; variantIndex < variantCount; variantIndex++)
        {
            for (int i = 0; i < scenarios.Count; i++)
            {
                if (ShouldRun(scenarios[i], variantIndex == 1))
                    count++;
            }
        }

        return count * runsPerScenario;
    }

    private List<Scenario> BuildScenarios()
    {
        List<Scenario> scenarios = new()
        {
            new Scenario("create-tweens-each-frame", AllocationTargetCount, OnCreateTweensEachFrame),
            new Scenario("cancel-and-recreate", AllocationTargetCount, OnCancelAndRecreate),
            new Scenario("then-chaining", AllocationTargetCount, OnThenChaining),
            new Scenario("callback-no-capture", AllocationTargetCount, OnCallbackNoCapture),
            new Scenario("callback-with-capture", AllocationTargetCount, OnCallbackWithCapture),
            new Scenario("declare-only-and-wait", AllocationTargetCount, OnDeclareOnlyAndWait)
        };

        for (int i = 0; i < CapacitySteps.Length; i++)
        {
            if (CapacitySteps[i] <= maxCapacityTweens)
                scenarios.Add(new Scenario($"capacity-{CapacitySteps[i]}", CapacitySteps[i], null, StartCapacityTweens));
        }

        scenarios.Add(new Scenario($"pool-stress-{poolStressCount}", poolStressCount, OnPoolStress));
        return scenarios;
    }

    /// <summary>
    /// Runs once at the start of a fresh process, with no warmup, so the pool's first growth is
    /// included. Reports the allocation peak and the last frame that allocated.
    /// </summary>
    private IEnumerator MeasurePoolGrowth()
    {
        if (poolGrowthMeasured)
        {
            Debug.LogWarning("[UITweens.Benchmark] Skipped pool growth: it needs a fresh process.");
            yield break;
        }

        poolGrowthMeasured = true;
        status.Show("Running pool growth");

        targets.Rebuild(poolStressCount, separateCanvasPerTarget: false);
        yield return null;
        yield return null;

        TweenSpawner spawner = TweenManager.GetSpawner();
        TweenBenchmarkSession session = CreateSession();
        int totalFrames = warmupFrames + measuredFrames;
        MeasureAccumulator accumulator = new(totalFrames);

        using ProfilerRecorder gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        bool includeGcMetric = Debug.isDebugBuild && gcRecorder.Valid;
        int lastAllocatingFrame = -1;

#if UITWEENS_BENCHMARK
        TweenPoolManager.TrackGrowth = true;
#endif

        for (int frame = 0; frame < totalFrames; frame++)
        {
            OnPoolStress(frame, spawner, targets);
            accumulator.RecordFrame(Time.unscaledDeltaTime * 1000d);

            if (includeGcMetric)
            {
                long bytes = Math.Max(0L, gcRecorder.LastValue);
                accumulator.RecordGcBytes(bytes);

                if (bytes > 0L)
                    lastAllocatingFrame = frame;
            }

            yield return null;
        }

#if UITWEENS_BENCHMARK
        TweenPoolManager.TrackGrowth = false;
#endif

        targets.CancelAllTweens();
        yield return null;

        TweenBenchmarkResult result = BuildResult(
            $"pool-growth-{poolStressCount}",
            SharedCanvasVariant,
            warmup: 0,
            totalFrames,
            seed,
            runIndex: 1,
            totalRuns: 1,
            session,
            accumulator,
            includeGcMetric,
            includeTweenMetric: false,
            includeCreateMetric: false);

#if UITWEENS_BENCHMARK
        result.metrics.Add(new TweenBenchmarkMetric
        {
            name = "startup-init",
            value = TweenManager.InitializationMilliseconds,
            unit = "ms",
            aggregation = "single"
        });
        result.metrics.Add(new TweenBenchmarkMetric
        {
            name = "startup-gc",
            value = TweenManager.InitializationBytes,
            unit = "bytes",
            aggregation = "single"
        });
        AddSingleMetric(result, "pool-prealloc-time", TweenPoolManager.PreAllocationMilliseconds, "ms");
        AddSingleMetric(result, "pool-prealloc-gc", TweenPoolManager.PreAllocationBytes, "bytes");
        AddSingleMetric(result, "pool-growth-entities", TweenPoolManager.GrowthEntities, "count");
        AddSingleMetric(result, "pool-growth-allocations", TweenPoolManager.GrowthAllocations, "count");
        AddSingleMetric(result, "pool-growth-time-total", TweenPoolManager.GrowthTotalMilliseconds, "ms");
        AddSingleMetric(result, "pool-growth-time-max", TweenPoolManager.GrowthMaxMilliseconds, "ms");
        AddSingleMetric(result, "pool-growth-gc", TweenPoolManager.GrowthBytes, "bytes");
#endif

        if (includeGcMetric)
        {
            result.metrics.Add(new TweenBenchmarkMetric
            {
                name = "gc-last-allocating-frame",
                value = lastAllocatingFrame,
                unit = "count",
                aggregation = "single"
            });
        }

        report.Save(result);
    }

    private IEnumerator MeasureScenario(Scenario scenario, string variant, bool separateCanvasPerTarget, int runIndex)
    {
        targets.Rebuild(scenario.TargetCount, separateCanvasPerTarget);
        yield return null;

        TweenSpawner spawner = TweenManager.GetSpawner();
        TweenBenchmarkSession session = CreateSession();

        scenario.SetupAction?.Invoke(targets);

        MeasureAccumulator accumulator = new(measuredFrames);
        using ProfilerRecorder gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        using ProfilerRecorder tweenRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "UITweens.UpdateLoop");

        bool includeGcMetric = Debug.isDebugBuild && gcRecorder.Valid;
#if UITWEENS_BENCHMARK
        const bool includeTweenMetric = true;
#else
        bool includeTweenMetric = Debug.isDebugBuild && tweenRecorder.Valid;
#endif

        bool includeCreateMetric = scenario.FrameAction != null;
        int totalFrames = warmupFrames + measuredFrames;
        for (int frame = 0; frame < totalFrames; frame++)
        {
            FrameTimingManager.CaptureFrameTimings();

            createStopwatch.Restart();
            scenario.FrameAction?.Invoke(frame, spawner, targets);
            createStopwatch.Stop();

            if (frame >= warmupFrames)
            {
                accumulator.RecordFrame(Time.unscaledDeltaTime * 1000d);
                RecordCpuTiming(accumulator);

                if (includeCreateMetric)
                    accumulator.RecordCreateMs(createStopwatch.ElapsedTicks * (1000d / System.Diagnostics.Stopwatch.Frequency));

                if (includeGcMetric)
                    accumulator.RecordGcBytes(Math.Max(0L, gcRecorder.LastValue));

                if (includeTweenMetric)
                {
#if UITWEENS_BENCHMARK
                    accumulator.RecordTweenNs(TweenManager.LastUpdateLoopNanoseconds);
#else
                    accumulator.RecordTweenNs(Math.Max(0L, tweenRecorder.LastValue));
#endif
                }
            }

            yield return null;
        }

        targets.CancelAllTweens();
        yield return null;

        report.Save(BuildResult(
            scenario.Name,
            variant,
            warmupFrames,
            measuredFrames,
            seed,
            runIndex,
            runsPerScenario,
            session,
            accumulator,
            includeGcMetric,
            includeTweenMetric,
            includeCreateMetric));
    }

    // Unlike frame time, these exclude the wait for the display, so they show real work below the
    // refresh rate. Subtracting capacity-0 gives the full cost of N tweens, including uGUI.
    private void RecordCpuTiming(MeasureAccumulator accumulator)
    {
        if (FrameTimingManager.GetLatestTimings(1, frameTimings) == 0)
            return;

        FrameTiming timing = frameTimings[0];
        if (timing.cpuMainThreadFrameTime > 0d)
            accumulator.RecordCpuMs(timing.cpuMainThreadFrameTime, timing.cpuRenderThreadFrameTime);
    }

#if UITWEENS_BENCHMARK
    private static void AddSingleMetric(TweenBenchmarkResult result, string name, double value, string unit)
    {
        result.metrics.Add(new TweenBenchmarkMetric { name = name, value = value, unit = unit, aggregation = "single" });
    }
#endif

    private TweenBenchmarkSession CreateSession()
        => TweenBenchmarkReport.CreateSession(validatedCommitHash, BuildSessionNotes());

    private static void StartCapacityTweens(TweenBenchmarkTargets benchmarkTargets)
    {
        for (int i = 0; i < benchmarkTargets.Count; i++)
        {
            RectTransform rect = benchmarkTargets.GetRect(i);
            float offset = (i % 2 == 0) ? 36f : -36f;
            rect.DOAnchorPosX(rect.anchoredPosition.x + offset, 0.5f, EaseType.SineInOut).LoopInfinite();
        }
    }

    private static void OnPoolStress(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        for (int i = 0; i < benchmarkTargets.Count; i++)
        {
            RectTransform rect = benchmarkTargets.GetRect(i);
            float targetX = ((frame + i) & 1) == 0 ? 18f : -18f;
            rect.DOAnchorPosX(targetX, 0.08f, EaseType.Linear);
        }
    }

    private static void OnCreateTweensEachFrame(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        int index = frame % benchmarkTargets.Count;
        RectTransform rect = benchmarkTargets.GetRect(index);
        float targetX = ((frame & 1) == 0) ? 46f : -46f;
        rect.DOAnchorPosX(targetX, 0.12f, EaseType.Linear);
    }

    private static void OnCancelAndRecreate(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        int index = frame % benchmarkTargets.Count;
        RectTransform rect = benchmarkTargets.GetRect(index);
        rect.DoCancel();

        float targetY = ((frame & 1) == 0) ? 18f : -18f;
        rect.DOAnchorPosY(targetY, 0.2f, EaseType.QuadInOut);
    }

    private static void OnThenChaining(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        int index = frame % benchmarkTargets.Count;
        RectTransform rect = benchmarkTargets.GetRect(index);
        float x = ((frame & 1) == 0) ? 22f : -22f;
        float y = ((frame & 1) == 0) ? -22f : 22f;

        TweenHandle second = rect.DOAnchorPosY(y, 0.08f, EaseType.SineIn).GetHandle();
        rect.DOAnchorPosX(x, 0.08f, EaseType.SineOut).Then(second);
    }

    private static void OnCallbackNoCapture(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        int index = frame % benchmarkTargets.Count;
        benchmarkTargets
            .GetRect(index)
            .DOScale(Vector3.one * 1.03f, 0.09f, EaseType.QuadOut)
            .OnComplete(NoCaptureCallback);
    }

    private static void OnCallbackWithCapture(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        int index = frame % benchmarkTargets.Count;
        RectTransform rect = benchmarkTargets.GetRect(index);
        float captured = rect.anchoredPosition.x + frame;

        rect.DOAnchorPosX(captured * 0.001f, 0.09f, EaseType.QuadInOut)
            .OnComplete(() => Consume(captured));
    }

    private static void OnDeclareOnlyAndWait(int frame, TweenSpawner spawner, TweenBenchmarkTargets benchmarkTargets)
    {
        int index = frame % benchmarkTargets.Count;
        RectTransform rect = benchmarkTargets.GetRect(index);
        spawner.Sequential(
            rect.DOAnchorPosX(12f, 0.05f, EaseType.Linear).DeclareOnly(),
            spawner.Wait(0.01f),
            rect.DOAnchorPosX(-12f, 0.05f, EaseType.Linear).DeclareOnly());
    }

    private static void StaticNoCaptureCallback()
    {
    }

    private static void Consume(float value)
    {
        _ = value;
    }

    private static TweenBenchmarkResult BuildResult(
        string scenarioName,
        string variant,
        int warmup,
        int measured,
        int randomSeed,
        int runIndex,
        int totalRuns,
        TweenBenchmarkSession session,
        MeasureAccumulator accumulator,
        bool includeGcMetric,
        bool includeTweenMetric,
        bool includeCreateMetric)
    {
        List<TweenBenchmarkMetric> metrics = new()
        {
            new() { name = "frame-time", value = accumulator.FrameMedianMs, unit = "ms", aggregation = "median" },
            new() { name = "frame-time", value = accumulator.FrameP95Ms, unit = "ms", aggregation = "p95" },
            new() { name = "frame-time", value = accumulator.FrameMaxMs, unit = "ms", aggregation = "max" }
        };

        if (includeTweenMetric)
        {
            metrics.Add(new TweenBenchmarkMetric { name = "tween-update-loop", value = accumulator.TweenMedianMs, unit = "ms", aggregation = "median" });
            metrics.Add(new TweenBenchmarkMetric { name = "tween-update-loop", value = accumulator.TweenP95Ms, unit = "ms", aggregation = "p95" });
            metrics.Add(new TweenBenchmarkMetric { name = "tween-update-loop", value = accumulator.TweenMaxMs, unit = "ms", aggregation = "max" });
        }

        if (accumulator.HasCpuTiming)
        {
            metrics.Add(new TweenBenchmarkMetric { name = "cpu-main-thread", value = accumulator.CpuMainMedianMs, unit = "ms", aggregation = "median" });
            metrics.Add(new TweenBenchmarkMetric { name = "cpu-main-thread", value = accumulator.CpuMainP95Ms, unit = "ms", aggregation = "p95" });
            metrics.Add(new TweenBenchmarkMetric { name = "cpu-render-thread", value = accumulator.CpuRenderMedianMs, unit = "ms", aggregation = "median" });
            metrics.Add(new TweenBenchmarkMetric { name = "cpu-render-thread", value = accumulator.CpuRenderP95Ms, unit = "ms", aggregation = "p95" });
        }

        if (includeCreateMetric)
        {
            metrics.Add(new TweenBenchmarkMetric { name = "tween-create-per-frame", value = accumulator.CreateMedianMs, unit = "ms", aggregation = "median" });
            metrics.Add(new TweenBenchmarkMetric { name = "tween-create-per-frame", value = accumulator.CreateP95Ms, unit = "ms", aggregation = "p95" });
            metrics.Add(new TweenBenchmarkMetric { name = "tween-create-per-frame", value = accumulator.CreateMaxMs, unit = "ms", aggregation = "max" });
        }

        if (includeGcMetric)
        {
            metrics.Add(new TweenBenchmarkMetric { name = "gc-allocated-per-window", value = accumulator.TotalGcBytes, unit = "bytes", aggregation = "single" });
            metrics.Add(new TweenBenchmarkMetric { name = "gc-allocated-per-frame", value = accumulator.GcP95Bytes, unit = "bytes", aggregation = "p95" });
            metrics.Add(new TweenBenchmarkMetric { name = "gc-allocated-per-frame", value = accumulator.GcMaxBytes, unit = "bytes", aggregation = "max" });
        }

        return new TweenBenchmarkResult
        {
            timestampUtc = DateTime.UtcNow.ToString("O"),
            session = session,
            workload = new TweenBenchmarkWorkload
            {
                profileOrScenario = scenarioName,
                variant = variant,
                runIndex = runIndex,
                totalRuns = totalRuns,
                warmupFrames = warmup,
                measuredFrames = measured,
                seed = randomSeed
            },
            metrics = metrics
        };
    }

    private IEnumerator WaitForTweenManager()
    {
        while (TweenManager.Instance == null || TweenManager.World == null)
            yield return null;

        yield return null;
        yield return null;

        while (TweenManager.GetSpawner() == null)
            yield return null;
    }

    private string BuildSessionNotes()
    {
        List<string> parts = new();

        if (!string.IsNullOrWhiteSpace(notes))
            parts.Add(notes);

        if (Application.isEditor)
            parts.Add("Editor run, not valid for publishing.");

        parts.Add($"targetFrameRate {Application.targetFrameRate}, display {Screen.currentResolution.refreshRateRatio.value:0.##} Hz.");

        if (!Debug.isDebugBuild)
            parts.Add("GC metrics omitted because this run is not a Development build.");

#if UITWEENS_BENCHMARK
        parts.Add("tween-update-loop timed with Stopwatch.");
#else
        if (!Debug.isDebugBuild)
            parts.Add("tween-update-loop omitted: add the UITWEENS_BENCHMARK define to time it in Release.");
        else
            parts.Add("tween-update-loop timed with ProfilerMarker.");
#endif

        return string.Join(" | ", parts);
    }

    private sealed class MeasureAccumulator
    {
        private readonly List<double> frameMs;
        private readonly List<double> tweenNs;
        private readonly List<double> createMs;
        private readonly List<double> cpuMainMs;
        private readonly List<double> cpuRenderMs;
        private readonly List<double> gcPerFrameBytes;
        private long totalGcBytes;

        public MeasureAccumulator(int expectedSamples)
        {
            int capacity = Math.Max(1, expectedSamples);
            frameMs = new List<double>(capacity);
            tweenNs = new List<double>(capacity);
            createMs = new List<double>(capacity);
            cpuMainMs = new List<double>(capacity);
            cpuRenderMs = new List<double>(capacity);
            gcPerFrameBytes = new List<double>(capacity);
        }

        public double FrameMedianMs => ComputePercentile(frameMs, 50);
        public double FrameP95Ms => ComputePercentile(frameMs, 95);
        public double FrameMaxMs => ComputeMax(frameMs);
        public double TweenMedianMs => ComputePercentile(tweenNs, 50) / 1_000_000d;
        public double TweenP95Ms => ComputePercentile(tweenNs, 95) / 1_000_000d;
        public double TweenMaxMs => ComputeMax(tweenNs) / 1_000_000d;
        public bool HasCpuTiming => cpuMainMs.Count > 0;
        public double CpuMainMedianMs => ComputePercentile(cpuMainMs, 50);
        public double CpuMainP95Ms => ComputePercentile(cpuMainMs, 95);
        public double CpuRenderMedianMs => ComputePercentile(cpuRenderMs, 50);
        public double CpuRenderP95Ms => ComputePercentile(cpuRenderMs, 95);
        public double CreateMedianMs => ComputePercentile(createMs, 50);
        public double CreateP95Ms => ComputePercentile(createMs, 95);
        public double CreateMaxMs => ComputeMax(createMs);
        public long TotalGcBytes => totalGcBytes;
        public double GcP95Bytes => ComputePercentile(gcPerFrameBytes, 95);
        public double GcMaxBytes => ComputeMax(gcPerFrameBytes);

        public void RecordFrame(double milliseconds)
            => frameMs.Add(milliseconds);

        public void RecordTweenNs(long nanoseconds)
            => tweenNs.Add(nanoseconds);

        public void RecordCreateMs(double milliseconds)
            => createMs.Add(milliseconds);

        public void RecordCpuMs(double mainThreadMs, double renderThreadMs)
        {
            cpuMainMs.Add(mainThreadMs);
            cpuRenderMs.Add(renderThreadMs);
        }

        public void RecordGcBytes(long bytes)
        {
            gcPerFrameBytes.Add(bytes);
            totalGcBytes += bytes;
        }

        private static double ComputePercentile(List<double> values, double percentile)
        {
            if (values.Count == 0)
                return 0d;

            List<double> ordered = new(values);
            ordered.Sort();

            double position = (percentile / 100d) * (ordered.Count - 1);
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);

            if (lower == upper)
                return ordered[lower];

            double fraction = position - lower;
            return ordered[lower] + ((ordered[upper] - ordered[lower]) * fraction);
        }

        private static double ComputeMax(List<double> values)
        {
            if (values.Count == 0)
                return 0d;

            double max = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }
    }
}
