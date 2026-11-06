using Leopotam.Ecs;
using UITweens.Services;
using UITweens.Systems;
using UnityEngine;

namespace UITweens
{
    public class TweenWorldBootstrapper
    {
        private EcsWorld world;
        private DelayedTweenCompletionSystem delayedCompletionSystem;
        private TweenMathSystem mathSystem;
        private UGUITweenApplySystem applySystem;
        private TweenPoolManager poolManager;
        private UITweenPoolConfig poolConfig;

        public EcsWorld World => world;
        public TweenPoolManager PoolManager => poolManager;

#if UITWEENS_BENCHMARK
        public static double EcsWorldMilliseconds { get; private set; }
        public static double ConfigLoadMilliseconds { get; private set; }
        public static double SystemsMilliseconds { get; private set; }

        private static double ElapsedMilliseconds(long startTimestamp)
            => (System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp) * (1000d / System.Diagnostics.Stopwatch.Frequency);
#endif

        public void InitializeWorld()
        {
#if UITWEENS_BENCHMARK
            long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            // Smaller than the LeoECS defaults. Everything here grows when it needs to.
            world = new EcsWorld(new EcsWorldConfig
            {
                WorldEntitiesCacheSize = 64,
                WorldFiltersCacheSize = 16,
                WorldComponentPoolsCacheSize = 32,
                FilterEntitiesCacheSize = 32
            });

#if UITWEENS_BENCHMARK
            EcsWorldMilliseconds = ElapsedMilliseconds(startTimestamp);
            startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            poolConfig = Resources.Load<UITweenPoolConfig>("UITweenPoolConfig");
            if (poolConfig == null)
                poolConfig = ScriptableObject.CreateInstance<UITweenPoolConfig>();

#if UITWEENS_BENCHMARK
            ConfigLoadMilliseconds = ElapsedMilliseconds(startTimestamp);
#endif
        }

        public void InitializePool()
        {
            poolManager = new TweenPoolManager(
                world,
                poolConfig.InitialPoolSize,
                poolConfig.RefillThreshold,
                poolConfig.GrowthBatchSize);
        }

        public void InitializeSystems()
        {
#if UITWEENS_BENCHMARK
            long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            // Run directly, because EcsSystems finds its fields through reflection at startup.
            delayedCompletionSystem = new DelayedTweenCompletionSystem(poolManager.Filters);
            mathSystem = new TweenMathSystem(poolManager.Filters);
            applySystem = new UGUITweenApplySystem(poolManager);

#if UITWEENS_BENCHMARK
            SystemsMilliseconds = ElapsedMilliseconds(startTimestamp);
#endif

            if (poolConfig.LogPoolAllocations)
                Debug.Log($"[UITweens] World initialized. Pool size: {poolConfig.InitialPoolSize} entities, refill {poolConfig.GrowthBatchSize} at {poolConfig.RefillThreshold}");
        }

        public void Update()
        {
            if (applySystem == null)
                return;

            delayedCompletionSystem.Run();
            mathSystem.Run();
            applySystem.Run();

#if DEBUG
            // Debug builds of LeoECS list every new entity until this runs. EcsSystems used to call it.
            world.CheckForLeakedEntities(null);
#endif
        }

        public void Destroy()
        {
            delayedCompletionSystem = null;
            mathSystem = null;
            applySystem = null;

            if (world != null)
            {
                world.Destroy();
                world = null;
            }
        }
    }
}

