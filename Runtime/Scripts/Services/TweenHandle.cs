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

        /// <summary>True while the tween is playing.</summary>
        public bool IsPlaying
        {
            get
            {
                if (!IsValid || !Entity.IsAlive())
                    return false;

                ref var control = ref Entity.Get<TweenControlComponent>();
                return control.State == TweenControlComponent.ControlState.Playing;
            }
        }

        /// <summary>True while the tween is paused.</summary>
        public bool IsPaused
        {
            get
            {
                if (!IsValid || !Entity.IsAlive())
                    return false;

                ref var control = ref Entity.Get<TweenControlComponent>();
                return control.State == TweenControlComponent.ControlState.Paused;
            }
        }

        /// <summary>True once the tween has been cancelled.</summary>
        public bool IsCancelled
        {
            get
            {
                if (!IsValid || !Entity.IsAlive())
                    return false;

                ref var control = ref Entity.Get<TweenControlComponent>();
                return control.State == TweenControlComponent.ControlState.Cancelled;
            }
        }

        /// <summary>True once the tween has finished.</summary>
        public bool IsFinished
        {
            get
            {
                if (!IsValid || !Entity.IsAlive())
                    return false;

                ref var clock = ref Entity.Get<TweenClockComponent>();
                return clock.IsFinished;
            }
        }

        internal TweenHandle(EcsEntity entity)
        {
            Entity = entity;
            IsValid = true;
        }
    }
}

