using System;
using System.Collections.Generic;
using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Systems
{
    /// <summary>
    /// Applies computed values to UI components, runs callbacks and ends finished tweens.
    /// Callbacks can start new tweens, which join the end of the list and wait for the next update.
    /// </summary>
    public class UGUITweenApplySystem : IEcsRunSystem
    {
        private readonly TweenPoolManager poolManager;
        private readonly List<EcsEntity> finishedEntities = new();
        private readonly List<EcsEntity> cancelledEntities = new();

        public UGUITweenApplySystem(TweenPoolManager poolManager)
        {
            this.poolManager = poolManager;
        }

        public void Run()
        {
            if (poolManager == null)
                return;

            finishedEntities.Clear();
            cancelledEntities.Clear();

            List<EcsEntity> activeEntities = poolManager.ActiveEntities;
            int activeCount = activeEntities.Count;
            for (int i = 0; i < activeCount; i++)
                ProcessEntity(activeEntities[i]);

            for (int i = 0; i < finishedEntities.Count; i++)
                CompleteEntity(finishedEntities[i]);

            for (int i = 0; i < cancelledEntities.Count; i++)
                CancelEntity(cancelledEntities[i]);
        }

        private void ProcessEntity(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            TweenControlComponent.ControlState state = entity.Get<TweenCoreComponent>().Control.State;

            if (state == TweenControlComponent.ControlState.Cancelled)
            {
                cancelledEntities.Add(entity);
                return;
            }

            if (state != TweenControlComponent.ControlState.Playing)
                return;

            ref TweenTargetComponent target = ref entity.Get<TweenCoreComponent>().Target;
            if (target.RectTransform == null)
            {
                TweenControlOperations.Cancel(entity);
                cancelledEntities.Add(entity);
                return;
            }

            ApplyValues(entity, ref target);

            bool hasCallbacks = entity.Has<TweenAdvancedCallbackComponent>();
            if (hasCallbacks)
                InvokeUpdateCallbacks(entity);

            // Only a callback can change the state after the check above.
            if (entity.Get<TweenCoreComponent>().Clock.IsFinished
                && (!hasCallbacks || entity.Get<TweenCoreComponent>().Control.State == TweenControlComponent.ControlState.Playing))
            {
                finishedEntities.Add(entity);
            }
        }

        private static void ApplyValues(EcsEntity entity, ref TweenTargetComponent target)
        {
            ref TweenResultComponent result = ref entity.Get<TweenCoreComponent>().Result;
            ref TweenApplyFlagsComponent flags = ref entity.Get<TweenCoreComponent>().Flags;

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
        }

        // Callbacks can resize component storage, so values are copied before each one runs.
        private static void InvokeUpdateCallbacks(EcsEntity entity)
        {
            ref TweenAdvancedCallbackComponent callbacks = ref entity.Get<TweenAdvancedCallbackComponent>();
            Action onStart = callbacks.StartCalled ? null : callbacks.OnStart;
            Action onUpdate = callbacks.OnUpdate;
            Action<float> onFloatUpdate = callbacks.OnFloatUpdate;
            Action<Vector3> onVectorUpdate = callbacks.OnVectorUpdate;
            callbacks.StartCalled = true;

            Invoke(onStart);
            Invoke(onUpdate);

            if (onFloatUpdate != null && entity.Has<TweenCustomFloatComponent>())
            {
                float value = entity.Get<TweenCustomFloatComponent>().CurrentValue;
                Invoke(onFloatUpdate, value);
            }

            if (onVectorUpdate != null && entity.Has<TweenCustomVectorComponent>())
            {
                Vector3 value = entity.Get<TweenCustomVectorComponent>().CurrentValue;
                Invoke(onVectorUpdate, value);
            }
        }

        private void CompleteEntity(EcsEntity entity)
        {
            if (entity.Has<TweenCallbackComponent>())
            {
                ref TweenCallbackComponent callback = ref entity.Get<TweenCallbackComponent>();
                Action onComplete = callback.OnComplete;
                Delegate stateCallback = callback.StateCallback;
                object state = callback.State;
                Action<Delegate, object> stateInvoker = callback.StateInvoker;

                callback.OnComplete = null;
                callback.StateCallback = null;
                callback.State = null;
                callback.StateInvoker = null;

                Invoke(onComplete);

                if (stateCallback != null)
                    InvokeWithState(stateInvoker, stateCallback, state);
            }

            TweenSequencer.ActivateNext(entity);
            poolManager.ReleaseEntity(entity);
        }

        private void CancelEntity(EcsEntity entity)
        {
            if (entity.Has<TweenCancelCallbackComponent>())
            {
                ref TweenCancelCallbackComponent callback = ref entity.Get<TweenCancelCallbackComponent>();
                Action onCancelled = callback.OnCancelled;
                callback.OnCancelled = null;
                Invoke(onCancelled);
            }

            TweenSequencer.CancelNext(entity);
            poolManager.ReleaseEntity(entity);
        }

        private static void Invoke(Action callback)
        {
            if (callback == null)
                return;

            try
            {
                callback.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[UITweens] A tween callback threw an exception. The tween will still complete and other tweens will continue running.");
                Debug.LogException(exception);
            }
        }

        private static void InvokeWithState(Action<Delegate, object> invoker, Delegate callback, object state)
        {
            try
            {
                invoker.Invoke(callback, state);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[UITweens] A tween callback threw an exception. The tween will still complete and other tweens will continue running.");
                Debug.LogException(exception);
            }
        }

        private static void Invoke<T>(Action<T> callback, T value)
        {
            try
            {
                callback.Invoke(value);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[UITweens] A tween callback threw an exception. The tween will still complete and other tweens will continue running.");
                Debug.LogException(exception);
            }
        }

        private static void ApplyRotation(RectTransform rectTransform, float rotationZ)
        {
            Vector3 currentRotation = rectTransform.eulerAngles;
            currentRotation.z = rotationZ;
            rectTransform.eulerAngles = currentRotation;
        }
    }
}
