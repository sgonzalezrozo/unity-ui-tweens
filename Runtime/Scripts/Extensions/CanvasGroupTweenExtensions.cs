using Tools.UITweens.Runtime.Scripts.Math;
using Tools.UITweens.Runtime.Scripts.Services;
using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Extensions
{
    public static class CanvasGroupTweenExtensions
    {
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
