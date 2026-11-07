using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    public static class TweenControlOperations
    {
        public static void Pause(TweenHandle handle)
        {
            if (!handle.IsValid)
            {
                Debug.LogWarning("[UITweens] Pause() was called on a TweenHandle that was never assigned a tween.");
                return;
            }
            if (!handle.IsAlive)
            {
                Debug.LogWarning("[UITweens] Pause() was called on a stale TweenHandle — the tween has already completed or its entity was recycled.");
                return;
            }

            ref TweenControlComponent control = ref handle.Entity.Get<TweenCoreComponent>().Control;
            if (control.State != TweenControlComponent.ControlState.Playing)
                return;

            ref TweenClockComponent clock = ref handle.Entity.Get<TweenCoreComponent>().Clock;
            control.PausedElapsedTime = clock.ElapsedTime;
            control.State = TweenControlComponent.ControlState.Paused;
        }

        public static void Resume(TweenHandle handle)
        {
            if (!handle.IsValid)
            {
                Debug.LogWarning("[UITweens] Resume() was called on a TweenHandle that was never assigned a tween.");
                return;
            }
            if (!handle.IsAlive)
            {
                Debug.LogWarning("[UITweens] Resume() was called on a stale TweenHandle — the tween has already completed or its entity was recycled.");
                return;
            }

            ref TweenControlComponent control = ref handle.Entity.Get<TweenCoreComponent>().Control;
            if (control.State == TweenControlComponent.ControlState.Paused)
                control.State = TweenControlComponent.ControlState.Playing;
        }

        public static void Cancel(TweenHandle handle)
        {
            if (!handle.IsValid || !handle.IsAlive)
                return;

            ref TweenControlComponent control = ref handle.Entity.Get<TweenCoreComponent>().Control;
            if (control.State != TweenControlComponent.ControlState.Cancelled)
                control.State = TweenControlComponent.ControlState.Cancelled;
        }

        public static void Cancel(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref TweenControlComponent control = ref entity.Get<TweenCoreComponent>().Control;
            if (control.State != TweenControlComponent.ControlState.Cancelled)
                control.State = TweenControlComponent.ControlState.Cancelled;
        }

        public static void TogglePause(TweenHandle handle)
        {
            if (!handle.IsValid)
            {
                Debug.LogWarning("[UITweens] TogglePause() was called on a TweenHandle that was never assigned a tween.");
                return;
            }
            if (!handle.IsAlive)
            {
                Debug.LogWarning("[UITweens] TogglePause() was called on a stale TweenHandle — the tween has already completed or its entity was recycled.");
                return;
            }

            ref TweenControlComponent control = ref handle.Entity.Get<TweenCoreComponent>().Control;
            if (control.State == TweenControlComponent.ControlState.Playing)
                Pause(handle);
            else if (control.State == TweenControlComponent.ControlState.Paused)
                Resume(handle);
        }
    }
}

