using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    /// <summary>A reference to one tween.</summary>
    public struct TweenHandle
    {
        /// <summary>True when this handle refers to a tween.</summary>
        public bool IsValid { get; private set; }
        internal EcsEntity Entity { get; private set; }

        // Entities are reused, so a handle only matches the tween it was created for.
        private int serial;

        /// <summary>True while this handle's tween still exists.</summary>
        internal bool IsAlive
            => IsValid && Entity.IsAlive() && Entity.Get<TweenCoreComponent>().Control.Serial == serial;

        /// <summary>True while the tween is playing.</summary>
        public bool IsPlaying
        {
            get
            {
                if (!IsAlive)
                    return false;

                ref var control = ref Entity.Get<TweenCoreComponent>().Control;
                return control.State == TweenControlComponent.ControlState.Playing;
            }
        }

        /// <summary>True while the tween is paused.</summary>
        public bool IsPaused
        {
            get
            {
                if (!IsAlive)
                    return false;

                ref var control = ref Entity.Get<TweenCoreComponent>().Control;
                return control.State == TweenControlComponent.ControlState.Paused;
            }
        }

        /// <summary>True once the tween has been cancelled.</summary>
        public bool IsCancelled
        {
            get
            {
                if (!IsAlive)
                    return false;

                ref var control = ref Entity.Get<TweenCoreComponent>().Control;
                return control.State == TweenControlComponent.ControlState.Cancelled;
            }
        }

        /// <summary>True once the tween has finished.</summary>
        public bool IsFinished
        {
            get
            {
                if (!IsAlive)
                    return false;

                ref var clock = ref Entity.Get<TweenCoreComponent>().Clock;
                return clock.IsFinished;
            }
        }

        /// <summary>The current value of a CustomFloat tween, or 0 once it has ended.</summary>
        public float FloatValue
        {
            get
            {
                if (!IsAlive || !Entity.Has<TweenCustomFloatComponent>())
                    return 0f;

                return Entity.Get<TweenCustomFloatComponent>().CurrentValue;
            }
        }

        /// <summary>The current value of a CustomVector tween, or zero once it has ended.</summary>
        public UnityEngine.Vector3 VectorValue
        {
            get
            {
                if (!IsAlive || !Entity.Has<TweenCustomVectorComponent>())
                    return UnityEngine.Vector3.zero;

                return Entity.Get<TweenCustomVectorComponent>().CurrentValue;
            }
        }

        internal TweenHandle(EcsEntity entity)
        {
            Entity = entity;
            IsValid = true;
            serial = entity.Get<TweenCoreComponent>().Control.Serial;
        }
    }
}

