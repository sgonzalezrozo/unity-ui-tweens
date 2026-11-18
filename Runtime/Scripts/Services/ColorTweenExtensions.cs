using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Color animation extensions for TweenSpawner.
    /// Single Responsibility: Color tweens for Graphic components (Image, RawImage, Text, TextMeshPro).
    /// </summary>
    public static class ColorTweenExtensions
    {
        /// <summary>
        /// Tween from current color to target.
        /// </summary>
        public static TweenBuilder Color(
            this TweenSpawner spawner,
            UnityEngine.UI.Graphic target,
            UnityEngine.Color targetColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Graphic target is null");
                return new TweenBuilder(spawner);
            }

            UnityEngine.Color startColor = target.color;
            return spawner.ColorFromTo(target, startColor, targetColor, duration, ease, easeParameter)
                .StartFromCurrent();
        }

        /// <summary>
        /// Tween from start to end color.
        /// </summary>
        public static TweenBuilder ColorFromTo(
            this TweenSpawner spawner,
            UnityEngine.UI.Graphic target,
            UnityEngine.Color startColor,
            UnityEngine.Color endColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Graphic target is null");
                return new TweenBuilder(spawner);
            }

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("[UITweens] Graphic has no RectTransform");
                return new TweenBuilder(spawner);
            }

            TweenHandle handle = spawner.SpawnTweenInternal(rect, duration, ease, easeParameter, graphic: target, kind: TweenPoolKind.Color);
            spawner.InitializeColorInternal(handle, startColor, endColor);
            return new TweenBuilder(spawner).SetHandle(handle);
        }
    }
}

