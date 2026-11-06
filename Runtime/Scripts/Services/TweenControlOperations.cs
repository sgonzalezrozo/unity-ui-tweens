using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    public static class TweenControlOperations
    {
        public static void Pause(TweenHandle handle)
        {
            if (!handle.IsValid || !handle.IsAlive)
            {
                Debug.LogWarning("[UITweens] Cannot pause invalid or dead tween handle");
                return;
            }

            ref TweenControlComponent control = ref handle.Entity.Get<TweenControlComponent>();
            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            ref TweenClockComponent clock = ref handle.Entity.Get<TweenClockComponent>();
            control.PausedElapsedTime = clock.ElapsedTime;
            control.State = TweenControlComponent.ControlState.Paused;
        }

        public static void Resume(TweenHandle handle)
        {
            if (!handle.IsValid || !handle.IsAlive)
            {
                Debug.LogWarning("[UITweens] Cannot resume invalid or dead tween handle");
                return;
            }

            ref TweenControlComponent control = ref handle.Entity.Get<TweenControlComponent>();
            if (control.State == TweenControlComponent.ControlState.Paused)
                control.State = TweenControlComponent.ControlState.Playing;
        }

        public static void Cancel(TweenHandle handle)
        {
            if (!handle.IsValid || !handle.IsAlive)
                return;

            ref TweenControlComponent control = ref handle.Entity.Get<TweenControlComponent>();
            if (control.State != TweenControlComponent.ControlState.Cancelled)
                control.State = TweenControlComponent.ControlState.Cancelled;
        }

        public static void Cancel(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();
            if (control.State != TweenControlComponent.ControlState.Cancelled)
                control.State = TweenControlComponent.ControlState.Cancelled;
        }

        public static void TogglePause(TweenHandle handle)
        {
            if (!handle.IsValid || !handle.IsAlive)
            {
                Debug.LogWarning("[UITweens] Cannot toggle pause on invalid or dead tween handle");
                return;
            }

            ref TweenControlComponent control = ref handle.Entity.Get<TweenControlComponent>();
            if (control.State == TweenControlComponent.ControlState.Playing)
                Pause(handle);
            else if (control.State == TweenControlComponent.ControlState.Paused)
                Resume(handle);
        }
    }
}

