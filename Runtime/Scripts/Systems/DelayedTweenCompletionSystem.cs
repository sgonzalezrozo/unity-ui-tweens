using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Systems
{
    /// <summary>
    /// System that detects when delayed tweens finish their delay phase and are transitioning to playing.
    /// Updates start values from current UI state and kills competitor tweens.
    /// CRITICAL: This system runs FIRST before all other tween systems.
    /// </summary>
    public class DelayedTweenCompletionSystem : IEcsRunSystem
    {
        private readonly TweenFilterSet filters;

        internal DelayedTweenCompletionSystem(TweenFilterSet filters)
        {
            this.filters = filters;
        }

        public void Run()
        {
            var delayedTweensFilter = filters.Delayed;
            int remaining = filters.ActiveTotal;
            if (delayedTweensFilter == null || remaining == 0)
                return;

            foreach (int idx in delayedTweensFilter)
            {
                ref TweenCoreComponent core = ref delayedTweensFilter.Get1(idx);
                ref var clock = ref core.Clock;
                ref var control = ref core.Control;
                ref var target = ref core.Target;

                EcsEntity entity = delayedTweensFilter.GetEntity(idx);

                // Every active tween has been seen, so the rest are pooled.
                if (remaining == 0)
                    break;

                if (control.State == TweenControlComponent.ControlState.Pooled)
                    continue;

                remaining--;

                if (control.State != TweenControlComponent.ControlState.Delayed || !clock.IsDelayFinished)
                    continue;

                if (clock.DelayDuration <= 0f)
                    continue;

                if (control.HasKilledCompetitors)
                    continue;

                UpdateStartValuesFromUI(entity, target);

                ref TweenClockComponent clockComp = ref entity.Get<TweenCoreComponent>().Clock;
                clockComp.HasCapturedStartValues = true;

                if (control.PredecessorHandle.IsValid)
                {
                    TweenManager.Cancel(control.PredecessorHandle);

                    ref TweenTargetComponent targetComp = ref entity.Get<TweenCoreComponent>().Target;
                    if (targetComp.RectTransform != null)
                    {
                        TweenPropertyType propType = TweenManager.DetectTweenPropertyType(entity);
                        TweenManager.RemoveFromPendingCancellations(targetComp.RectTransform, propType);

                        TweenHandle currentHandle;
                        if (TweenManager.TryGetComponentTween(targetComp.RectTransform, propType, out currentHandle))
                        {
                            if (currentHandle.Entity == control.PredecessorHandle.Entity)
                                TweenManager.RemoveComponentTween(targetComp.RectTransform, propType);
                        }
                    }
                }
                else
                {
                    TweenManager.KillCompetitorTweens(entity);
                }

                control.HasKilledCompetitors = true;
            }
        }

        /// <summary>
        /// Update a delayed tween's start value to the current UI state.
        /// Called when a delayed tween's delay finishes, before animation begins.
        /// </summary>
        private void UpdateStartValuesFromUI(EcsEntity entity, TweenTargetComponent target)
        {
            if (!entity.IsAlive() || target.RectTransform == null)
                return;

            // A tween with an explicit start value, such as DOColorFromTo, keeps it.
            if (!entity.Get<TweenCoreComponent>().Clock.StartFromCurrent)
                return;

            // Check which property is being animated and update its start value
            if (entity.Has<TweenAlphaComponent>() && target.CanvasGroup != null)
            {
                float currentAlpha = target.CanvasGroup.alpha;
                ref TweenAlphaComponent alphaComp = ref entity.Get<TweenAlphaComponent>();
                alphaComp.StartValue = currentAlpha;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.AlphaResult = currentAlpha;
            }
            else if (entity.Has<TweenColorComponent>() && target.Image != null)
            {
                Color currentColor = target.Image.color;
                ref TweenColorComponent colorComp = ref entity.Get<TweenColorComponent>();
                colorComp.StartValue = currentColor;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.ColorResult = currentColor;
            }
            else if (entity.Has<TweenPositionComponent>())
            {
                Vector3 currentPos = target.RectTransform.anchoredPosition;
                ref TweenPositionComponent posComp = ref entity.Get<TweenPositionComponent>();
                posComp.StartValue = currentPos;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.PositionResult = currentPos;
            }
            else if (entity.Has<TweenScaleComponent>())
            {
                Vector3 currentScale = target.RectTransform.localScale;
                ref TweenScaleComponent scaleComp = ref entity.Get<TweenScaleComponent>();
                scaleComp.StartValue = currentScale;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.ScaleResult = currentScale;
            }
            else if (entity.Has<TweenRotationComponent>())
            {
                float currentRotation = target.RectTransform.eulerAngles.z;
                ref TweenRotationComponent rotComp = ref entity.Get<TweenRotationComponent>();
                rotComp.StartValue = currentRotation;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.RotationResult = currentRotation;
            }
            else if (entity.Has<TweenSizeDeltaComponent>())
            {
                Vector2 currentSize = target.RectTransform.sizeDelta;
                ref TweenSizeDeltaComponent sizeComp = ref entity.Get<TweenSizeDeltaComponent>();
                sizeComp.StartValue = currentSize;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.SizeDeltaResult = currentSize;
            }
            else if (entity.Has<TweenPivotComponent>())
            {
                Vector2 currentPivot = target.RectTransform.pivot;
                ref TweenPivotComponent pivotComp = ref entity.Get<TweenPivotComponent>();
                pivotComp.StartValue = currentPivot;

                ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
                result.PivotResult = currentPivot;
            }
        }
    }
}

