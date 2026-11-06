using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    /// <summary>
    /// Tracks progress of tweens (elapsed time, completion, eased progress).
    /// Single Responsibility: Progress tracking only
    /// </summary>
    public class TweenProgressTracker
    {
        private readonly EasingCurveEvaluator easing;

        public TweenProgressTracker(EasingCurveEvaluator easing)
        {
            this.easing = easing;
        }

        /// <summary>
        /// Calculate eased progress (0-1) for tween at current elapsed time.
        /// </summary>
        public float CalculateEasedProgress(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return 0f;

            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            float normalizedTime = clock.Duration > 0f ? clock.ElapsedTime / clock.Duration : 1f;

            return easing.Evaluate(normalizedTime, clock.Ease, clock.EaseParameter);
        }

        /// <summary>
        /// Check if tween has finished (elapsed time >= duration).
        /// </summary>
        public bool IsFinished(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return true;

            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            return clock.ElapsedTime >= clock.Duration;
        }

        /// <summary>
        /// Update elapsed time for tween.
        /// </summary>
        public void UpdateElapsedTime(EcsEntity entity, float deltaTime)
        {
            if (!entity.IsAlive())
                return;

            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            ref var control = ref entity.Get<TweenCoreComponent>().Control;

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
        }

        /// <summary>
        /// Get raw linear progress (0-1) based on elapsed time.
        /// </summary>
        public float GetLinearProgress(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return 0f;

            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            return clock.Duration > 0f ? clock.ElapsedTime / clock.Duration : 1f;
        }

        /// <summary>
        /// Reset tween progress to start.
        /// </summary>
        public void Reset(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            clock.ElapsedTime = 0f;

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            control.State = TweenControlComponent.ControlState.Playing;
            control.PausedElapsedTime = 0f;
        }
    }
}

