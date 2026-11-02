using System;
using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    public class TweenBuilder
    {
        private readonly TweenSpawner spawner;
        private TweenHandle lastHandle;
        // NEW: Phase 8 - Track component for delayed update
        private UnityEngine.Object trackedComponent;

        internal TweenBuilder(TweenSpawner spawner)
        {
            this.spawner = spawner;
            lastHandle = new TweenHandle();
        }

        internal TweenBuilder SetHandle(TweenHandle handle)
        {
            lastHandle = handle;
            return this;
        }

        /// <summary>
        /// NEW: Phase 8 - Store reference to the component this tween is for.
        /// Needed so we can notify TweenManager when delay is set after initial tracking.
        /// </summary>
        internal TweenBuilder SetTrackedComponent(UnityEngine.Object component)
        {
            trackedComponent = component;
            return this;
        }

        public TweenBuilder Delay(float delaySeconds)
        {
            if (delaySeconds <= 0f || !lastHandle.IsValid || !lastHandle.Entity.IsAlive())
                return this;

            // NEW: Phase 10 - Unified state machine: Set delay on entity, transition from NotStarted to Delayed
            // Entity stays alive in pool with Delayed state. State machine handles delay progression.
            // No more external queueing - everything is integrated into ECS components.

            TweenInitializer initializer = spawner.GetInitializer();
            if (initializer != null)
            {
                initializer.SetDelay(lastHandle.Entity, delaySeconds);

                // NEW: Phase 14 - GetAndRemovePredecessor atomically retrieves and removes from pendingCancellations.
                // This prevents ProcessPendingCancellations from cancelling the predecessor prematurely.
                if (trackedComponent != null)
                {
                    TweenPropertyType propType = DetectPropertyType(lastHandle.Entity);

                    TweenHandle predecessorHandle = TweenManager.GetAndRemovePredecessor(trackedComponent, propType);

                    if (predecessorHandle.IsValid)
                    {
                        ref var control = ref lastHandle.Entity.Get<TweenControlComponent>();
                        control.PredecessorHandle = predecessorHandle;
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Helper to detect property type from entity components.
        /// </summary>
        private TweenPropertyType DetectPropertyType(EcsEntity entity)
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

        public TweenBuilder Loop(int repeatCount)
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref TweenLoopComponent loopComp = ref lastHandle.Entity.Get<TweenLoopComponent>();
                loopComp.Mode = TweenLoopComponent.LoopMode.Repeat;
                loopComp.LoopCount = repeatCount;
                loopComp.CurrentLoop = 0;
            }

            return this;
        }

        public TweenBuilder LoopInfinite()
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref var loopComp = ref lastHandle.Entity.Get<TweenLoopComponent>();
                loopComp.Mode = TweenLoopComponent.LoopMode.Infinite;
            }
            return this;
        }

        public TweenBuilder Then(TweenHandle nextHandle)
        {
            if (lastHandle.IsValid && nextHandle.IsValid)
                spawner.LinkTweens(lastHandle, nextHandle);

            return SetHandle(nextHandle);
        }

        public TweenBuilder OnComplete(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref TweenCallbackComponent cbComp = ref lastHandle.Entity.Get<TweenCallbackComponent>();
                cbComp.OnComplete = callback;
            }

            return this;
        }

        public TweenBuilder OnStart(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnStart = callback;
                advancedCb.StartCalled = false;
            }

            return this;
        }

        public TweenBuilder OnUpdate(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnUpdate = callback;
            }

            return this;
        }

        public TweenHandle GetHandle()
            => lastHandle;

        public ITweenDeclaration DeclareOnly()
        {
            return new DeclarationWrapper(this);
        }
    }
}
