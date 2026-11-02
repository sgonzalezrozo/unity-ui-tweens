using System.Collections.Generic;
using Leopotam.Ecs;
using Tools.UITweens.Runtime.Scripts.Components;
using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Services
{
    public class TweenDelayedSpawner
    {
        private DelayedTweenQueue delayedTweenQueue = new();
        private List<TweenDefinition> cachedReadyToSpawnList = new();
        private TweenTracker tracker;

        public TweenDelayedSpawner(TweenTracker tracker)
        {
            this.tracker = tracker;
        }

        public void EnqueueDelayedTween(TweenDefinition definition)
            => delayedTweenQueue.Enqueue(definition);

        public void ProcessDelayQueue(float deltaTime, EcsWorld world)
        {
            cachedReadyToSpawnList.Clear();
            delayedTweenQueue.ProcessDelays(deltaTime, cachedReadyToSpawnList);

            if (cachedReadyToSpawnList.Count == 0)
                return;

            TweenSpawner spawner = new TweenSpawner(world);
            for (int i = 0; i < cachedReadyToSpawnList.Count; i++)
                SpawnDelayedTween(cachedReadyToSpawnList[i], spawner);
        }

        public void Cleanup()
            => cachedReadyToSpawnList?.Clear();

        private void SpawnDelayedTween(TweenDefinition def, TweenSpawner spawner)
        {
            if (def.TargetComponent == null)
            {
                Debug.LogWarning("[UITweens] Cannot spawn delayed tween with null target component");
                return;
            }

            int componentId = def.TargetComponent.GetInstanceID();

            Debug.Log($"[SpawnDelayedTween] Spawning delayed tween for {def.TargetComponent.name}, property: {def.PropertyType}, duration: {def.Duration}");

            TweenHandle oldHandle = new TweenHandle();
            if (tracker.TryGetTween(def.TargetComponent, def.PropertyType, out TweenHandle previousHandle))
            {
                oldHandle = previousHandle;
                tracker.RemoveTween(def.TargetComponent, def.PropertyType);
                Debug.Log($"[SpawnDelayedTween] Found previous tween entity {previousHandle.Entity.GetInternalId()}, will cancel it");
            }


            TweenHandle handle = spawner.SpawnTweenInternal(
                def.RectTransform,
                def.Duration,
                def.Ease,
                def.EaseParameter,
                def.CanvasGroup,
                def.Image
            );

            if (!handle.IsValid)
            {
                Debug.LogError("[SpawnDelayedTween] Failed to spawn tween");
                return;
            }

            Debug.Log($"[SpawnDelayedTween] New tween entity created: {handle.Entity.GetInternalId()}");

            InitializeTweenProperties(handle, def, spawner);
            InitializeTweenCallbacks(handle, def);
            InitializeTweenLoop(handle, def);

            if (oldHandle.IsValid)
            {
                Debug.Log($"[SpawnDelayedTween] Cancelling old tween entity {oldHandle.Entity.GetInternalId()} for {def.TargetComponent.name}, property: {def.PropertyType}");
                TweenControlOperations.Cancel(oldHandle);
            }

            tracker.TrackTween(def.TargetComponent, handle);
            Debug.Log($"[SpawnDelayedTween] Tracked new delayed tween - {def.TargetComponent.name}, property: {def.PropertyType}, entity: {handle.Entity.GetInternalId()}");
        }

        private void InitializeTweenProperties(TweenHandle handle, TweenDefinition def, TweenSpawner spawner)
        {
            if (def.HasAlpha)
                spawner.InitializeAlphaInternal(handle, def.StartAlpha, def.EndAlpha);
            if (def.HasPosition)
                spawner.InitializePositionInternal(handle, def.StartPosition, def.EndPosition);
            if (def.HasScale)
                spawner.InitializeScaleInternal(handle, def.StartScale, def.EndScale);
            if (def.HasRotation)
                spawner.InitializeRotationInternal(handle, def.StartRotation, def.EndRotation);
            if (def.HasColor)
                spawner.InitializeColorInternal(handle, def.StartColor, def.EndColor);
            if (def.HasSizeDelta)
                spawner.InitializeSizeDeltaInternal(handle, def.StartSizeDelta, def.EndSizeDelta);
            if (def.HasPivot)
                spawner.InitializePivotInternal(handle, def.StartPivot, def.EndPivot);
        }

        private void InitializeTweenCallbacks(TweenHandle handle, TweenDefinition def)
        {
            if (def.OnComplete != null)
            {
                ref TweenCallbackComponent callback = ref handle.Entity.Get<TweenCallbackComponent>();
                callback.OnComplete = def.OnComplete;
            }

            if (def.OnStart != null || def.OnUpdate != null)
            {
                ref TweenAdvancedCallbackComponent advCallback = ref handle.Entity.Get<TweenAdvancedCallbackComponent>();
                advCallback.OnStart = def.OnStart;
                advCallback.OnUpdate = def.OnUpdate;
                advCallback.StartCalled = true;
                advCallback.OnStart?.Invoke();
            }

            if (def.OnCancelled != null)
            {
                ref TweenCancelCallbackComponent cancelCallback = ref handle.Entity.Get<TweenCancelCallbackComponent>();
                cancelCallback.OnCancelled = def.OnCancelled;
            }
        }

        private void InitializeTweenLoop(TweenHandle handle, TweenDefinition def)
        {
            if (def.IsInfiniteLoop)
            {
                ref TweenLoopComponent loop = ref handle.Entity.Get<TweenLoopComponent>();
                loop.Mode = TweenLoopComponent.LoopMode.Infinite;
            }
            else if (def.LoopCount > 0)
            {
                ref TweenLoopComponent loop = ref handle.Entity.Get<TweenLoopComponent>();
                loop.Mode = TweenLoopComponent.LoopMode.Repeat;
                loop.LoopCount = def.LoopCount;
            }
        }
    }
}

