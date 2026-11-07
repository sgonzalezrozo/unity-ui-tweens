using UITweens.Math;
using UITweens.Services;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Extensions
{
    public static class ImageTweenExtensions
    {
        /// <summary>Animates the color from its current value.</summary>
        public static TweenBuilder DOColor(
            this Image image,
            Color targetColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] DOColor() was called with a null Image — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Color(image, targetColor, duration, ease, easeParameter);

            TweenHandle handle = builder.GetHandle();
            TweenManager.TrackTweenForComponent(image, handle);
            builder.SetTrackedComponent(image);
            builder.SetTrackedComponent(image);

            return builder;
        }

        /// <summary>Animates the color between two values.</summary>
        public static TweenBuilder DOColorFromTo(
            this Image image,
            Color startColor,
            Color endColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] DOColorFromTo() was called with a null Image — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .ColorFromTo(image, startColor, endColor, duration, ease, easeParameter);

            TweenHandle handle = builder.GetHandle();
            TweenManager.TrackTweenForComponent(image, handle);
            builder.SetTrackedComponent(image);
            builder.SetTrackedComponent(image);

            return builder;
        }

        /// <summary>Animates the alpha.</summary>
        public static TweenBuilder DOFade(
            this Image image,
            float targetAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] DOFade() was called with a null Image — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Alpha(image, targetAlpha, duration, ease, easeParameter);

            TweenHandle handle = builder.GetHandle();
            TweenManager.TrackTweenForComponent(image, handle);
            builder.SetTrackedComponent(image);
            builder.SetTrackedComponent(image);

            return builder;
        }
    }
}
