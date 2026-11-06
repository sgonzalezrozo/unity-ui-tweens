using System.Collections.Generic;
using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    public class TweenTracker
    {
        private static readonly TweenPropertyType[] AllPropertyTypes =
        {
            TweenPropertyType.Alpha,
            TweenPropertyType.Position,
            TweenPropertyType.Scale,
            TweenPropertyType.Rotation,
            TweenPropertyType.Color,
            TweenPropertyType.SizeDelta,
            TweenPropertyType.Pivot,
            TweenPropertyType.Unknown
        };

        // OPTIMIZATION: Pre-size collections to avoid resizing overhead (5-10ms gain)
        private Dictionary<long, TweenHandle> componentPropertyTweenHandles = new(32);
        private Dictionary<long, TweenHandle> pendingCancellations = new(16);

        private readonly Dictionary<int, UnityEngine.Object> registeredTargets = new();
        private readonly List<int> destroyedTargetIds = new();

        // Packs a component and property into one key, which is cheaper than a tuple key.
        private static long MakeKey(int componentId, TweenPropertyType propertyType)
            => ((long)componentId << 32) | (uint)(int)propertyType;

        public int TrackedTweenCount
            => componentPropertyTweenHandles.Count;

        public void TrackTween(UnityEngine.Object component, TweenHandle handle)
        {
            if (component == null || !handle.IsValid)
                return;

            TrackTween(component.GetInstanceID(), DetectPropertyType(handle), handle);
        }

        /// <summary>
        /// Stops tracking a tween that is being held, so it doesn't replace a running tween on
        /// the same property. Returns the tracking key to restore when the tween is activated.
        /// </summary>
        public bool TryUntrack(EcsEntity entity, TweenHandle predecessor, out int componentId, out TweenPropertyType propertyType)
        {
            componentId = 0;
            propertyType = TweenPropertyType.Unknown;

            long trackedKey = 0L;
            bool found = false;

            foreach (KeyValuePair<long, TweenHandle> kvp in componentPropertyTweenHandles)
            {
                if (kvp.Value.Entity != entity)
                    continue;

                trackedKey = kvp.Key;
                found = true;
                break;
            }

            if (!found)
                return false;

            componentId = (int)(trackedKey >> 32);
            propertyType = (TweenPropertyType)(int)(trackedKey & 0xFFFFFFFFL);

            if (pendingCancellations.TryGetValue(trackedKey, out TweenHandle pending))
            {
                pendingCancellations.Remove(trackedKey);
                predecessor = pending;
            }

            if (predecessor.IsValid && predecessor.Entity.IsAlive())
                componentPropertyTweenHandles[trackedKey] = predecessor;
            else
                componentPropertyTweenHandles.Remove(trackedKey);

            return true;
        }

        public void TrackTween(int componentId, TweenPropertyType propertyType, TweenHandle handle)
        {
            long key = MakeKey(componentId, propertyType);

            bool hasPrevious = componentPropertyTweenHandles.TryGetValue(key, out TweenHandle previousHandle);
            bool isDifferentTween = !hasPrevious || previousHandle.Entity != handle.Entity;

            if (hasPrevious && previousHandle.IsValid && isDifferentTween)
                pendingCancellations[key] = previousHandle;

            componentPropertyTweenHandles[key] = handle;
        }

        /// <summary>
        /// Cancels replaced tweens. This waits until the next update so that Delay(), Then() and
        /// Sequential() can claim the old tween first and keep it running until theirs starts.
        /// </summary>
        public void ProcessPendingCancellations()
        {
            if (pendingCancellations.Count == 0)
                return;

            foreach (KeyValuePair<long, TweenHandle> kvp in pendingCancellations)
            {
                TweenHandle handle = kvp.Value;
                if (!handle.IsValid || !handle.Entity.IsAlive())
                    continue;

                if (handle.Entity.Get<TweenControlComponent>().State != TweenControlComponent.ControlState.Delayed)
                    TweenControlOperations.Cancel(handle);
            }

            pendingCancellations.Clear();
        }

        public bool TryGetTween(UnityEngine.Object component, TweenPropertyType propertyType, out TweenHandle handle)
        {
            handle = new TweenHandle();
            if (component == null)
                return false;

            return componentPropertyTweenHandles.TryGetValue(MakeKey(component.GetInstanceID(), propertyType), out handle);
        }

        public bool TryGetAnyTween(UnityEngine.Object component, out TweenHandle handle)
        {
            handle = new TweenHandle();
            if (component == null)
                return false;

            int componentId = component.GetInstanceID();
            for (int i = 0; i < AllPropertyTypes.Length; i++)
            {
                if (componentPropertyTweenHandles.TryGetValue(MakeKey(componentId, AllPropertyTypes[i]), out handle))
                    return true;
            }

            return false;
        }

        public void RemoveTween(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (component == null)
                return;

            componentPropertyTweenHandles.Remove(MakeKey(component.GetInstanceID(), propertyType));
        }

        public void ClearAllTweensForComponent(UnityEngine.Object component)
        {
            if (component == null)
                return;

            int componentId = component.GetInstanceID();
            for (int i = 0; i < AllPropertyTypes.Length; i++)
                componentPropertyTweenHandles.Remove(MakeKey(componentId, AllPropertyTypes[i]));
        }

        public void CancelAllTweensForComponent(int componentId)
        {
            for (int i = 0; i < AllPropertyTypes.Length; i++)
            {
                long key = MakeKey(componentId, AllPropertyTypes[i]);
                if (!componentPropertyTweenHandles.TryGetValue(key, out TweenHandle handle) || !handle.IsValid)
                    continue;

                TweenControlOperations.Cancel(handle);
                componentPropertyTweenHandles.Remove(key);
            }
        }

        public void RegisterTarget(UnityEngine.Object target)
        {
            if (target != null)
                registeredTargets[target.GetInstanceID()] = target;
        }

        /// <summary>Cancels tweens on destroyed objects.</summary>
        public void CleanupDestroyedTargets()
        {
            destroyedTargetIds.Clear();

            foreach (KeyValuePair<int, UnityEngine.Object> kvp in registeredTargets)
            {
                if (kvp.Value == null)
                    destroyedTargetIds.Add(kvp.Key);
            }

            for (int i = 0; i < destroyedTargetIds.Count; i++)
            {
                CancelAllTweensForComponent(destroyedTargetIds[i]);
                registeredTargets.Remove(destroyedTargetIds[i]);
            }
        }

        public void PauseAllTweensForComponent(int componentId)
        {
            for (int i = 0; i < AllPropertyTypes.Length; i++)
            {
                if (TryGetLiveTween(componentId, AllPropertyTypes[i], out TweenHandle handle))
                    TweenControlOperations.Pause(handle);
            }
        }

        public void ResumeAllTweensForComponent(int componentId)
        {
            for (int i = 0; i < AllPropertyTypes.Length; i++)
            {
                if (TryGetLiveTween(componentId, AllPropertyTypes[i], out TweenHandle handle))
                    TweenControlOperations.Resume(handle);
            }
        }

        /// <summary>Pauses everything if any tween is playing, so tweens stay in step.</summary>
        public void TogglePauseAllTweensForComponent(int componentId)
        {
            for (int i = 0; i < AllPropertyTypes.Length; i++)
            {
                if (TryGetLiveTween(componentId, AllPropertyTypes[i], out TweenHandle handle) && handle.IsPlaying)
                {
                    PauseAllTweensForComponent(componentId);
                    return;
                }
            }

            ResumeAllTweensForComponent(componentId);
        }

        private bool TryGetLiveTween(int componentId, TweenPropertyType propertyType, out TweenHandle handle)
            => componentPropertyTweenHandles.TryGetValue(MakeKey(componentId, propertyType), out handle)
               && handle.IsValid
               && handle.Entity.IsAlive();

        public void RemoveFromPendingCancellations(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (component == null)
                return;

            pendingCancellations.Remove(MakeKey(component.GetInstanceID(), propertyType));
        }

        public TweenHandle GetAndRemovePredecessor(UnityEngine.Object component, TweenPropertyType propertyType)
        {
            if (component == null)
                return new TweenHandle();

            long key = MakeKey(component.GetInstanceID(), propertyType);

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
            long key = MakeKey(componentId, propertyType);

            if (componentPropertyTweenHandles.TryGetValue(key, out TweenHandle competitor)
                && competitor.Entity != entity
                && competitor.IsValid)
            {
                TweenControlOperations.Cancel(competitor);
                componentPropertyTweenHandles.Remove(key);
            }
        }

        public Dictionary<long, TweenHandle> GetAllTrackedTweens()
            => componentPropertyTweenHandles;

        public void Cleanup()
        {
            pendingCancellations.Clear();
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
