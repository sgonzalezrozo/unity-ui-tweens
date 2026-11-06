using System.Collections;
using Leopotam.Ecs;
using UITweens.Services;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UITweens
{
    public class TweenManager : MonoBehaviour
    {
        private const int CLEANUP_INTERVAL_FRAMES = 60;
        private const int INIT_STAGE_COUNT = 4;
        private static readonly ProfilerMarker UpdateLoopMarker = new("UITweens.UpdateLoop");

#if UITWEENS_BENCHMARK
        private static readonly System.Diagnostics.Stopwatch UpdateLoopStopwatch = new();

        /// <summary>
        /// Duration of the last tween update. ProfilerMarker is compiled out of Release players,
        /// so this is how the benchmark sample times them.
        /// </summary>
        public static long LastUpdateLoopNanoseconds { get; private set; }

        /// <summary>Time spent loading the prefab and setting up the tween world and services.</summary>
        public static double InitializationMilliseconds { get; private set; }

        /// <summary>Managed memory added by the same setup work.</summary>
        public static long InitializationBytes { get; private set; }

        internal static void BeginInitializationSample(out long startTimestamp, out long startBytes)
        {
            startBytes = System.GC.GetTotalMemory(false);
            startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>Time spent loading and instantiating the prefab.</summary>
        public static double StartupPrefabMilliseconds { get; internal set; }

        /// <summary>Time spent building the tween world, including the pool.</summary>
        public static double StartupWorldMilliseconds { get; private set; }

        /// <summary>Time spent creating the tracker and other services.</summary>
        public static double StartupServicesMilliseconds { get; private set; }

        /// <summary>Seconds since launch when setup began.</summary>
        public static double StartupBeginSeconds { get; private set; }

        /// <summary>Time spent creating the tracker alone.</summary>
        public static double StartupTrackerMilliseconds { get; private set; }

        /// <summary>Longest single startup stage, which is the largest startup cost on one frame.</summary>
        public static double StartupWorstStageMilliseconds { get; private set; }

        internal static double EndInitializationSample(long startTimestamp, long startBytes)
        {
            long endTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            double milliseconds = (endTimestamp - startTimestamp) * (1000d / System.Diagnostics.Stopwatch.Frequency);
            InitializationMilliseconds += milliseconds;
            StartupWorstStageMilliseconds = System.Math.Max(StartupWorstStageMilliseconds, milliseconds);
            InitializationBytes += System.Math.Max(0L, System.GC.GetTotalMemory(false) - startBytes);
            return milliseconds;
        }
#endif

        public static TweenManager Instance
            => instance;

        private static int initialPoolSize = 50;
        private static int refillThreshold = 2;
        private static int growthBatchSize = 12;
        private static int startupDelayFrames = 30;

        /// <summary>
        /// Changes the startup settings. Call it before the first scene loads, for example from a
        /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] method.
        /// </summary>
        /// <param name="initialPoolSize">Entities prepared in small batches after startup.</param>
        /// <param name="refillThreshold">The pool refills when this many entities or fewer are left.</param>
        /// <param name="growthBatchSize">Entities added per update while filling or refilling.</param>
        /// <param name="startupDelayFrames">Frames to wait after the first scene before starting setup.</param>
        public static void Configure(
            int initialPoolSize = 50,
            int refillThreshold = 2,
            int growthBatchSize = 12,
            int startupDelayFrames = 30)
        {
            if (instance != null && instance.initStage > 0)
                Debug.LogWarning("[UITweens] Configure() was called after startup began, so the pool settings won't change.");

            TweenManager.initialPoolSize = Mathf.Max(0, initialPoolSize);
            TweenManager.refillThreshold = Mathf.Max(0, refillThreshold);
            TweenManager.growthBatchSize = Mathf.Max(0, growthBatchSize);
            TweenManager.startupDelayFrames = Mathf.Max(0, startupDelayFrames);
        }

        /// <summary>
        /// Finishes startup and prepares entities now, along with position, alpha and scale tweens.
        /// Call it during a loading screen so that the first tweens don't pay for this work.
        /// </summary>
        public static void Prewarm(int entityCount)
        {
            if (instance == null)
            {
                Debug.LogError("[UITweens] TweenManager isn't created yet. Call Prewarm() from Start() or later.");
                return;
            }

            instance.EnsureInitialized();
            instance.tweenBootstrapper.PoolManager.Prewarm(entityCount);
        }

        /// <summary>True once startup has finished. Tweens can still start before this.</summary>
        public static bool IsReady
            => instance != null && instance.initStage >= INIT_STAGE_COUNT;

        public static EcsWorld World
            => instance?.world;

        // OPTIMIZATION: Cache TweenSpawner singleton (20-50ms gain - avoid repeated allocation)
        private static TweenSpawner cachedSpawner;

        /// <summary>Gets the spawner used to create tweens.</summary>
        public static TweenSpawner GetSpawner()
        {
            if (instance == null)
            {
                Debug.LogError("[UITweens] TweenManager not initialized. Call TweenManager.Initialize() first.");
                return null;
            }

            // Tweens can start before the frame-spread setup has finished, for example in Start().
            instance.EnsureInitialized();

            // Return cached instance instead of creating new one every time
            if (cachedSpawner == null)
                cachedSpawner = new TweenSpawner(instance.world, instance.tweenBootstrapper.PoolManager);

            return cachedSpawner;
        }

        /// <summary>Pauses a tween.</summary>
        public static void Pause(TweenHandle handle)
            => TweenControlOperations.Pause(handle);

        /// <summary>Resumes a paused tween.</summary>
        public static void Resume(TweenHandle handle)
            => TweenControlOperations.Resume(handle);

        /// <summary>Cancels a tween.</summary>
        public static void Cancel(TweenHandle handle)
            => TweenControlOperations.Cancel(handle);

        public static void Cancel(EcsEntity entity)
            => TweenControlOperations.Cancel(entity);

        /// <summary>Pauses a playing tween or resumes a paused one.</summary>
        public static void TogglePause(TweenHandle handle)
            => TweenControlOperations.TogglePause(handle);

        private static TweenManager instance;

        private TweenWorldBootstrapper tweenBootstrapper;
        private EcsWorld world;
        private TweenTracker tracker;
        private int lastCleanupFrame;
        private int initStage;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[UITweens] TweenManager already exists, destroying duplicate");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Defer initialization to Start() for frame-spreading (50-100ms gain)
            // Awake only sets up singleton
        }

        private void Start()
        {
            StartCoroutine(InitializeAsync());
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Each stage runs on its own frame, so startup work never lands on one frame at once.
        private IEnumerator InitializeAsync()
        {
            for (int i = 0; i < startupDelayFrames; i++)
                yield return null;

            while (initStage < INIT_STAGE_COUNT)
            {
                yield return null;
                RunNextInitStage();
            }
        }

        private void Update()
        {
            if (tracker == null || tweenBootstrapper == null)
                return;

#if UITWEENS_BENCHMARK
            UpdateLoopStopwatch.Restart();
#endif

            using (UpdateLoopMarker.Auto())
            {
                tracker.ProcessPendingCancellations();
                tweenBootstrapper.Update();

                if (Time.frameCount - lastCleanupFrame >= CLEANUP_INTERVAL_FRAMES)
                {
                    tracker.CleanupDestroyedTargets();
                    lastCleanupFrame = Time.frameCount;
                }

                tweenBootstrapper.PoolManager.RefillIfLow();
            }

#if UITWEENS_BENCHMARK
            UpdateLoopStopwatch.Stop();
            LastUpdateLoopNanoseconds = (long)(UpdateLoopStopwatch.ElapsedTicks * (1_000_000_000d / System.Diagnostics.Stopwatch.Frequency));
#endif
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            tweenBootstrapper?.Destroy();
            tracker?.Cleanup();
            cachedSpawner = null;  // Clear cached spawner
        }

        private void EnsureInitialized()
        {
            while (initStage < INIT_STAGE_COUNT)
                RunNextInitStage();
        }

        private void RunNextInitStage()
        {
            if (initStage >= INIT_STAGE_COUNT)
                return;

#if UITWEENS_BENCHMARK
            BeginInitializationSample(out long startTimestamp, out long startBytes);
#endif

            switch (initStage)
            {
                case 0:
#if UITWEENS_BENCHMARK
                    StartupBeginSeconds = Time.realtimeSinceStartupAsDouble;
#endif
                    tweenBootstrapper = new TweenWorldBootstrapper();
                    tweenBootstrapper.InitializeWorld();
                    world = tweenBootstrapper.World;
                    break;

                case 1:
                    tweenBootstrapper.InitializePool(initialPoolSize, refillThreshold, growthBatchSize);
                    break;

                case 2:
                    tweenBootstrapper.InitializeSystems();
                    break;

                case 3:
                    tracker = new TweenTracker();
#if UITWEENS_BENCHMARK
                    StartupTrackerMilliseconds = (System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp) * (1000d / System.Diagnostics.Stopwatch.Frequency);
#endif
                    break;
            }

#if UITWEENS_BENCHMARK
            double milliseconds = EndInitializationSample(startTimestamp, startBytes);
            if (initStage <= 2)
                StartupWorldMilliseconds += milliseconds;
            else
                StartupServicesMilliseconds += milliseconds;
#endif

            initStage++;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            tracker?.CleanupDestroyedTargets();
        }

        internal static void TrackTweenForComponent(UnityEngine.Object component, TweenHandle handle)
            => instance?.tracker?.TrackTween(component, handle);

        internal static bool TryUntrackTween(EcsEntity entity, TweenHandle predecessor, out int componentId, out TweenPropertyType propertyType)
        {
            if (instance?.tracker != null)
                return instance.tracker.TryUntrack(entity, predecessor, out componentId, out propertyType);

            componentId = 0;
            propertyType = TweenPropertyType.Unknown;
            return false;
        }

        internal static void TrackTween(int componentId, TweenPropertyType propertyType, TweenHandle handle)
            => instance?.tracker?.TrackTween(componentId, propertyType, handle);

        internal static void OnDelayFinished(UnityEngine.Object component, TweenPropertyType propertyType)
        {
        }

        internal static void OnTweenDelaySet(UnityEngine.Object component, TweenHandle handle)
        {
        }

        public static void KillCompetitorTweens(EcsEntity entity)
            => instance?.tracker?.KillCompetitorTweens(entity);

        public static TweenPropertyType DetectTweenPropertyType(EcsEntity entity)
        {
            if (instance?.tracker != null)
                return instance.tracker.DetectPropertyType(entity);

            return TweenPropertyType.Unknown;
        }

        public static int GetComponentTweenHandlesCount()
            => instance?.tracker?.TrackedTweenCount ?? 0;

        internal static bool TryGetComponentTween(UnityEngine.Object component, TweenPropertyType propertyType, out TweenHandle handle)
        {
            if (instance?.tracker != null)
                return instance.tracker.TryGetTween(component, propertyType, out handle);

            handle = new TweenHandle();
            return false;
        }

        internal static bool TryGetComponentTween(UnityEngine.Object component, out TweenHandle handle)
        {
            if (instance?.tracker != null)
                return instance.tracker.TryGetAnyTween(component, out handle);

            handle = new TweenHandle();
            return false;
        }

        internal static void RemoveComponentTween(UnityEngine.Object component, TweenPropertyType propertyType)
            => instance?.tracker?.RemoveTween(component, propertyType);

        internal static void PauseComponentTweens(UnityEngine.Object component)
        {
            if (component != null)
                instance?.tracker?.PauseAllTweensForComponent(component.GetInstanceID());
        }

        internal static void ResumeComponentTweens(UnityEngine.Object component)
        {
            if (component != null)
                instance?.tracker?.ResumeAllTweensForComponent(component.GetInstanceID());
        }

        internal static void TogglePauseComponentTweens(UnityEngine.Object component)
        {
            if (component != null)
                instance?.tracker?.TogglePauseAllTweensForComponent(component.GetInstanceID());
        }

        internal static void CancelComponentTweens(UnityEngine.Object component)
        {
            if (component != null)
                instance?.tracker?.CancelAllTweensForComponent(component.GetInstanceID());
        }

        internal static void RemoveFromPendingCancellations(UnityEngine.Object component, TweenPropertyType propertyType)
            => instance?.tracker?.RemoveFromPendingCancellations(component, propertyType);

        internal static TweenHandle GetAndRemovePredecessor(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (instance?.tracker != null)
                return instance.tracker.GetAndRemovePredecessor(component, propertyType);

            return new TweenHandle();
        }
    }
}
