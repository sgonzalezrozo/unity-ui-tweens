using System.Collections.Generic;
using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    public class TweenTracker
    {
        // OPTIMIZATION: Pre-size collections to avoid resizing overhead (5-10ms gain)
        private Dictionary<(int, TweenPropertyType), TweenHandle> componentPropertyTweenHandles = new(256);
        private Dictionary<(int, TweenPropertyType), TweenHandle> pendingCancellations = new(64);
        private List<(int, TweenPropertyType)> cachedKeysToRemoveList = new(64);
        private List<(int, TweenPropertyType)> cachedPendingKeysToProcess = new(64);

        public int TrackedTweenCount
            => componentPropertyTweenHandles.Count;

        public void TrackTween(UnityEngine.Object component, TweenHandle handle)
        {
            if (component == null || !handle.IsValid)
                return;

            int componentId = component.GetInstanceID();
            TweenPropertyType propertyType = DetectPropertyType(handle);
            (int, TweenPropertyType) key = (componentId, propertyType);

            bool hasPrevious = componentPropertyTweenHandles.TryGetValue(key, out TweenHandle previousHandle);
            bool isDifferentTween = !hasPrevious || previousHandle.Entity != handle.Entity;

            if (hasPrevious && previousHandle.IsValid && isDifferentTween)
                pendingCancellations[key] = previousHandle;

            componentPropertyTweenHandles[key] = handle;
        }

        public void ProcessPendingCancellations()
        {
            if (pendingCancellations.Count == 0)
                return;

            cachedPendingKeysToProcess.Clear();
            cachedKeysToRemoveList.Clear();

            cachedPendingKeysToProcess.AddRange(pendingCancellations.Keys);

            for (int i = 0; i < cachedPendingKeysToProcess.Count; i++)
            {
                (int, TweenPropertyType) key = cachedPendingKeysToProcess[i];
                TweenHandle handle = pendingCancellations[key];

                if (!handle.IsValid || !handle.Entity.IsAlive())
                {
                    cachedKeysToRemoveList.Add(key);
                    continue;
                }

                ref TweenControlComponent control = ref handle.Entity.Get<TweenControlComponent>();

                if (control.State == TweenControlComponent.ControlState.Delayed)
                {
                    cachedKeysToRemoveList.Add(key);
                    continue;
                }

                TweenControlOperations.Cancel(handle);
                cachedKeysToRemoveList.Add(key);
            }

            for (int i = 0; i < cachedKeysToRemoveList.Count; i++)
                pendingCancellations.Remove(cachedKeysToRemoveList[i]);
        }

        public bool TryGetTween(UnityEngine.Object component, TweenPropertyType propertyType, out TweenHandle handle)
        {
            handle = new TweenHandle();
            if (component == null)
                return false;

            return componentPropertyTweenHandles.TryGetValue((component.GetInstanceID(), propertyType), out handle);
        }

        public bool TryGetAnyTween(UnityEngine.Object component, out TweenHandle handle)
        {
            handle = new TweenHandle();
            if (component == null)
                return false;

            int componentId = component.GetInstanceID();
            foreach (KeyValuePair<(int, TweenPropertyType), TweenHandle> kvp in componentPropertyTweenHandles)
            {
                if (kvp.Key.Item1 == componentId)
                {
                    handle = kvp.Value;
                    return true;
                }
            }

            return false;
        }

        public void RemoveTween(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (component == null)
                return;

            componentPropertyTweenHandles.Remove((component.GetInstanceID(), propertyType));
        }

        public void ClearAllTweensForComponent(UnityEngine.Object component)
        {
            if (component == null)
                return;

            int componentId = component.GetInstanceID();
            List<(int, TweenPropertyType)> keysToRemove = new();

            foreach (KeyValuePair<(int, TweenPropertyType), TweenHandle> kvp in componentPropertyTweenHandles)
            {
                if (kvp.Key.Item1 == componentId)
                    keysToRemove.Add(kvp.Key);
            }

            for (int i = 0; i < keysToRemove.Count; i++)
                componentPropertyTweenHandles.Remove(keysToRemove[i]);
        }

        public void CancelAllTweensForComponent(int componentId)
        {
            List<(int, TweenPropertyType)> keysToRemove = new();

            foreach (KeyValuePair<(int, TweenPropertyType), TweenHandle> kvp in componentPropertyTweenHandles)
            {
                if (kvp.Key.Item1 == componentId && kvp.Value.IsValid)
                {
                    TweenControlOperations.Cancel(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
                componentPropertyTweenHandles.Remove(keysToRemove[i]);
        }

        public void RemoveFromPendingCancellations(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (component == null)
                return;

            pendingCancellations.Remove((component.GetInstanceID(), propertyType));
        }

        public TweenHandle GetAndRemovePredecessor(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (component == null)
                return new TweenHandle();

            (int, TweenPropertyType) key = (component.GetInstanceID(), propertyType);

            if (pendingCancellations.TryGetValue(key, out TweenHandle predecessorHandle))
            {
                pendingCancellations.Remove(key);
                return predecessorHandle;
            }

            return new TweenHandle();
        }

        public void KillCompetitorTweens(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
            if (target.RectTransform == null)
                return;

            TweenPropertyType propertyType = DetectPropertyType(entity);
            if (propertyType == TweenPropertyType.Unknown)
                return;

            int componentId = target.RectTransform.GetInstanceID();
            (int, TweenPropertyType) key = (componentId, propertyType);

            int killedCount = 0;
            List<(int, TweenPropertyType)> keysToRemove = new();

            foreach (KeyValuePair<(int, TweenPropertyType), TweenHandle> kvp in componentPropertyTweenHandles)
            {
                if (kvp.Key == key && kvp.Value.Entity != entity && kvp.Value.IsValid)
                {
                    TweenControlOperations.Cancel(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                    killedCount++;
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
                componentPropertyTweenHandles.Remove(keysToRemove[i]);

        }

        public Dictionary<(int, TweenPropertyType), TweenHandle> GetAllTrackedTweens()
            => componentPropertyTweenHandles;

        public void Cleanup()
        {
            cachedKeysToRemoveList?.Clear();
            cachedPendingKeysToProcess?.Clear();
        }

        public TweenPropertyType DetectPropertyType(TweenHandle handle)
        {
            if (!handle.IsValid || !handle.Entity.IsAlive())
                return TweenPropertyType.Unknown;

            return DetectPropertyType(handle.Entity);
        }

        public TweenPropertyType DetectPropertyType(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return TweenPropertyType.Unknown;

            if (entity.Has<TweenAlphaComponent>())
                return TweenPropertyType.Alpha;
            if (entity.Has<TweenPositionComponent>())
                return TweenPropertyType.Position;
            if (entity.Has<TweenScaleComponent>())
                return TweenPropertyType.Scale;
            if (entity.Has<TweenRotationComponent>())
                return TweenPropertyType.Rotation;
            if (entity.Has<TweenColorComponent>())
                return TweenPropertyType.Color;
            if (entity.Has<TweenSizeDeltaComponent>())
                return TweenPropertyType.SizeDelta;
            if (entity.Has<TweenPivotComponent>())
                return TweenPropertyType.Pivot;

            return TweenPropertyType.Unknown;
        }
    }
}
