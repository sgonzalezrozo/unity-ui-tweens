using UITweens.Math;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Extensions
{
    public static class RectTransformTweenExtensions
    {
        /// <summary>Animates anchoredPosition.</summary>
        public static TweenBuilder DOAnchorPos(
            this RectTransform rect,
            Vector2 targetPos,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOAnchorPos() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Position(rect, targetPos, duration, ease, easeParameter);
            builder.SetTrackedComponent(rect);
            builder.SetTrackedComponent(rect);

            return builder;
        }

        /// <summary>Animates anchoredPosition.x.</summary>
        public static TweenBuilder DOAnchorPosX(
            this RectTransform rect,
            float targetX,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOAnchorPosX() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .PositionX(rect, targetX, duration, ease, easeParameter);
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates anchoredPosition.y.</summary>
        public static TweenBuilder DOAnchorPosY(
            this RectTransform rect,
            float targetY,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOAnchorPosY() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .PositionY(rect, targetY, duration, ease, easeParameter);
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates localScale.</summary>
        public static TweenBuilder DOScale(
            this RectTransform rect,
            Vector3 targetScale,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOScale() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Scale(rect, targetScale, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates localScale.x.</summary>
        public static TweenBuilder DOScaleX(
            this RectTransform rect,
            float targetX,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOScaleX() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .ScaleX(rect, targetX, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates localScale.y.</summary>
        public static TweenBuilder DOScaleY(
            this RectTransform rect,
            float targetY,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOScaleY() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .ScaleY(rect, targetY, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates localScale.z.</summary>
        public static TweenBuilder DOScaleZ(
            this RectTransform rect,
            float targetZ,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOScaleZ() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .ScaleZ(rect, targetZ, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates the Z rotation.</summary>
        public static TweenBuilder DORotate(
            this RectTransform rect,
            float targetRotation,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DORotate() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .RotationZ(rect, targetRotation, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates sizeDelta.</summary>
        public static TweenBuilder DOSizeDelta(
            this RectTransform rect,
            Vector2 targetSize,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOSizeDelta() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Size(rect, targetSize, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }

        /// <summary>Animates pivot.</summary>
        public static TweenBuilder DOPivot(
            this RectTransform rect,
            Vector2 targetPivot,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (rect == null)
            {
                Debug.LogWarning("[UITweens] DOPivot() was called with a null RectTransform — no tween was created.");
                return new TweenBuilder(TweenManager.GetSpawner());
            }

            TweenBuilder builder = TweenManager.GetSpawner()
                .Pivot(rect, targetPivot, duration, ease, easeParameter);
            TweenManager.TrackTweenForComponent(rect, builder.GetHandle());
            builder.SetTrackedComponent(rect);
            return builder;
        }
    }
}

