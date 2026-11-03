using UITweens.Components;
using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Position animation extensions for TweenSpawner.
    /// Single Responsibility: Position tweens only (X, Y, FromTo).
    /// </summary>
    public static class PositionTweenExtensions
    {
        /// <summary>
        /// Tween from current position to target.
        /// </summary>
        public static TweenBuilder Position(
            this TweenSpawner spawner,
            RectTransform element,
            Vector2 targetPosition,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector2 startPosition = element.anchoredPosition;
            return spawner.PositionFromTo(element, startPosition, targetPosition, duration, ease, easeParameter)
                .StartFromCurrent();
        }

        /// <summary>
        /// Tween from start to end position.
        /// </summary>
        public static TweenBuilder PositionFromTo(
            this TweenSpawner spawner,
            RectTransform element,
            Vector2 startPosition,
            Vector2 endPosition,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            TweenHandle handle = spawner.SpawnTweenInternal(element, duration, ease, easeParameter);
            spawner.InitializePositionInternal(handle, startPosition, endPosition);

            // Track the tween so it can be controlled later.
            TweenManager.TrackTweenForComponent(element, handle);

            return new TweenBuilder(spawner).SetHandle(handle);
        }

        /// <summary>
        /// Tween ONLY X axis (keep Y unchanged).
        /// </summary>
        public static TweenBuilder PositionX(
            this TweenSpawner spawner,
            RectTransform element,
            float targetX,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector2 currentPos = element.anchoredPosition;
            Vector2 targetPos = new Vector2(targetX, currentPos.y);
            return spawner.PositionFromTo(element, currentPos, targetPos, duration, ease, easeParameter)
                .StartFromCurrent(TweenClockComponent.AXIS_X);
        }

        /// <summary>
        /// Tween ONLY Y axis (keep X unchanged).
        /// </summary>
        public static TweenBuilder PositionY(
            this TweenSpawner spawner,
            RectTransform element,
            float targetY,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            Vector2 currentPos = element.anchoredPosition;
            Vector2 targetPos = new Vector2(currentPos.x, targetY);
            return spawner.PositionFromTo(element, currentPos, targetPos, duration, ease, easeParameter)
                .StartFromCurrent(TweenClockComponent.AXIS_Y);
        }
    }
}
