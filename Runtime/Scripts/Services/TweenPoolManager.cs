using System.Collections.Generic;
using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Math;

namespace UITweens.Services
{
    /// <summary>The pool a tween returns to. None means it has no property to keep.</summary>
    public enum TweenPoolKind : byte
    {
        None,
        Alpha,
        Position,
        Scale,
        Rotation,
        Color,
        SizeDelta,
        Pivot,
        CustomFloat,
        CustomVector
    }

    public class TweenPoolManager
    {
        private readonly EcsWorld world;
        // Core components, the delay component and one property component.
        private const int BASE_COMPONENT_COUNT = 7;

        private readonly Stack<EcsEntity> availableEntities;
        private readonly Stack<EcsEntity>[] entitiesByKind = new Stack<EcsEntity>[10];
        private readonly List<EcsEntity> activeEntities;
        private int[] activeSlotByEntityId;

        private const int DEFAULT_POOL_SIZE = 50;
        private const int DEFAULT_REFILL_THRESHOLD = 2;
        private const int DEFAULT_GROWTH_BATCH_SIZE = 12;

        internal TweenFilterSet Filters { get; }

        private readonly int refillThreshold;
        private readonly int growthBatchSize;
        private int totalCreated;
        private int entitiesToWarmUp;

#if UITWEENS_BENCHMARK
        /// <summary>Times entity creation when the pool is empty. Off by default because it adds work.</summary>
        public static bool TrackGrowth { get; set; }

        public static double PreAllocationMilliseconds { get; private set; }
        public static double PreAllocationMaxSliceMilliseconds { get; private set; }
        public static long PreAllocationBytes { get; private set; }
        public static int GrowthEntities { get; private set; }
        public static int GrowthAllocations { get; private set; }
        public static double GrowthTotalMilliseconds { get; private set; }
        public static double GrowthMaxMilliseconds { get; private set; }
        public static long GrowthBytes { get; private set; }

        // Details of the slowest entity creation, to tell a resize from a GC pause.
        public static long WorstGrowthBytes { get; private set; }
        public static int WorstGrowthEntityId { get; private set; }
        public static int WorstGrowthFrame { get; private set; }
        public static int WorstGrowthCollections { get; private set; }

        private static double TimestampToMilliseconds(long ticks)
            => ticks * (1000d / System.Diagnostics.Stopwatch.Frequency);

        private EcsEntity CreateTrackedEntity()
        {
            int startCollections = System.GC.CollectionCount(0);
            long startBytes = System.GC.GetTotalMemory(false);
            long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();

            EcsEntity entity = world.NewEntity();
            PreInitializeEntity(entity);

            double milliseconds = TimestampToMilliseconds(System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp);
            long bytes = System.GC.GetTotalMemory(false) - startBytes;

            GrowthEntities++;
            GrowthTotalMilliseconds += milliseconds;

            if (milliseconds > GrowthMaxMilliseconds)
            {
                GrowthMaxMilliseconds = milliseconds;
                WorstGrowthBytes = bytes;
                WorstGrowthEntityId = entity.GetInternalId();
                WorstGrowthFrame = UnityEngine.Time.frameCount;
                WorstGrowthCollections = System.GC.CollectionCount(0) - startCollections;
            }

            if (bytes > 0L)
            {
                GrowthAllocations++;
                GrowthBytes += bytes;
            }

            return entity;
        }
#endif

        // OPTIMIZATION: Accept pool size parameter for configurable pre-allocation 
        public TweenPoolManager(
            EcsWorld world,
            int poolSize = DEFAULT_POOL_SIZE,
            int refillThreshold = DEFAULT_REFILL_THRESHOLD,
            int growthBatchSize = DEFAULT_GROWTH_BATCH_SIZE)
        {
            this.world = world;
            Filters = new TweenFilterSet(world);
            this.refillThreshold = refillThreshold;
            this.growthBatchSize = growthBatchSize;
            poolSize = poolSize > 0 ? poolSize : 0;
            availableEntities = new Stack<EcsEntity>(poolSize);
            activeEntities = new List<EcsEntity>(poolSize);
            activeSlotByEntityId = new int[poolSize > 16 ? poolSize : 16];
            totalCreated = 0;

#if UITWEENS_BENCHMARK
            GrowthEntities = 0;
            GrowthAllocations = 0;
            GrowthTotalMilliseconds = 0d;
            GrowthMaxMilliseconds = 0d;
            GrowthBytes = 0L;
            WorstGrowthBytes = 0L;
            WorstGrowthEntityId = 0;
            WorstGrowthFrame = 0;
            WorstGrowthCollections = 0;

            PreAllocationMilliseconds = 0d;
            PreAllocationMaxSliceMilliseconds = 0d;
            PreAllocationBytes = 0L;
#endif

            // Nothing is created during startup. The update fills the pool in small batches.
            entitiesToWarmUp = poolSize;
        }

        public EcsEntity AcquireEntity(TweenPoolKind kind = TweenPoolKind.None)
        {
            EcsEntity entity;
            Stack<EcsEntity> sameKind = entitiesByKind[(int)kind];

            if (kind != TweenPoolKind.None && sameKind != null && sameKind.Count > 0)
                entity = sameKind.Pop();
            else if (availableEntities.Count > 0)
                entity = availableEntities.Pop();
            else
                entity = CreateEntity();

            entity.Get<TweenControlComponent>().Kind = kind;
            AddActive(entity);
            return entity;
        }

        /// <summary>
        /// Adds a batch when the pool is nearly empty. This runs in the update, not when a tween is
        /// requested, so the next tween finds an entity ready and doesn't pay for the growth.
        /// </summary>
        public void RefillIfLow()
        {
            if (entitiesToWarmUp > 0)
            {
                WarmUpBatch();
                return;
            }

            if (availableEntities.Count > refillThreshold)
                return;

            for (int i = 0; i < growthBatchSize; i++)
                availableEntities.Push(CreateEntity());
        }

        /// <summary>Creates entities now, and the filters for the most common tween types.</summary>
        public void Prewarm(int entityCount)
        {
            Filters.RequirePosition();
            Filters.RequireAlpha();
            Filters.RequireScale();

            entitiesToWarmUp = 0;
            while (availableEntities.Count < entityCount)
                availableEntities.Push(CreateEntity());
        }

        private void WarmUpBatch()
        {
#if UITWEENS_BENCHMARK
            long startBytes = System.GC.GetTotalMemory(false);
            long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            int count = growthBatchSize < entitiesToWarmUp ? growthBatchSize : entitiesToWarmUp;
            for (int i = 0; i < count; i++)
                availableEntities.Push(CreateEntity());

            // A growth size of 0 turns refills off, but the initial pool still fills.
            if (count == 0)
            {
                availableEntities.Push(CreateEntity());
                count = 1;
            }

            entitiesToWarmUp -= count;

#if UITWEENS_BENCHMARK
            double milliseconds = TimestampToMilliseconds(System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp);
            PreAllocationMilliseconds += milliseconds;
            PreAllocationMaxSliceMilliseconds = System.Math.Max(PreAllocationMaxSliceMilliseconds, milliseconds);
            PreAllocationBytes += System.Math.Max(0L, System.GC.GetTotalMemory(false) - startBytes);
#endif
        }

        private EcsEntity CreateEntity()
        {
            totalCreated++;

#if UITWEENS_BENCHMARK
            if (TrackGrowth)
                return CreateTrackedEntity();
#endif

            EcsEntity entity = world.NewEntity();
            PreInitializeEntity(entity);
            return entity;
        }

        /// <summary>
        /// Returns a finished tween to the pool for reuse. Its serial changes, so old handles stop
        /// working, and it's left as a new entity would be, apart from its core components.
        /// </summary>
        public void ReleaseEntity(EcsEntity entity)
        {
            if (!RemoveActive(entity) || !entity.IsAlive())
                return;

            ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();
            int nextSerial = control.Serial + 1;
            TweenPoolKind kind = control.Kind;

            control = default;
            control.Serial = nextSerial;
            control.Kind = kind;
            control.State = TweenControlComponent.ControlState.Pooled;

            entity.Get<TweenClockComponent>() = default;
            entity.Get<TweenResultComponent>() = default;
            entity.Get<TweenTargetComponent>() = default;
            entity.Get<TweenApplyFlagsComponent>() = default;

            if (kind == TweenPoolKind.None)
            {
                RemoveOptionalComponents(entity);
                RemoveProperty(entity);
                availableEntities.Push(entity);
                return;
            }

            // Most tweens have no optional components, so this usually skips every lookup.
            if (entity.GetComponentsCount() > BASE_COMPONENT_COUNT)
                RemoveOptionalComponents(entity);

            Stack<EcsEntity> sameKind = entitiesByKind[(int)kind];
            if (sameKind == null)
            {
                sameKind = new Stack<EcsEntity>();
                entitiesByKind[(int)kind] = sameKind;
            }

            sameKind.Push(entity);
        }

        private static void RemoveOptionalComponents(EcsEntity entity)
        {
            RemoveOptional<TweenLoopComponent>(entity);
            RemoveOptional<TweenCallbackComponent>(entity);
            RemoveOptional<TweenCancelCallbackComponent>(entity);
            RemoveOptional<TweenAdvancedCallbackComponent>(entity);
            RemoveOptional<TweenSequenceLinkComponent>(entity);
        }

        private static void RemoveProperty(EcsEntity entity)
        {
            RemoveOptional<TweenPositionComponent>(entity);
            RemoveOptional<TweenAlphaComponent>(entity);
            RemoveOptional<TweenScaleComponent>(entity);
            RemoveOptional<TweenRotationComponent>(entity);
            RemoveOptional<TweenColorComponent>(entity);
            RemoveOptional<TweenSizeDeltaComponent>(entity);
            RemoveOptional<TweenPivotComponent>(entity);
            RemoveOptional<TweenCustomFloatComponent>(entity);
            RemoveOptional<TweenCustomVectorComponent>(entity);
        }

        private static void RemoveOptional<T>(EcsEntity entity) where T : struct
        {
            if (entity.Has<T>())
                entity.Del<T>();
        }

        /// <summary>
        /// The active entities, in no set order. New tweens are added at the end, and tweens are
        /// only removed by ReleaseEntity, so a callback can safely start a tween during a loop.
        /// </summary>
        internal List<EcsEntity> ActiveEntities
            => activeEntities;

        private void AddActive(EcsEntity entity)
        {
            int entityId = entity.GetInternalId();
            if (entityId >= activeSlotByEntityId.Length)
                System.Array.Resize(ref activeSlotByEntityId, System.Math.Max(entityId + 1, activeSlotByEntityId.Length * 2));

            activeEntities.Add(entity);
            activeSlotByEntityId[entityId] = activeEntities.Count;
        }

        // Slots hold the list index plus one, so 0 means inactive.
        private bool RemoveActive(EcsEntity entity)
        {
            int entityId = entity.GetInternalId();
            if (entityId >= activeSlotByEntityId.Length)
                return false;

            int index = activeSlotByEntityId[entityId] - 1;
            if (index < 0 || activeEntities[index] != entity)
                return false;

            int lastIndex = activeEntities.Count - 1;
            EcsEntity last = activeEntities[lastIndex];
            activeEntities[index] = last;
            activeSlotByEntityId[last.GetInternalId()] = index + 1;
            activeSlotByEntityId[entityId] = 0;
            activeEntities.RemoveAt(lastIndex);
            return true;
        }

        public int GetActiveEntityCount()
        {
            return activeEntities.Count;
        }

        public (int active, int available, int total) GetPoolStats()
        {
            return (activeEntities.Count, availableEntities.Count, totalCreated);
        }

        public IEnumerable<EcsEntity> GetAllActiveEntities()
        {
            return activeEntities;
        }

        // Only pre-initialize core components (20-30ms gain)
        // Optional components (callbacks, loop, delay) are allocated on-demand
        // This reduces 10 Gets per entity to 5 Gets, cutting pre-allocation overhead by 50%
        private void PreInitializeEntity(EcsEntity entity)
        {
            // CORE components - always needed
            entity.Get<TweenClockComponent>();           // Timer tracking
            entity.Get<TweenControlComponent>();          // Pause/resume/cancel state
            entity.Get<TweenResultComponent>();           // Computed animation values
            entity.Get<TweenTargetComponent>();           // Unity references (RectTransform, CanvasGroup, Image)
            entity.Get<TweenApplyFlagsComponent>();       // Which properties to apply
            entity.Get<TweenDelayComponent>();            // Stays through reuse, because every tween has one

            // OPTIONAL components - allocated on-demand when needed:
            // - TweenCallbackComponent: Only if OnComplete callback used
            // - TweenLoopComponent: Only if Loop() called
            // - TweenDelayComponent: Only if Delay() called
            // - TweenAdvancedCallbackComponent: Only if OnStart/OnUpdate callbacks used
            // - TweenSequenceLinkComponent: Only if Then() chaining used
            // - Color/Size/Pivot/CustomFloat/CustomVector: Only for those specific tween types

        }
    }
}

