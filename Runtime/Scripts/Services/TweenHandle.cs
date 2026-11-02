using Leopotam.Ecs;
using UITweens.Components;

namespace UITweens.Services
{
    public struct TweenHandle
    {
        public bool IsValid { get; private set; }
        internal EcsEntity Entity { get; private set; }

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

