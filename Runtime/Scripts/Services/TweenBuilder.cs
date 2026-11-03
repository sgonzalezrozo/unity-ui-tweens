using System;
using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    /// <summary>Fluent API for configuring a tween that has already been created.</summary>
    public class TweenBuilder
    {
        private readonly TweenSpawner spawner;
        private TweenHandle lastHandle;
        // Track component for delayed update
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
        /// Store reference to the component this tween is for.
        /// Needed so we can notify TweenManager when delay is set after initial tracking.
        /// </summary>
        internal TweenBuilder SetTrackedComponent(UnityEngine.Object component)
        {
            trackedComponent = component;
            return this;
        }

        /// <summary>Waits before starting.</summary>
        public TweenBuilder Delay(float delaySeconds)
        {
            if (delaySeconds <= 0f || !lastHandle.IsValid || !lastHandle.Entity.IsAlive())
                return this;

            // Unified state machine: Set delay on entity, transition from NotStarted to Delayed
            // Entity stays alive in pool with Delayed state. State machine handles delay progression.
            // No more external queueing - everything is integrated into ECS components.

            TweenInitializer initializer = spawner.GetInitializer();
            if (initializer != null)
            {
                initializer.SetDelay(lastHandle.Entity, delaySeconds);

                // GetAndRemovePredecessor atomically retrieves and removes from pendingCancellations.
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

        /// <summary>Repeats after the first play, so Loop(2) plays three times.</summary>
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

        /// <summary>Repeats until cancelled.</summary>
        public TweenBuilder LoopInfinite()
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref var loopComp = ref lastHandle.Entity.Get<TweenLoopComponent>();
                loopComp.Mode = TweenLoopComponent.LoopMode.Infinite;
            }
            return this;
        }

        /// <summary>Links another tween. The linked tween does not wait yet, so use OnComplete to run tweens in order.</summary>
        public TweenBuilder Then(TweenHandle nextHandle)
        {
            if (lastHandle.IsValid && nextHandle.IsValid)
                spawner.LinkTweens(lastHandle, nextHandle);

            return SetHandle(nextHandle);
        }

        /// <summary>Runs code when the tween finishes.</summary>
        public TweenBuilder OnComplete(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref TweenCallbackComponent cbComp = ref lastHandle.Entity.Get<TweenCallbackComponent>();
                cbComp.OnComplete = callback;
            }

            return this;
        }

        /// <summary>Runs code when the tween starts.</summary>
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

        /// <summary>Runs code on each update.</summary>
        public TweenBuilder OnUpdate(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.Entity.IsAlive())
            {
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnUpdate = callback;
            }

            return this;
        }

        /// <summary>Gets a handle for controlling this tween later.</summary>
        public TweenHandle GetHandle()
            => lastHandle;

        /// <summary>Wraps this tween for Sequential(). The tween is already running, so it does not defer it.</summary>
        public ITweenDeclaration DeclareOnly()
        {
            return new DeclarationWrapper(this);
        }
    }
}
