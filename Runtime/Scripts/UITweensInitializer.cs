using UnityEngine;

namespace UITweens
{
    public class UITweensInitializer
    {
        private const string TWEEN_MANAGER_RESOURCE_PATH = "Tools/UITweens/TweenManager";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeTweenManager()
        {
#if UITWEENS_BENCHMARK
            TweenManager.BeginInitializationSample(out long startTimestamp, out long startBytes);
#endif

            GameObject prefab = Resources.Load<GameObject>(TWEEN_MANAGER_RESOURCE_PATH);

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[UITweens] TweenManager prefab not found at Resources/{TWEEN_MANAGER_RESOURCE_PATH}. " +
                    "Please run: Tools > UI Tweens > Setup");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = "TweenManager";

#if UITWEENS_BENCHMARK
            TweenManager.StartupPrefabMilliseconds = TweenManager.EndInitializationSample(startTimestamp, startBytes);
#endif
        }
    }
}
