using UITweens.Math;
using UITweens.Services;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Extensions
{
    public static class ImageTweenExtensions
    {
        public static TweenBuilder DOColor(
            this Image image,
            Color targetColor,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] Image is null in DOColor()");
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
                Debug.LogWarning("[UITweens] Image is null in DOColorFromTo()");
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

        public static TweenBuilder DOFade(
            this Image image,
            float targetAlpha,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (image == null)
            {
                Debug.LogWarning("[UITweens] Image is null in DOFade()");
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
