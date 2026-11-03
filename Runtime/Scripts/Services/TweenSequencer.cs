using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Runs tweens one after another. A tween starts as soon as it's created, so a tween in a
    /// sequence is held in the Waiting state until the tween before it finishes.
    /// </summary>
    internal static class TweenSequencer
    {
        /// <summary>Stops a tween that hasn't started from running until it's activated.</summary>
        internal static bool Hold(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return false;

            ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();

            switch (control.State)
            {
                case TweenControlComponent.ControlState.Waiting:
                    return true;

                case TweenControlComponent.ControlState.NotStarted:
                case TweenControlComponent.ControlState.Delayed:
                    break;

                default:
                    Debug.LogWarning("[UITweens] Can't hold a tween that has already started");
                    return false;
            }

            TweenHandle predecessor = control.PredecessorHandle;
            control.PredecessorHandle = default;

            control.HasTrackingKey = TweenManager.TryUntrackTween(
                entity,
                predecessor,
                out control.TrackedComponentId,
                out control.TrackedPropertyType);

            control.State = TweenControlComponent.ControlState.Waiting;
            return true;
        }

        /// <summary>Starts a held tween from the UI's current value.</summary>
        internal static void Activate(EcsEntity entity)
        {
            if (!entity.IsAlive())
                return;

            ref TweenControlComponent control = ref entity.Get<TweenControlComponent>();
            if (control.State != TweenControlComponent.ControlState.Waiting)
                return;

            control.State = TweenControlComponent.ControlState.NotStarted;
            CaptureStartValues(entity);

            if (control.HasTrackingKey)
                TweenManager.TrackTween(control.TrackedComponentId, control.TrackedPropertyType, new TweenHandle(entity));
        }

        /// <summary>Makes the second tween wait for the end of the first tween's chain.</summary>
        internal static void Link(EcsEntity first, EcsEntity second)
        {
            if (!first.IsAlive() || !Hold(second))
                return;

            EcsEntity last = first;
            while (last.Has<TweenSequenceLinkComponent>()
                   && last.Get<TweenSequenceLinkComponent>().HasNext
                   && last.Get<TweenSequenceLinkComponent>().NextEntity.IsAlive())
            {
                last = last.Get<TweenSequenceLinkComponent>().NextEntity;
            }

            ref TweenSequenceLinkComponent link = ref last.Get<TweenSequenceLinkComponent>();
            link.NextEntity = second;
            link.HasNext = true;
        }

        /// <summary>Starts the next tween in a chain.</summary>
        internal static void ActivateNext(EcsEntity finished)
        {
            if (TryGetNext(finished, out EcsEntity next))
                Activate(next);
        }

        /// <summary>Cancels the rest of a chain, so waiting tweens aren't left held forever.</summary>
        internal static void CancelNext(EcsEntity cancelled)
        {
            if (TryGetNext(cancelled, out EcsEntity next))
                TweenControlOperations.Cancel(next);
        }

        private static bool TryGetNext(EcsEntity entity, out EcsEntity next)
        {
            next = default;

            if (!entity.IsAlive() || !entity.Has<TweenSequenceLinkComponent>())
                return false;

            ref TweenSequenceLinkComponent link = ref entity.Get<TweenSequenceLinkComponent>();
            if (!link.HasNext)
                return false;

            next = link.NextEntity;
            link.HasNext = false;
            return next.IsAlive();
        }

        private static void CaptureStartValues(EcsEntity entity)
        {
            ref TweenClockComponent clock = ref entity.Get<TweenClockComponent>();
            if (!clock.StartFromCurrent)
                return;

            byte mask = clock.AxisMask;
            ref TweenTargetComponent target = ref entity.Get<TweenTargetComponent>();
            if (target.RectTransform == null)
                return;

            if (entity.Has<TweenAlphaComponent>() && target.CanvasGroup != null)
            {
                entity.Get<TweenAlphaComponent>().StartValue = target.CanvasGroup.alpha;
            }
            else if (entity.Has<TweenColorComponent>() && target.Image != null)
            {
                ref TweenColorComponent color = ref entity.Get<TweenColorComponent>();
                Color current = target.Image.color;

                if (mask == TweenClockComponent.ALPHA_ONLY)
                    color.EndValue = new Color(current.r, current.g, current.b, color.EndValue.a);

                color.StartValue = current;
            }
            else if (entity.Has<TweenPositionComponent>())
            {
                ref TweenPositionComponent position = ref entity.Get<TweenPositionComponent>();
                Vector2 current = target.RectTransform.anchoredPosition;

                position.EndValue = new Vector2(
                    Target(mask, TweenClockComponent.AXIS_X, position.EndValue.x, current.x),
                    Target(mask, TweenClockComponent.AXIS_Y, position.EndValue.y, current.y));
                position.StartValue = current;
            }
            else if (entity.Has<TweenScaleComponent>())
            {
                ref TweenScaleComponent scale = ref entity.Get<TweenScaleComponent>();
                Vector3 current = target.RectTransform.localScale;

                scale.EndValue = new Vector3(
                    Target(mask, TweenClockComponent.AXIS_X, scale.EndValue.x, current.x),
                    Target(mask, TweenClockComponent.AXIS_Y, scale.EndValue.y, current.y),
                    Target(mask, TweenClockComponent.AXIS_Z, scale.EndValue.z, current.z));
                scale.StartValue = current;
            }
            else if (entity.Has<TweenRotationComponent>())
            {
                entity.Get<TweenRotationComponent>().StartValue = target.RectTransform.eulerAngles.z;
            }
            else if (entity.Has<TweenSizeDeltaComponent>())
            {
                entity.Get<TweenSizeDeltaComponent>().StartValue = target.RectTransform.sizeDelta;
            }
            else if (entity.Has<TweenPivotComponent>())
            {
                entity.Get<TweenPivotComponent>().StartValue = target.RectTransform.pivot;
            }
        }

        /// <summary>Keeps an axis at its current value when the tween doesn't animate that axis.</summary>
        private static float Target(byte mask, byte axis, float end, float current)
            => mask == TweenClockComponent.ALL_AXES || (mask & axis) != 0 ? end : current;
    }
}
