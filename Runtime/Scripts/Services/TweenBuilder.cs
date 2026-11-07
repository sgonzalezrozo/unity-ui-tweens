using System;
using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    /// <summary>
    /// Fluent API for configuring a tween that has already been created.
    /// It's a struct so that creating a tween doesn't allocate.
    /// </summary>
    public struct TweenBuilder
    {
        private readonly TweenSpawner spawner;
        private TweenHandle lastHandle;
        // Track component for delayed update
        private UnityEngine.Object trackedComponent;

        internal TweenBuilder(TweenSpawner spawner)
        {
            this.spawner = spawner;
            lastHandle = new TweenHandle();
            trackedComponent = null;
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
        /// <summary>
        /// Marks this tween as starting from the UI's current value, so a tween that
        /// waits in a sequence reads that value when it starts, not when it's created.
        /// </summary>
        internal TweenBuilder StartFromCurrent(byte axisMask = TweenClockComponent.ALL_AXES)
        {
            if (!lastHandle.IsValid || !lastHandle.IsAlive)
                return this;

            ref TweenClockComponent clock = ref lastHandle.Entity.Get<TweenCoreComponent>().Clock;
            clock.StartFromCurrent = true;
            clock.AxisMask = axisMask;
            return this;
        }

        public TweenBuilder Delay(float delaySeconds)
        {
            if (delaySeconds <= 0f || !lastHandle.IsValid || !lastHandle.IsAlive)
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
                        ref var control = ref lastHandle.Entity.Get<TweenCoreComponent>().Control;
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

        /// <summary>
        /// Repeats after the first play, so Loop(2) plays three times. With LoopType.Yoyo it plays
        /// back and forth, so an odd repeat count ends where it started.
        /// </summary>
        public TweenBuilder Loop(int repeatCount, LoopType type = LoopType.Restart)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                TweenInitializer.EnsureLoopComponent(lastHandle.Entity);
                ref TweenLoopComponent loopComp = ref lastHandle.Entity.Get<TweenLoopComponent>();
                loopComp.Mode = TweenLoopComponent.LoopMode.Repeat;
                loopComp.LoopCount = repeatCount;
                loopComp.CurrentLoop = 0;
                loopComp.Yoyo = type == LoopType.Yoyo;
            }

            return this;
        }

        /// <summary>Repeats until cancelled. With LoopType.Yoyo it plays back and forth.</summary>
        public TweenBuilder LoopInfinite(LoopType type = LoopType.Restart)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                TweenInitializer.EnsureLoopComponent(lastHandle.Entity);
                ref var loopComp = ref lastHandle.Entity.Get<TweenLoopComponent>();
                loopComp.Mode = TweenLoopComponent.LoopMode.Infinite;
                loopComp.Yoyo = type == LoopType.Yoyo;
            }
            return this;
        }

        /// <summary>
        /// Plays from the given value to the value the target has now, instead of the other way
        /// round. The target jumps to the given value straight away, even if the tween is delayed:
        /// panel.DOScale(Vector3.zero, 0.3f, EaseType.BackOut).From();
        /// </summary>
        public TweenBuilder From()
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
                TweenValueModifiers.ApplyFrom(lastHandle.Entity);

            return this;
        }

        /// <summary>
        /// Treats the given value as an offset from the value the tween starts at, so
        /// rect.DOAnchorPosX(50f, 0.3f).Relative() moves 50 units right from wherever it is
        /// when it starts. Works with From() too.
        /// </summary>
        public TweenBuilder Relative()
        {
            if (!lastHandle.IsValid || !lastHandle.IsAlive)
                return this;

            ref TweenClockComponent clock = ref lastHandle.Entity.Get<TweenCoreComponent>().Clock;
            if (clock.IsRelative)
                return this;

            clock.IsRelative = true;

            // A From() tween's values are already fixed, so its offset is applied now.
            if (clock.IsFrom)
            {
                TweenValueModifiers.ResolveRelative(lastHandle.Entity);
                TweenValueModifiers.ShowStartValue(lastHandle.Entity);
            }

            return this;
        }

        /// <summary>
        /// Advances with unscaled time, so the tween keeps playing when Time.timeScale is 0,
        /// for example on a pause menu. See also TweenManager.IgnoreTimeScaleByDefault.
        /// </summary>
        public TweenBuilder IgnoreTimeScale(bool ignore = true)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
                lastHandle.Entity.Get<TweenCoreComponent>().Clock.IgnoreTimeScale = ignore;

            return this;
        }

        /// <summary>
        /// Runs another tween when this one finishes. The next tween waits, and reads its
        /// start value when it starts. Cancelling this tween also cancels the next one.
        /// Returns a builder for the next tween, so calls can be chained.
        /// </summary>
        public TweenBuilder Then(TweenHandle nextHandle)
        {
            if (lastHandle.IsValid && nextHandle.IsValid)
                spawner.LinkTweens(lastHandle, nextHandle);

            trackedComponent = null;
            return SetHandle(nextHandle);
        }

        /// <summary>Runs code when the tween finishes.</summary>
        public TweenBuilder OnComplete(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                ref TweenCallbackComponent cbComp = ref lastHandle.Entity.Get<TweenCallbackComponent>();
                cbComp.OnComplete = callback;
                cbComp.StateCallback = null;
                cbComp.State = null;
                cbComp.StateInvoker = null;
            }

            return this;
        }

        /// <summary>
        /// Runs code when the tween finishes, and passes it a value. Unlike a closure, this
        /// doesn't allocate if the callback doesn't capture anything:
        /// tween.OnComplete(rect, static target => target.gameObject.SetActive(false));
        /// </summary>
        public TweenBuilder OnComplete<TState>(TState state, Action<TState> callback) where TState : class
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                ref TweenCallbackComponent cbComp = ref lastHandle.Entity.Get<TweenCallbackComponent>();
                cbComp.OnComplete = null;
                cbComp.StateCallback = callback;
                cbComp.State = state;
                cbComp.StateInvoker = TweenStateInvoker<TState>.Invoke;
            }

            return this;
        }

        /// <summary>Runs code when the tween starts.</summary>
        public TweenBuilder OnStart(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                TweenInitializer.EnsureAdvancedCallbackComponent(lastHandle.Entity);
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnStart = callback;
                advancedCb.StartCalled = false;
            }

            return this;
        }

        /// <summary>Runs code on each update.</summary>
        public TweenBuilder OnUpdate(Action callback)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                TweenInitializer.EnsureAdvancedCallbackComponent(lastHandle.Entity);
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnUpdate = callback;
            }

            return this;
        }

        /// <summary>Runs code with the current value of a CustomFloat tween on each update.</summary>
        public TweenBuilder OnFloatUpdate(Action<float> callback)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                TweenInitializer.EnsureAdvancedCallbackComponent(lastHandle.Entity);
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnFloatUpdate = callback;
            }

            return this;
        }

        /// <summary>Runs code with the current value of a CustomVector tween on each update.</summary>
        public TweenBuilder OnVectorUpdate(Action<UnityEngine.Vector3> callback)
        {
            if (lastHandle.IsValid && lastHandle.IsAlive)
            {
                TweenInitializer.EnsureAdvancedCallbackComponent(lastHandle.Entity);
                ref TweenAdvancedCallbackComponent advancedCb = ref lastHandle.Entity.Get<TweenAdvancedCallbackComponent>();
                advancedCb.OnVectorUpdate = callback;
            }

            return this;
        }

        /// <summary>Gets a handle for controlling this tween later.</summary>
        public TweenHandle GetHandle()
            => lastHandle;

        /// <summary>
        /// Holds this tween so it doesn't start, and wraps it for TweenSpawner.Sequential().
        /// A declared tween stays held until it's passed to Sequential().
        /// </summary>
        public TweenDeclaration DeclareOnly()
        {
            if (lastHandle.IsValid)
                TweenSequencer.Hold(lastHandle.Entity);

            return new TweenDeclaration(this);
        }
    }

    /// <summary>Calls a stored Action&lt;TState&gt; without knowing TState. Created once per type.</summary>
    internal static class TweenStateInvoker<TState> where TState : class
    {
        public static readonly Action<Delegate, object> Invoke =
            (callback, state) => ((Action<TState>)callback).Invoke((TState)state);
    }
}
