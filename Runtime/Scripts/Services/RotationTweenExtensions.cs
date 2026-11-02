using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Rotation animation extensions for TweenSpawner.
    /// Single Responsibility: Z-axis rotation tweens only.
    /// </summary>
    public static class RotationTweenExtensions
    {
        /// <summary>
        /// Tween rotation (z-axis) from current to target.
        /// </summary>
        public static TweenBuilder RotationZ(
            this TweenSpawner spawner,
            RectTransform element,
            float targetRotation,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            float startRotation = element.eulerAngles.z;
            return spawner.RotationZFromTo(element, startRotation, targetRotation, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween rotation (z-axis) from start to end.
        /// </summary>
        public static TweenBuilder RotationZFromTo(
            this TweenSpawner spawner,
            RectTransform element,
            float startRotation,
            float endRotation,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(element, duration, ease, easeParameter);
            spawner.InitializeRotationInternal(handle, startRotation, endRotation);
            return new TweenBuilder(spawner).SetHandle(handle);
        }
    }
}

