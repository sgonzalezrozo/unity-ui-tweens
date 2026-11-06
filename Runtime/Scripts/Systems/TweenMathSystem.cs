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
        private readonly TweenFilterSet filters;

        internal TweenMathSystem(TweenFilterSet filters)
        {
            this.filters = filters;
        }

        public void Run()
        {
            float deltaTime = Time.deltaTime;

            // Filters exist only for tween types that have been used.
            var alphaFilter = filters.Alpha;
            var positionFilter = filters.Position;
            var scaleFilter = filters.Scale;
            var rotationFilter = filters.Rotation;
            var colorFilter = filters.Color;
            var sizeFilter = filters.SizeDelta;
            var pivotFilter = filters.Pivot;
            var customFloatFilter = filters.CustomFloat;
            var customVectorFilter = filters.CustomVector;

            int remainingAlpha = filters.GetActiveCount(TweenPoolKind.Alpha);
            int remainingPosition = filters.GetActiveCount(TweenPoolKind.Position);
            int remainingScale = filters.GetActiveCount(TweenPoolKind.Scale);
            int remainingRotation = filters.GetActiveCount(TweenPoolKind.Rotation);
            int remainingColor = filters.GetActiveCount(TweenPoolKind.Color);
            int remainingSizeDelta = filters.GetActiveCount(TweenPoolKind.SizeDelta);
            int remainingPivot = filters.GetActiveCount(TweenPoolKind.Pivot);
            int remainingCustomFloat = filters.GetActiveCount(TweenPoolKind.CustomFloat);
            int remainingCustomVector = filters.GetActiveCount(TweenPoolKind.CustomVector);

            // ...existing code for alpha, position, scale, rotation filters...

            // Process alpha tweens
            if (alphaFilter != null && remainingAlpha > 0)
            foreach (int idx in alphaFilter)
            {
                ref TweenClockComponent clock = ref alphaFilter.Get1(idx);
                ref TweenAlphaComponent alpha = ref alphaFilter.Get2(idx);
                ref TweenResultComponent result = ref alphaFilter.Get3(idx);
                ref TweenControlComponent control = ref alphaFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingAlpha == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingAlpha--;

                // Skip cancelled tweens
                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                // Skip paused tweens
                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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

                            EcsEntity entity = alphaFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                // Only animate if playing
                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;

                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.AlphaResult = TweenMath.Lerp(alpha.StartValue, alpha.EndValue, easedT);

                // NEW: Handle looping - check if animation finished and should loop
                if (clock.IsFinished)
                {
                    EcsEntity entity = alphaFilter.GetEntity(idx);

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

            // Process position tweens
            if (positionFilter != null && remainingPosition > 0)
            foreach (int idx in positionFilter)
            {
                ref TweenClockComponent clock = ref positionFilter.Get1(idx);
                ref TweenPositionComponent position = ref positionFilter.Get2(idx);
                ref TweenResultComponent result = ref positionFilter.Get3(idx);
                ref TweenControlComponent control = ref positionFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingPosition == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingPosition--;

                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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

                            EcsEntity entity = positionFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;


                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.PositionResult = TweenMath.LerpVector2(position.StartValue, position.EndValue, easedT);

                // NEW: Handle looping
                if (clock.IsFinished)
                {
                    EcsEntity entity = positionFilter.GetEntity(idx);

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

            // Process scale tweens
            if (scaleFilter != null && remainingScale > 0)
            foreach (int idx in scaleFilter)
            {
                ref TweenClockComponent clock = ref scaleFilter.Get1(idx);
                ref TweenScaleComponent scale = ref scaleFilter.Get2(idx);
                ref TweenResultComponent result = ref scaleFilter.Get3(idx);
                ref TweenControlComponent control = ref scaleFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingScale == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingScale--;

                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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
                            EcsEntity entity = scaleFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;


                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.ScaleResult = TweenMath.LerpVector3(scale.StartValue, scale.EndValue, easedT);

                // NEW: Handle looping
                if (clock.IsFinished)
                {
                    EcsEntity entity = scaleFilter.GetEntity(idx);

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

            // Process rotation tweens
            if (rotationFilter != null && remainingRotation > 0)
            foreach (int idx in rotationFilter)
            {
                ref TweenClockComponent clock = ref rotationFilter.Get1(idx);
                ref TweenRotationComponent rotation = ref rotationFilter.Get2(idx);
                ref TweenResultComponent result = ref rotationFilter.Get3(idx);
                ref TweenControlComponent control = ref rotationFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingRotation == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingRotation--;

                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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
                            EcsEntity entity = rotationFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;


                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.RotationResult = TweenMath.Lerp(rotation.StartValue, rotation.EndValue, easedT);

                // NEW: Handle looping
                if (clock.IsFinished)
                {
                    EcsEntity entity = rotationFilter.GetEntity(idx);

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

            // Process color tweens
            if (colorFilter != null && remainingColor > 0)
            foreach (int idx in colorFilter)
            {
                ref TweenClockComponent clock = ref colorFilter.Get1(idx);
                ref TweenColorComponent color = ref colorFilter.Get2(idx);
                ref TweenResultComponent result = ref colorFilter.Get3(idx);
                ref TweenControlComponent control = ref colorFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingColor == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingColor--;

                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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
                            EcsEntity entity = colorFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;


                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.ColorResult = TweenMath.LerpColor(color.StartValue, color.EndValue, easedT);

                // NEW: Handle looping
                if (clock.IsFinished)
                {
                    EcsEntity entity = colorFilter.GetEntity(idx);

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


            // Process size delta tweens
            if (sizeFilter != null && remainingSizeDelta > 0)
            foreach (int idx in sizeFilter)
            {
                ref TweenClockComponent clock = ref sizeFilter.Get1(idx);
                ref TweenSizeDeltaComponent size = ref sizeFilter.Get2(idx);
                ref TweenResultComponent result = ref sizeFilter.Get3(idx);
                ref TweenControlComponent control = ref sizeFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingSizeDelta == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingSizeDelta--;

                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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
                            EcsEntity entity = sizeFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;


                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.SizeDeltaResult = TweenMath.LerpVector2(size.StartValue, size.EndValue, easedT);

                // NEW: Handle looping
                if (clock.IsFinished)
                {
                    EcsEntity entity = sizeFilter.GetEntity(idx);

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

            // Process pivot tweens
            if (pivotFilter != null && remainingPivot > 0)
            foreach (int idx in pivotFilter)
            {
                ref TweenClockComponent clock = ref pivotFilter.Get1(idx);
                ref TweenPivotComponent pivot = ref pivotFilter.Get2(idx);
                ref TweenResultComponent result = ref pivotFilter.Get3(idx);
                ref TweenControlComponent control = ref pivotFilter.Get4(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingPivot == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingPivot--;

                if (control.State == TweenControlComponent.ControlState.Cancelled)
                    continue;

                if (control.State == TweenControlComponent.ControlState.Paused)
                    continue;

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
                            EcsEntity entity = pivotFilter.GetEntity(idx);
                            if (entity.Has<TweenTargetComponent>())
                            {
                                ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
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
                        continue;
                    }
                }

                if (control.State != TweenControlComponent.ControlState.Playing)
                    continue;


                clock.ElapsedTime += deltaTime;
                float normalizedT = clock.GetNormalizedTime();
                float easedT = TweenMath.EvaluateEase(normalizedT, clock.Ease, clock.EaseParameter);

                result.PivotResult = TweenMath.LerpVector2(pivot.StartValue, pivot.EndValue, easedT);

                // NEW: Handle looping
                if (clock.IsFinished)
                {
                    EcsEntity entity = pivotFilter.GetEntity(idx);

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

            // Process custom float tweens
            if (customFloatFilter != null && remainingCustomFloat > 0)
            foreach (int idx in customFloatFilter)
            {
                ref TweenClockComponent clock = ref customFloatFilter.Get1(idx);
                ref TweenCustomFloatComponent custom = ref customFloatFilter.Get2(idx);
                ref TweenControlComponent control = ref customFloatFilter.Get3(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingCustomFloat == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingCustomFloat--;

                if (!TryAdvanceClock(ref clock, ref control, deltaTime))
                    continue;

                float easedT = TweenMath.EvaluateEase(clock.GetNormalizedTime(), clock.Ease, clock.EaseParameter);
                custom.CurrentValue = TweenMath.Lerp(custom.StartValue, custom.EndValue, easedT);

                if (!clock.IsFinished)
                    continue;

                EcsEntity entity = customFloatFilter.GetEntity(idx);
                if (TryProcessLoop(ref entity, ref clock) && entity.Get<TweenLoopComponent>().ShouldReverse)
                    SwapValuesForReverse(ref custom.StartValue, ref custom.EndValue);
            }

            // Process custom vector tweens
            if (customVectorFilter != null && remainingCustomVector > 0)
            foreach (int idx in customVectorFilter)
            {
                ref TweenClockComponent clock = ref customVectorFilter.Get1(idx);
                ref TweenCustomVectorComponent custom = ref customVectorFilter.Get2(idx);
                ref TweenControlComponent control = ref customVectorFilter.Get3(idx);

                // Every active tween of this kind has been seen, so the rest are pooled.
                if (remainingCustomVector == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remainingCustomVector--;

                if (!TryAdvanceClock(ref clock, ref control, deltaTime))
                    continue;

                float easedT = TweenMath.EvaluateEase(clock.GetNormalizedTime(), clock.Ease, clock.EaseParameter);
                custom.CurrentValue = Vector3.LerpUnclamped(custom.StartValue, custom.EndValue, easedT);

                if (!clock.IsFinished)
                    continue;

                EcsEntity entity = customVectorFilter.GetEntity(idx);
                if (TryProcessLoop(ref entity, ref clock) && entity.Get<TweenLoopComponent>().ShouldReverse)
                    SwapValuesForReverse(ref custom.StartValue, ref custom.EndValue);
            }
        }

        /// <summary>
        /// Moves a tween through its delay and advances its clock.
        /// Returns true while the tween is playing.
        /// </summary>
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
