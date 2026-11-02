using System.Collections.Generic;
using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Systems
{
    public class UGUITweenApplySystem : IEcsRunSystem
    {
        private readonly TweenPoolManager poolManager;
        private List<EcsEntity> entitiesToDestroy = new();
        private HashSet<EcsEntity> cachedActiveEntities;

        public UGUITweenApplySystem(TweenPoolManager poolManager)
        {
            this.poolManager = poolManager;
        }

        public void Run()
        {
            if (poolManager == null)
                return;

            cachedActiveEntities = poolManager.GetAllActiveEntities() as HashSet<EcsEntity>;
            if (cachedActiveEntities == null)
                return;

            ProcessStartCallbacks();
            ProcessCompletionCallbacks();
            ProcessApply();
            ProcessCancellationCleanup();
        }

        private void ProcessStartCallbacks()
        {
            foreach (EcsEntity entity in cachedActiveEntities)
            {
                if (!entity.IsAlive())
                    continue;

                try
                {
                    ref TweenAdvancedCallbackComponent callback = ref entity.Get<TweenAdvancedCallbackComponent>();
                    ref TweenClockComponent clock = ref entity.Get<TweenClockComponent>();
                    ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();

                    if (!clock.IsDelayFinished)
                        continue;

                    if (control.State != TweenControlComponent.ControlState.Playing || callback.StartCalled || callback.OnStart == null)
                        continue;

                    callback.StartCalled = true;
                    callback.OnStart.Invoke();
                }
                catch
                {
                    // Entity doesn't have required components
                }
            }
        }

        private void ProcessCompletionCallbacks()
        {
            foreach (EcsEntity entity in cachedActiveEntities)
            {
                if (!entity.IsAlive())
                    continue;

                try
                {
                    ref TweenCallbackComponent callback = ref entity.Get<TweenCallbackComponent>();
                    ref TweenClockComponent clock = ref entity.Get<TweenClockComponent>();
                    ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();

                    if (!clock.IsFinished)
                        continue;

                    if (control.State != TweenControlComponent.ControlState.Playing || !callback.HasCallback)
                        continue;

                    callback.OnComplete.Invoke();
                    callback.OnComplete = null;
                }
                catch
                {
                    // Entity doesn't have required components
                }
            }
        }

        private void ProcessApply()
        {
            entitiesToDestroy.Clear();

            foreach (EcsEntity entity in cachedActiveEntities)
            {
                if (!entity.IsAlive())
                    continue;

                try
                {
                    ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
                    ref TweenResultComponent result = ref entity.Get<TweenResultComponent>();
                    ref TweenApplyFlagsComponent flags = ref entity.Get<TweenApplyFlagsComponent>();
                    ref TweenClockComponent clock = ref entity.Get<TweenClockComponent>();
                    ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();

                    if (target.RectTransform == null || control.State != TweenControlComponent.ControlState.Playing)
                        continue;

                    if (flags.ApplyPosition)
                        target.RectTransform.anchoredPosition = result.PositionResult;

                    if (flags.ApplyScale)
                        target.RectTransform.localScale = result.ScaleResult;

                    if (flags.ApplyRotation)
                        ApplyRotation(target.RectTransform, result.RotationResult);

                    if (flags.ApplyAlpha && target.CanvasGroup != null)
                        target.CanvasGroup.alpha = result.AlphaResult;

                    if (flags.ApplyColor && target.Image != null)
                        target.Image.color = result.ColorResult;

                    if (flags.ApplySizeDelta)
                        target.RectTransform.sizeDelta = result.SizeDeltaResult;

                    if (flags.ApplyPivot)
                        target.RectTransform.pivot = result.PivotResult;

                    if (clock.IsFinished)
                    {
                        entity.Del<TweenActiveMarkerComponent>();
                        entitiesToDestroy.Add(entity);
                    }
                }
                catch
                {
                    // Entity doesn't have required components
                }
            }

            for (int i = 0; i < entitiesToDestroy.Count; i++)
                entitiesToDestroy[i].Destroy();
        }

        private void ProcessCancellationCleanup()
        {
            entitiesToDestroy.Clear();

            foreach (EcsEntity entity in cachedActiveEntities)
            {
                if (!entity.IsAlive())
                    continue;

                try
                {
                    ref TweenCancelCallbackComponent callback = ref entity.Get<TweenCancelCallbackComponent>();
                    ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();

                    if (control.State != TweenControlComponent.ControlState.Cancelled)
                        continue;

                    if (callback.HasCallback)
                    {
                        callback.OnCancelled.Invoke();
                        callback.OnCancelled = null;
                    }

                    entity.Del<TweenActiveMarkerComponent>();
                    entitiesToDestroy.Add(entity);
                }
                catch
                {
                    // Entity doesn't have required components
                }
            }

            for (int i = 0; i < entitiesToDestroy.Count; i++)
                entitiesToDestroy[i].Destroy();
        }

        private void ApplyRotation(RectTransform rectTransform, float rotationZ)
        {
            Vector3 currentRotation = rectTransform.eulerAngles;
            currentRotation.z = rotationZ;
            rectTransform.eulerAngles = currentRotation;
        }
    }
}
