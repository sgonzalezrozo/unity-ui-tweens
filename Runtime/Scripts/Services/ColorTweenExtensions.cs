using Tools.UITweens.Runtime.Scripts.Math;
using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Services
{
    /// <summary>
    /// Color animation extensions for TweenSpawner.
    /// Single Responsibility: Color tweens for Image components only.
    /// </summary>
    public static class ColorTweenExtensions
    {
        /// <summary>
        /// Tween from current color to target.
        /// </summary>
        public static TweenBuilder Color(
            this TweenSpawner spawner,
            UnityEngine.UI.Image target,
            UnityEngine.Color targetColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Image target is null");
                return new TweenBuilder(spawner);
            }

            UnityEngine.Color startColor = target.color;
            return spawner.ColorFromTo(target, startColor, targetColor, duration, ease, easeParameter);
        }

        /// <summary>
        /// Tween from start to end color.
        /// </summary>
        public static TweenBuilder ColorFromTo(
            this TweenSpawner spawner,
            UnityEngine.UI.Image target,
            UnityEngine.Color startColor,
            UnityEngine.Color endColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Image target is null");
                return new TweenBuilder(spawner);
            }

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("[UITweens] Image has no RectTransform");
                return new TweenBuilder(spawner);
            }

            TweenHandle handle = spawner.SpawnTweenInternal(rect, duration, ease, easeParameter, image: target);
            spawner.InitializeColorInternal(handle, startColor, endColor);
            return new TweenBuilder(spawner).SetHandle(handle);
        }
    }
}

