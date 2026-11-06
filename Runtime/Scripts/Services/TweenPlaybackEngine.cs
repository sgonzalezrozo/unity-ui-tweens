using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Executes tween playback: applies values to UI elements based on progress.
    /// Single Responsibility: Animation execution only
    /// </summary>
    public class TweenPlaybackEngine
    {
        private readonly TweenProgressTracker progressTracker;

        public TweenPlaybackEngine(TweenProgressTracker progressTracker)
        {
            this.progressTracker = progressTracker;
        }

        /// <summary>
        /// Apply tween result to target UI element.
        /// </summary>
        public void ApplyTweenResult(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var target = ref entity.Get<TweenCoreComponent>().Target;
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;

            if (target.RectTransform == null)
                return;

            if (flags.ApplyPosition)
                target.RectTransform.anchoredPosition = result.PositionResult;

            if (flags.ApplyScale)
                target.RectTransform.localScale = result.ScaleResult;

            if (flags.ApplyRotation)
            {
                Vector3 euler = target.RectTransform.eulerAngles;
                euler.z = result.RotationResult;
                target.RectTransform.eulerAngles = euler;
            }

            if (flags.ApplySizeDelta)
                target.RectTransform.sizeDelta = result.SizeDeltaResult;

            if (flags.ApplyPivot)
                target.RectTransform.pivot = result.PivotResult;

            if (flags.ApplyAlpha && target.CanvasGroup != null)
                target.CanvasGroup.alpha = result.AlphaResult;

            if (flags.ApplyColor && target.Image != null)
                target.Image.color = result.ColorResult;
        }

        /// <summary>
        /// Update tween progress and apply results.
        /// </summary>
        public void UpdateTween(EcsEntity entity, float deltaTime)
        {
            if (!entity.IsAlive())
                return;

            // Update timing
            progressTracker.UpdateElapsedTime(entity, deltaTime);

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            // Handle delay
            HandleDelay(entity);

            // Calculate eased progress
            float easedProgress = progressTracker.CalculateEasedProgress(entity);
            Mathf.Clamp01(easedProgress);

            // Apply results
            ApplyTweenResult(entity);

            // Handle completion
            if (progressTracker.IsFinished(entity))
                HandleCompletion(entity);
        }

        /// <summary>
        /// Handle start callback (called when delay finishes).
        /// </summary>
        public void FireOnStartCallback(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var advCallbacks = ref entity.Get<TweenAdvancedCallbackComponent>();
            if (advCallbacks.OnStart != null && !advCallbacks.StartCalled)
            {
                advCallbacks.StartCalled = true;
                advCallbacks.OnStart?.Invoke();
            }
        }

        /// <summary>
        /// Handle update callback (called every frame).
        /// </summary>
        public void FireOnUpdateCallback(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var advCallbacks = ref entity.Get<TweenAdvancedCallbackComponent>();
            advCallbacks.OnUpdate?.Invoke();
        }

        /// <summary>
        /// Handle completion callback.
        /// </summary>
        public void FireOnCompleteCallback(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var callbacks = ref entity.Get<TweenCallbackComponent>();
            callbacks.OnComplete?.Invoke();

            ref var advCallbacks = ref entity.Get<TweenAdvancedCallbackComponent>();
            advCallbacks.OnUpdate = null;
        }

        /// <summary>
        /// Pause tween execution.
        /// </summary>
        public void Pause(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            if (control.State == TweenControlComponent.ControlState.Playing)
            {
                ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
                control.PausedElapsedTime = clock.ElapsedTime;
                control.State = TweenControlComponent.ControlState.Paused;
            }
        }

        /// <summary>
        /// Resume paused tween.
        /// </summary>
        public void Resume(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            if (control.State == TweenControlComponent.ControlState.Paused)
            {
                control.State = TweenControlComponent.ControlState.Playing;
            }
        }

        /// <summary>
        /// Cancel tween execution.
        /// </summary>
        public void Cancel(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            control.State = TweenControlComponent.ControlState.Cancelled;
        }

        private void HandleDelay(EcsEntity entity)
        {
            ref var delay = ref entity.Get<TweenCoreComponent>().Delay;
            if (delay.ElapsedDelay < delay.DelayTime)
            {
                delay.ElapsedDelay += Time.deltaTime;
                if (delay.ElapsedDelay >= delay.DelayTime)
                    FireOnStartCallback(entity);
            }
        }

        private void HandleCompletion(EcsEntity entity)
        {
            ref var loop = ref entity.Get<TweenLoopComponent>();

            if (loop.Mode == TweenLoopComponent.LoopMode.None)
            {
                FireOnCompleteCallback(entity);
            }
            else if (loop.Mode == TweenLoopComponent.LoopMode.Repeat)
            {
                loop.CurrentLoop++;
                if (loop.CurrentLoop >= loop.LoopCount)
                {
                    FireOnCompleteCallback(entity);
                }
                else
                {
                    progressTracker.Reset(entity);
                }
            }
            else if (loop.Mode == TweenLoopComponent.LoopMode.Infinite)
            {
                progressTracker.Reset(entity);
            }
            else if (loop.Mode == TweenLoopComponent.LoopMode.Reverse)
            {
                FireOnCompleteCallback(entity);
            }
            else if (loop.Mode == TweenLoopComponent.LoopMode.PingPong)
            {
                loop.CurrentLoop++;
                if (loop.CurrentLoop >= loop.LoopCount)
                {
                    FireOnCompleteCallback(entity);
                }
                else
                {
                    progressTracker.Reset(entity);
                }
            }
        }
    }
}

