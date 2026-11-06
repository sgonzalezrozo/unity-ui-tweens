using System.Collections.Generic;
using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Math;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Systems
{
    /// <summary>
    /// LeoECS system that advances tween clocks and computes interpolated values.
    ///
    /// THREAD SAFETY: This system is BACKGROUND-THREAD SAFE.
    /// - Performs pure math calculations (easing, interpolation)
    /// - Writes ONLY to TweenResultComponent (computed values)
    /// - Makes ZERO Unity UI calls (no RectTransform, CanvasGroup, etc)
    /// - Can be moved to background jobs or async tasks without risk
    ///
    /// Data Flow:
    ///   Input:  TweenClockComponent (timer), Tween*Component (animation params)
    ///   Calcs:  EvaluateEase() → Lerp() → TweenResultComponent
    ///   Output: TweenResultComponent (computed float/Vector values)
    ///
    /// The UGUITweenApplySystem reads these results on the main thread and applies them to UI.
    /// Respects TweenControlComponent.State (pause/resume/cancel)
    /// Handles Color, SizeDelta, Pivot components
    /// </summary>
    public class TweenMathSystem : IEcsRunSystem
    {
        private readonly TweenPoolManager poolManager;

        internal TweenMathSystem(TweenPoolManager poolManager)
        {
            this.poolManager = poolManager;
        }

        public void Run()
        {
            float deltaTime = Time.deltaTime;

            // Only active tweens are visited, so tweens waiting in the pool cost nothing.
            List<EcsEntity> active = poolManager.ActiveEntities;
            int count = active.Count;

            for (int i = 0; i < count; i++)
            {
                EcsEntity entity = active[i];
                ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();

                switch (core.Control.Kind)
                {
                    case TweenPoolKind.Alpha:
                        ProcessAlpha(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.Position:
                        ProcessPosition(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.Scale:
                        ProcessScale(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.Rotation:
                        ProcessRotation(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.Color:
                        ProcessColor(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.SizeDelta:
                        ProcessSizeDelta(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.Pivot:
                        ProcessPivot(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.CustomFloat:
                        ProcessCustomFloat(entity, ref core, deltaTime);
                        break;

                    case TweenPoolKind.CustomVector:
                        ProcessCustomVector(entity, ref core, deltaTime);
                        break;

                    default:
                        ProcessAnyKind(entity, ref core, deltaTime);
                        break;
                }
            }
        }

        // A tween with no kind can carry any property component.
        private void ProcessAnyKind(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            if (entity.Has<TweenAlphaComponent>())
                ProcessAlpha(entity, ref core, deltaTime);

            if (entity.Has<TweenPositionComponent>())
                ProcessPosition(entity, ref core, deltaTime);

            if (entity.Has<TweenScaleComponent>())
                ProcessScale(entity, ref core, deltaTime);

            if (entity.Has<TweenRotationComponent>())
                ProcessRotation(entity, ref core, deltaTime);

            if (entity.Has<TweenColorComponent>())
                ProcessColor(entity, ref core, deltaTime);

            if (entity.Has<TweenSizeDeltaComponent>())
                ProcessSizeDelta(entity, ref core, deltaTime);

            if (entity.Has<TweenPivotComponent>())
                ProcessPivot(entity, ref core, deltaTime);

            if (entity.Has<TweenCustomFloatComponent>())
                ProcessCustomFloat(entity, ref core, deltaTime);

            if (entity.Has<TweenCustomVectorComponent>())
                ProcessCustomVector(entity, ref core, deltaTime);
        }

        private void ProcessAlpha(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenAlphaComponent alpha = ref entity.Get<TweenAlphaComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            // Skip cancelled tweens
            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            // Skip paused tweens
            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            // NotStarted → Delayed or Playing
            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            // Delayed → Playing with spillover time
            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.CanvasGroup != null)
                            {
                                float currentAlpha = target.CanvasGroup.alpha;
                                alpha.StartValue = currentAlpha;
                                result.AlphaResult = currentAlpha;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                        clock.HasCapturedStartValues = true;

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            // Only animate if playing
            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.AlphaResult = TweenMath.Lerp(alpha.StartValue, alpha.EndValue, easedT);

            // NEW: Handle looping - check if animation finished and should loop
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    // Loop is continuing, handle reverse mode
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref alpha.StartValue, ref alpha.EndValue);
                        }
                    }
                    // Continue to next frame, animation will restart from beginning
                }
            }
        }

        private void ProcessPosition(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenPositionComponent position = ref entity.Get<TweenPositionComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.RectTransform != null)
                            {
                                Vector2 currentPos = target.RectTransform.anchoredPosition;
                                position.StartValue = currentPos;
                                result.PositionResult = currentPos;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                        clock.HasCapturedStartValues = true;

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.PositionResult = TweenMath.LerpVector2(position.StartValue, position.EndValue, easedT);

            // NEW: Handle looping
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref position.StartValue, ref position.EndValue);
                        }
                    }
                }
            }
        }

        private void ProcessScale(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenScaleComponent scale = ref entity.Get<TweenScaleComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        // Update start value from current UI state
                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.RectTransform != null)
                            {
                                Vector3 currentScale = target.RectTransform.localScale;
                                scale.StartValue = currentScale;
                                result.ScaleResult = currentScale;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                    {
                        clock.HasCapturedStartValues = true;
                    }

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.ScaleResult = TweenMath.LerpVector3(scale.StartValue, scale.EndValue, easedT);

            // NEW: Handle looping
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref scale.StartValue, ref scale.EndValue);
                        }
                    }
                }
            }
        }

        private void ProcessRotation(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenRotationComponent rotation = ref entity.Get<TweenRotationComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        // Update start value from current UI state
                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.RectTransform != null)
                            {
                                float currentRotation = target.RectTransform.eulerAngles.z;
                                rotation.StartValue = currentRotation;
                                result.RotationResult = currentRotation;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                    {
                        clock.HasCapturedStartValues = true;
                    }

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.RotationResult = TweenMath.Lerp(rotation.StartValue, rotation.EndValue, easedT);

            // NEW: Handle looping
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref rotation.StartValue, ref rotation.EndValue);
                        }
                    }
                }
            }
        }

        private void ProcessColor(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenColorComponent color = ref entity.Get<TweenColorComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    // Cancel predecessor when delayed tween's delay finishes
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        // Update start value from current UI state
                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.Image != null)
                            {
                                Color currentColor = target.Image.color;
                                color.StartValue = currentColor;
                                result.ColorResult = currentColor;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                    {
                        clock.HasCapturedStartValues = true;
                    }

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.ColorResult = TweenMath.LerpColor(color.StartValue, color.EndValue, easedT);

            // NEW: Handle looping
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref color.StartValue, ref color.EndValue);
                        }
                    }
                }
            }
        }

        private void ProcessSizeDelta(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenSizeDeltaComponent size = ref entity.Get<TweenSizeDeltaComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        // Update start value from current UI state
                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.RectTransform != null)
                            {
                                Vector2 currentSize = target.RectTransform.sizeDelta;
                                size.StartValue = currentSize;
                                result.SizeDeltaResult = currentSize;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                    {
                        clock.HasCapturedStartValues = true;
                    }

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.SizeDeltaResult = TweenMath.LerpVector2(size.StartValue, size.EndValue, easedT);

            // NEW: Handle looping
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref size.StartValue, ref size.EndValue);
                        }
                    }
                }
            }
        }

        private void ProcessPivot(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenPivotComponent pivot = ref entity.Get<TweenPivotComponent>();
            ref TweenResultComponent result = ref core.Result;
            ref TweenControlComponent control = ref core.Control;

            if (control.State == TweenControlComponent.ControlState.Cancelled)
                return;

            if (control.State == TweenControlComponent.ControlState.Paused)
                return;

            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (clock.IsDelayFinished)
                {
                    if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
                    {
                        TweenManager.Cancel(control.PredecessorHandle);
                        control.HasKilledCompetitors = true;

                        // Update start value from current UI state
                        if (entity.Has<TweenCoreComponent>())
                        {
                            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
                            if (target.RectTransform != null)
                            {
                                Vector2 currentPivot = target.RectTransform.pivot;
                                pivot.StartValue = currentPivot;
                                result.PivotResult = currentPivot;
                            }
                        }
                    }

                    if (!clock.HasCapturedStartValues)
                    {
                        clock.HasCapturedStartValues = true;
                    }

                    float spillover = clock.ElapsedDelay - clock.DelayDuration;
                    control.State = TweenControlComponent.ControlState.Playing;
                    clock.ElapsedTime = spillover > 0 ? spillover : 0f;
                }
                else
                {
                    return;
                }
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            clock.ElapsedTime += deltaTime;
            float normalizedT = clock.GetNormalizedTime();
            float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

            result.PivotResult = TweenMath.LerpVector2(pivot.StartValue, pivot.EndValue, easedT);

            // NEW: Handle looping
            if (clock.IsFinished)
            {

                if (TryProcessLoop(ref entity, ref clock))
                {
                    if (entity.Has<TweenLoopComponent>())
                    {
                        ref var loop = ref entity.Get<TweenLoopComponent>();
                        if (loop.ShouldReverse)
                        {
                            SwapValuesForReverse(ref pivot.StartValue, ref pivot.EndValue);
                        }
                    }
                }
            }
        }

        private void ProcessCustomFloat(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenCustomFloatComponent custom = ref entity.Get<TweenCustomFloatComponent>();
            ref TweenControlComponent control = ref core.Control;

            if (!TryAdvanceClock(ref clock, ref control, deltaTime))
                return;

            float easedT = TweenMath.EvaluateEase(clock.GetNormalizedTime(), clock.Ease, clock.EaseParameter);
            custom.CurrentValue = TweenMath.Lerp(custom.StartValue, custom.EndValue, easedT);

            if (!clock.IsFinished)
                return;

            if (TryProcessLoop(ref entity, ref clock) && entity.Get<TweenLoopComponent>().ShouldReverse)
                SwapValuesForReverse(ref custom.StartValue, ref custom.EndValue);
        }

        private void ProcessCustomVector(EcsEntity entity, ref TweenCoreComponent core, float deltaTime)
        {
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenCustomVectorComponent custom = ref entity.Get<TweenCustomVectorComponent>();
            ref TweenControlComponent control = ref core.Control;

            if (!TryAdvanceClock(ref clock, ref control, deltaTime))
                return;

            float easedT = TweenMath.EvaluateEase(clock.GetNormalizedTime(), clock.Ease, clock.EaseParameter);
            custom.CurrentValue = Vector3.LerpUnclamped(custom.StartValue, custom.EndValue, easedT);

            if (!clock.IsFinished)
                return;

            if (TryProcessLoop(ref entity, ref clock) && entity.Get<TweenLoopComponent>().ShouldReverse)
                SwapValuesForReverse(ref custom.StartValue, ref custom.EndValue);
        }

        private static bool TryAdvanceClock(ref TweenClockComponent clock, ref TweenControlComponent control, float deltaTime)
        {
            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = clock.HasDelay
                    ? TweenControlComponent.ControlState.Delayed
                    : TweenControlComponent.ControlState.Playing;
            }

            if (control.State == TweenControlComponent.ControlState.Delayed)
            {
                clock.ElapsedDelay += deltaTime;
                if (!clock.IsDelayFinished)
                    return false;

                float spillover = clock.ElapsedDelay - clock.DelayDuration;
                control.State = TweenControlComponent.ControlState.Playing;
                clock.ElapsedTime = spillover > 0 ? spillover : 0f;
            }

            if (control.State != TweenControlComponent.ControlState.Playing)
                return false;

            clock.ElapsedTime += deltaTime;
            return true;
        }

        /// <summary>
        /// Check if tween should loop and process loop logic.
        /// Returns true if looping should continue, false if animation is finished.
        /// CRITICAL FIX: Only allow looping if Mode is explicitly set to Infinite or Repeat/Reverse/PingPong
        /// </summary>
        private bool TryProcessLoop(ref EcsEntity entity, ref TweenClockComponent clock)
        {
            if (!entity.Has<TweenLoopComponent>())
                return false; // No loop component, animation finished

            ref var loop = ref entity.Get<TweenLoopComponent>();

            // CRITICAL: Only loop if Mode is explicitly set to a looping mode
            // Mode.None means no looping - animation is finished
            if (loop.Mode == TweenLoopComponent.LoopMode.None)
            {
                return false;
            }

            // Check if loop should continue
            bool shouldLoop = loop.Mode == TweenLoopComponent.LoopMode.Infinite ||
                              loop.CurrentLoop < loop.LoopCount;

            if (!shouldLoop)
            {
                return false; // Loop finished
            }

            // Reset animation progress for next iteration
            clock.ElapsedTime = 0f;
            loop.CurrentLoop++;

            // Mark if we need to reverse values
            if (loop.Mode == TweenLoopComponent.LoopMode.Reverse ||
                loop.Mode == TweenLoopComponent.LoopMode.PingPong)
            {
                loop.ShouldReverse = !loop.ShouldReverse;
            }

            // Continue looping
            return true;
        }

        /// <summary>
        /// Swap start and end values for Reverse/PingPong mode.
        /// </summary>
        private void SwapValuesForReverse<T>(ref T start, ref T end) where T : struct
        {
            (start, end) = (end, start);
        }
    }
}
