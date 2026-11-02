using Tools.UITweens.Runtime.Scripts.Math;
using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Services
{
    /// <summary>
    /// Size and Pivot animation extensions for TweenSpawner.
    /// Single Responsibility: Size delta and pivot tweens only.
    /// </summary>
    public static class SizePivotTweenExtensions
    {
        /// <summary>
        /// Tween from current size delta to target.
        /// </summary>
        public static TweenBuilder Size(
            this TweenSpawner spawner,
            RectTransform element,
            Vector2 targetSize,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector2 startSize = element.sizeDelta;
            return spawner.SizeFromTo(element, startSize, targetSize, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween from start to end size delta.
        /// </summary>
        public static TweenBuilder SizeFromTo(
            this TweenSpawner spawner,
            RectTransform element,
            Vector2 startSize,
            Vector2 endSize,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(element, duration, ease, easeParameter);
            spawner.InitializeSizeDeltaInternal(handle, startSize, endSize);
            return new TweenBuilder(spawner).SetHandle(handle);
        }

        /// <summary>
        /// Tween from current pivot to target.
        /// </summary>
        public static TweenBuilder Pivot(
            this TweenSpawner spawner,
            RectTransform element,
            Vector2 targetPivot,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector2 startPivot = element.pivot;
            return spawner.PivotFromTo(element, startPivot, targetPivot, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween from start to end pivot.
        /// </summary>
        public static TweenBuilder PivotFromTo(
            this TweenSpawner spawner,
            RectTransform element,
            Vector2 startPivot,
            Vector2 endPivot,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(element, duration, ease, easeParameter);
            spawner.InitializePivotInternal(handle, startPivot, endPivot);
            return new TweenBuilder(spawner).SetHandle(handle);
        }
    }
}

