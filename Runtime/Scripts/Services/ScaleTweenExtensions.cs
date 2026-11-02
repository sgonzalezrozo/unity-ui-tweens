using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Scale animation extensions for TweenSpawner.
    /// Single Responsibility: Scale tweens (X, Y, Z, and combined) only.
    /// </summary>
    public static class ScaleTweenExtensions
    {
        /// <summary>
        /// Tween from current scale to target.
        /// </summary>
        public static TweenBuilder Scale(
            this TweenSpawner spawner,
            RectTransform element,
            Vector3 targetScale,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector3 startScale = element.localScale;
            return spawner.ScaleFromTo(element, startScale, targetScale, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween from start to end scale.
        /// </summary>
        public static TweenBuilder ScaleFromTo(
            this TweenSpawner spawner,
            RectTransform element,
            Vector3 startScale,
            Vector3 endScale,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(element, duration, ease, easeParameter);
            spawner.InitializeScaleInternal(handle, startScale, endScale);
            return new TweenBuilder(spawner).SetHandle(handle);
        }

        /// <summary>
        /// Tween ONLY X scale (keep Y and Z unchanged).
        /// </summary>
        public static TweenBuilder ScaleX(
            this TweenSpawner spawner,
            RectTransform element,
            float targetScaleX,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector3 currentScale = element.localScale;
            Vector3 targetScale = new Vector3(targetScaleX, currentScale.y, currentScale.z);
            return spawner.ScaleFromTo(element, currentScale, targetScale, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween ONLY Y scale (keep X and Z unchanged).
        /// </summary>
        public static TweenBuilder ScaleY(
            this TweenSpawner spawner,
            RectTransform element,
            float targetScaleY,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector3 currentScale = element.localScale;
            Vector3 targetScale = new Vector3(currentScale.x, targetScaleY, currentScale.z);
            return spawner.ScaleFromTo(element, currentScale, targetScale, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween ONLY Z scale (keep X and Y unchanged).
        /// </summary>
        public static TweenBuilder ScaleZ(
            this TweenSpawner spawner,
            RectTransform element,
            float targetScaleZ,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector3 currentScale = element.localScale;
            Vector3 targetScale = new Vector3(currentScale.x, currentScale.y, targetScaleZ);
            return spawner.ScaleFromTo(element, currentScale, targetScale, duration, ease, easeParameter);
        }
    }
}

