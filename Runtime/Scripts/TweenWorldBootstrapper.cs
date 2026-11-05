using Leopotam.Ecs;
using UITweens.Services;
using UITweens.Systems;
using UnityEngine;

namespace UITweens
{
    public class TweenWorldBootstrapper
    {
        private EcsWorld world;
        private EcsSystems systems;
        private TweenPoolManager poolManager;

        public EcsWorld World => world;
        public TweenPoolManager PoolManager => poolManager;

#if UITWEENS_BENCHMARK
        public static double EcsWorldMilliseconds { get; private set; }
        public static double ConfigLoadMilliseconds { get; private set; }
        public static double SystemsMilliseconds { get; private set; }

        private static double ElapsedMilliseconds(long startTimestamp)
            => (System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp) * (1000d / System.Diagnostics.Stopwatch.Frequency);
#endif

        public void Initialize()
        {
#if UITWEENS_BENCHMARK
            long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            world = new EcsWorld();

#if UITWEENS_BENCHMARK
            EcsWorldMilliseconds = ElapsedMilliseconds(startTimestamp);
            startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            UITweenPoolConfig poolConfig = Resources.Load<UITweenPoolConfig>("UITweenPoolConfig");
            if (poolConfig == null)
                poolConfig = ScriptableObject.CreateInstance<UITweenPoolConfig>();

#if UITWEENS_BENCHMARK
            ConfigLoadMilliseconds = ElapsedMilliseconds(startTimestamp);
#endif

            poolManager = new TweenPoolManager(
                world,
                poolConfig.InitialPoolSize,
                poolConfig.RefillThreshold,
                poolConfig.GrowthBatchSize);

#if UITWEENS_BENCHMARK
            startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
#endif

            systems = new EcsSystems(world);
            systems
                .Add(new DelayedTweenCompletionSystem(world))
                .Add(new TweenMathSystem(world))
                .Add(new UGUITweenApplySystem(poolManager))
                .Init();

#if UITWEENS_BENCHMARK
            SystemsMilliseconds = ElapsedMilliseconds(startTimestamp);
#endif

            if (poolConfig.LogPoolAllocations)
                Debug.Log($"[UITweens] World initialized. Pool size: {poolConfig.InitialPoolSize} entities, refill {poolConfig.GrowthBatchSize} at {poolConfig.RefillThreshold}");
        }

        public void Update()
        {
            systems?.Run();
        }

        public void Destroy()
        {
            if (systems != null)
            {
                systems.Destroy();
                systems = null;
            }

            if (world != null)
            {
                world.Destroy();
                world = null;
            }
        }
    }
}

