using UITweens.Math;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Extensions
{
    public static class CanvasGroupTweenExtensions
    {
        /// <summary>Animates the alpha from its current value.</summary>
        public static TweenBuilder DOFade(
            this CanvasGroup canvasGroup,
            float targetAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("[UITweens] CanvasGroup is null in DOFade()");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Alpha(canvasGroup, targetAlpha, duration, ease, easeParameter);

            TweenHandle handle = builder.GetHandle();
            TweenManager.TrackTweenForComponent(canvasGroup, handle);
            builder.SetTrackedComponent(canvasGroup);
            builder.SetTrackedComponent(canvasGroup);

            return builder;
        }

        /// <summary>Animates the alpha between two values.</summary>
        public static TweenBuilder DOFadeFromTo(
            this CanvasGroup canvasGroup,
            float startAlpha,
            float endAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("[UITweens] CanvasGroup is null in DOFadeFromTo()");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .AlphaFromTo(canvasGroup, startAlpha, endAlpha, duration, ease, easeParameter);

            TweenHandle handle = builder.GetHandle();
            TweenManager.TrackTweenForComponent(canvasGroup, handle);
            builder.SetTrackedComponent(canvasGroup);
            builder.SetTrackedComponent(canvasGroup);

            return builder;
        }
    }
}
