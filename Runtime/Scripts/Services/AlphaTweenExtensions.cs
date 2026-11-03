using UITweens.Components;
using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Alpha animation extensions for TweenSpawner.
    /// Single Responsibility: Alpha tweens for CanvasGroup and Image only.
    /// </summary>
    public static class AlphaTweenExtensions
    {
        /// <summary>
        /// Tween from current alpha to target (CanvasGroup).
        /// </summary>
        public static TweenBuilder Alpha(
            this TweenSpawner spawner,
            CanvasGroup canvasGroup,
            float targetAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (canvasGroup == null)
            {
                Debug.LogError("[UITweens] CanvasGroup is null");
                return new TweenBuilder(spawner);
            }

            float startAlpha = canvasGroup.alpha;
            return spawner.AlphaFromTo(canvasGroup, startAlpha, targetAlpha, duration, ease, easeParameter)
                .StartFromCurrent();
        }

        /// <summary>
        /// Tween from start to end alpha (CanvasGroup).
        /// </summary>
        public static TweenBuilder AlphaFromTo(
            this TweenSpawner spawner,
            CanvasGroup canvasGroup,
            float startAlpha,
            float endAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (canvasGroup == null)
            {
                Debug.LogError("[UITweens] CanvasGroup is null");
                return new TweenBuilder(spawner);
            }

            RectTransform rectTransform = canvasGroup.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("[UITweens] CanvasGroup has no RectTransform");
                return new TweenBuilder(spawner);
            }

            TweenHandle handle = spawner.SpawnTweenInternal(rectTransform, duration, ease, easeParameter, canvasGroup);
            spawner.InitializeAlphaInternal(handle, startAlpha, endAlpha);
            return new TweenBuilder(spawner).SetHandle(handle);
        }

        /// <summary>
        /// Tween Image alpha/opacity only (keep RGB unchanged).
        /// </summary>
        public static TweenBuilder Alpha(
            this TweenSpawner spawner,
            UnityEngine.UI.Image target,
            float targetAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Image target is null");
                return new TweenBuilder(spawner);
            }

            UnityEngine.Color currentColor = target.color;
            UnityEngine.Color targetColor = new UnityEngine.Color(
                currentColor.r,
                currentColor.g,
                currentColor.b,
                targetAlpha);

            return spawner.ColorFromTo(target, currentColor, targetColor, duration, ease, easeParameter)
                .StartFromCurrent(TweenClockComponent.ALPHA_ONLY);
        }
    }
}

