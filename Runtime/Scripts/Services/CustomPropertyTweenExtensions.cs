using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Custom property animation extensions for TweenSpawner.
    /// Single Responsibility: Custom float and vector property tweens only.
    /// Zero-allocation: Uses singleton dummy RectTransform, never creates GameObjects.
    /// </summary>
    public static class CustomPropertyTweenExtensions
    {
        /// <summary>
        /// Singleton dummy RectTransform for custom property tweens.
        /// Created once, reused for all custom tweens. Avoids memory allocation per tween.
        /// </summary>
        private static RectTransform dummyRectTransform;

        static CustomPropertyTweenExtensions()
        {
            GameObject dummy = new GameObject("[UITween_CustomPropertyDummy]");
            dummy.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            dummyRectTransform = dummy.AddComponent<RectTransform>();
            UnityEngine.Object.DontDestroyOnLoad(dummy);
        }

        /// <summary>
        /// Animate a custom float property. Zero-allocation, reuses singleton dummy.
        /// </summary>
        public static TweenBuilder CustomFloat(
            this TweenSpawner spawner,
            float startValue,
            float endValue,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(dummyRectTransform, duration, ease, easeParameter);
            spawner.InitializeCustomFloatInternal(handle, startValue, endValue);
            return new TweenBuilder(spawner).SetHandle(handle);
        }

        /// <summary>
        /// Animate a custom vector property. Zero-allocation, reuses singleton dummy.
        /// </summary>
        public static TweenBuilder CustomVector(
            this TweenSpawner spawner,
            Vector3 startValue,
            Vector3 endValue,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(dummyRectTransform, duration, ease, easeParameter);
            spawner.InitializeCustomVectorInternal(handle, startValue, endValue);
            return new TweenBuilder(spawner).SetHandle(handle);
        }
    }
}

