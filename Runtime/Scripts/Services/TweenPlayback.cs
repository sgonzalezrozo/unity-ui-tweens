using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>Complete(), Restart(), Rewind() and what happens when a tween's GameObject is disabled.</summary>
    internal static class TweenPlayback
    {
        /// <summary>
        /// Jumps to the end and finishes on the next update, which runs OnComplete and starts the next
        /// tween in a sequence. Finite loops end where their last play would; infinite loops end at the target.
        /// </summary>
        internal static void Complete(EcsEntity entity)
        {
            if (!CanControl(entity))
                return;

            ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenControlComponent control = ref core.Control;

            if (clock.IsRelative && !clock.ValuesResolved)
                TweenValueModifiers.ResolveRelative(entity);

            if (entity.Has<TweenLoopComponent>())
            {
                ref TweenLoopComponent loop = ref entity.Get<TweenLoopComponent>();
                bool infinite = loop.Mode == TweenLoopComponent.LoopMode.Infinite;
                int plays = loop.LoopCount + 1;

                // A Yoyo loop ends backward when its last play is an even-numbered one.
                bool endBackward = loop.Yoyo && !infinite && (plays - 1) % 2 == 1;
                if (loop.Yoyo && endBackward != loop.ShouldReverse)
                    TweenValueModifiers.Swap(entity);

                loop.ShouldReverse = endBackward;
                clock.Backward = endBackward;
                loop.Mode = TweenLoopComponent.LoopMode.None;
            }

            // A delayed tween that replaces another one normally cancels it when the delay ends.
            if (control.PredecessorHandle.IsValid && !control.HasKilledCompetitors)
            {
                TweenManager.Cancel(control.PredecessorHandle);
                control.HasKilledCompetitors = true;
            }

            clock.ElapsedDelay = clock.DelayDuration;
            clock.ElapsedTime = clock.Duration;
            clock.PausedByDisable = false;
            control.State = TweenControlComponent.ControlState.Playing;

            TweenValueModifiers.ShowValue(entity, end: true);
        }

        /// <summary>Goes back to the start and plays again, with the delay unless includeDelay is false.</summary>
        internal static void Restart(EcsEntity entity, bool includeDelay)
        {
            if (!CanControl(entity))
                return;

            ResetToStart(entity);

            ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();
            bool delayed = includeDelay && core.Clock.HasDelay;
            core.Clock.ElapsedDelay = delayed ? 0f : core.Clock.DelayDuration;
            core.Control.State = delayed
                ? TweenControlComponent.ControlState.Delayed
                : TweenControlComponent.ControlState.Playing;
        }

        /// <summary>Goes back to the start and pauses. Resume() plays it from there, without the delay.</summary>
        internal static void Rewind(EcsEntity entity)
        {
            if (!CanControl(entity))
                return;

            ResetToStart(entity);

            ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();
            core.Clock.ElapsedDelay = core.Clock.DelayDuration;
            core.Control.PausedElapsedTime = 0f;
            core.Control.State = TweenControlComponent.ControlState.Paused;
        }

        /// <summary>Pauses, cancels or completes a tween whose GameObject was disabled, as its DisableBehaviour says.</summary>
        internal static void ApplyDisableBehaviour(EcsEntity entity, ref TweenCoreComponent core)
        {
            RectTransform rect = core.Target.RectTransform;
            if (rect == null)
                return;

            bool active = rect.gameObject.activeInHierarchy;
            ref TweenClockComponent clock = ref core.Clock;
            ref TweenControlComponent control = ref core.Control;

            switch (clock.DisableBehaviour)
            {
                case DisableBehaviour.Pause:
                    if (!active && !clock.PausedByDisable && IsRunning(control.State))
                    {
                        clock.ResumeState = control.State;
                        clock.PausedByDisable = true;
                        control.State = TweenControlComponent.ControlState.Paused;
                    }
                    else if (active && clock.PausedByDisable)
                    {
                        clock.PausedByDisable = false;
                        if (control.State == TweenControlComponent.ControlState.Paused)
                            control.State = clock.ResumeState;
                    }
                    break;

                case DisableBehaviour.Cancel:
                    if (!active)
                        TweenControlOperations.Cancel(entity);
                    break;

                case DisableBehaviour.Complete:
                    if (!active && control.State != TweenControlComponent.ControlState.Cancelled)
                        Complete(entity);
                    break;
            }
        }

        private static void ResetToStart(EcsEntity entity)
        {
            ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();
            ref TweenClockComponent clock = ref core.Clock;

            if (clock.IsRelative && !clock.ValuesResolved)
                TweenValueModifiers.ResolveRelative(entity);

            if (entity.Has<TweenLoopComponent>())
            {
                ref TweenLoopComponent loop = ref entity.Get<TweenLoopComponent>();
                if (loop.Yoyo && loop.ShouldReverse)
                    TweenValueModifiers.Swap(entity);

                loop.ShouldReverse = false;
                loop.CurrentLoop = 0;
            }

            clock.Backward = false;
            clock.ElapsedTime = 0f;
            clock.PausedByDisable = false;

            TweenValueModifiers.ShowValue(entity, end: false);
        }

        private static bool CanControl(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return false;

            TweenControlComponent.ControlState state = entity.Get<TweenCoreComponent>().Control.State;
            return state == TweenControlComponent.ControlState.NotStarted
                   || state == TweenControlComponent.ControlState.Delayed
                   || state == TweenControlComponent.ControlState.Playing
                   || state == TweenControlComponent.ControlState.Paused;
        }

        private static bool IsRunning(TweenControlComponent.ControlState state)
            => state == TweenControlComponent.ControlState.NotStarted
               || state == TweenControlComponent.ControlState.Delayed
               || state == TweenControlComponent.ControlState.Playing;
    }
}
