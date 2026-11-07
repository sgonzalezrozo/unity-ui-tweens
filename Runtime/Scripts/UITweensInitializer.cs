using UnityEngine;

namespace UITweens
{
    public class UITweensInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeTweenManager()
        {
            if (!TweenManager.AutoStartEnabled)
                return;

#if UITWEENS_BENCHMARK
            TweenManager.BeginInitializationSample(out long startTimestamp, out long startBytes);
#endif

            // Created in code, because loading a prefab from Resources costs more at startup.
            new GameObject("TweenManager", typeof(TweenManager));

#if UITWEENS_BENCHMARK
            TweenManager.StartupPrefabMilliseconds = TweenManager.EndInitializationSample(startTimestamp, startBytes);
#endif
        }
    }
}
