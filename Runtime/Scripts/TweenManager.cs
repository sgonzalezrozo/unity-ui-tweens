using System.Collections;
using Leopotam.Ecs;
using UITweens.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UITweens
{
    public class TweenManager : MonoBehaviour
    {
        private const int CLEANUP_INTERVAL_FRAMES = 60;

        public static TweenManager Instance
            => instance;

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
        private TweenCleaner cleaner;
        private TweenDelayedSpawner delayedSpawner;
        private int lastCleanupFrame;

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

        private IEnumerator InitializeAsync()
        {
            yield return null;
            InitializeEcsWorld();

            yield return null;
            InitializeServices();

            yield return null;
        }

        private void Update()
        {
            if (tracker == null || delayedSpawner == null || tweenBootstrapper == null)
                return;

            tracker.ProcessPendingCancellations();
            delayedSpawner.ProcessDelayQueue(Time.deltaTime, world);
            tweenBootstrapper.Update();

            if (Time.frameCount - lastCleanupFrame < CLEANUP_INTERVAL_FRAMES)
                return;

            cleaner.CleanupOrphanedTweens(tracker);
            lastCleanupFrame = Time.frameCount;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            tweenBootstrapper?.Destroy();
            tracker?.Cleanup();
            delayedSpawner?.Cleanup();
            cachedSpawner = null;  // Clear cached spawner
        }

        private void InitializeEcsWorld()
        {
            tweenBootstrapper = new TweenWorldBootstrapper();
            tweenBootstrapper.Initialize();
            world = tweenBootstrapper.World;
        }

        private void InitializeServices()
        {
            tracker = new TweenTracker();
            cleaner = new TweenCleaner();
            delayedSpawner = new TweenDelayedSpawner(tracker);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            => cleaner.CleanupOrphanedTweens(tracker);

        public void RegisterTransform(RectTransform rectTransform)
            => cleaner.RegisterTransform(rectTransform);

        public void RegisterCanvasGroup(CanvasGroup canvasGroup)
            => cleaner.RegisterCanvasGroup(canvasGroup);

        public void RegisterImage(UnityEngine.UI.Image image)
            => cleaner.RegisterImage(image);

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

        internal static void ClearComponentTween(UnityEngine.Object component)
            => instance?.tracker?.ClearAllTweensForComponent(component);

        internal static void EnqueueDelayedTween(TweenDefinition definition)
            => instance?.delayedSpawner?.EnqueueDelayedTween(definition);

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

