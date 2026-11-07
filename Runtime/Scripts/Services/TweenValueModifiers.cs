using Leopotam.Ecs;
using UITweens.Components;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>Rewrites a tween's start and end values for From() and Relative().</summary>
    internal static class TweenValueModifiers
    {
        /// <summary>
        /// Swaps start and end, so the tween plays from the given value to the value the target has now,
        /// and shows the given value straight away.
        /// </summary>
        internal static void ApplyFrom(EcsEntity entity)
        {
            ref TweenClockComponent clock = ref entity.Get<TweenCoreComponent>().Clock;
            if (clock.IsFrom)
                return;

            clock.IsFrom = true;

            // The end is the value the target has now, so a later start mustn't read it again.
            clock.StartFromCurrent = false;

            Swap(entity);

            if (clock.IsRelative)
                ResolveRelative(entity);

            ShowStartValue(entity);
        }

        /// <summary>
        /// Turns a Relative() offset into absolute values. Normal tweens add the start value to the
        /// end when they start playing; From() tweens add the value at creation to the start.
        /// </summary>
        internal static void ResolveRelative(EcsEntity entity)
        {
            ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();
            ref TweenClockComponent clock = ref core.Clock;
            clock.ValuesResolved = true;

            bool from = clock.IsFrom;
            byte mask = clock.AxisMask;

            if (entity.Has<TweenAlphaComponent>())
            {
                ref TweenAlphaComponent c = ref entity.Get<TweenAlphaComponent>();
                if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
            }
            else if (entity.Has<TweenColorComponent>())
            {
                ref TweenColorComponent c = ref entity.Get<TweenColorComponent>();
                if (mask == TweenClockComponent.ALPHA_ONLY)
                {
                    if (from) c.StartValue.a += c.EndValue.a; else c.EndValue.a += c.StartValue.a;
                }
                else
                {
                    if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
                }
            }
            else if (entity.Has<TweenPositionComponent>())
            {
                ref TweenPositionComponent c = ref entity.Get<TweenPositionComponent>();
                if (from) c.StartValue = Add(mask, c.StartValue, c.EndValue); else c.EndValue = Add(mask, c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenScaleComponent>())
            {
                ref TweenScaleComponent c = ref entity.Get<TweenScaleComponent>();
                if (from) c.StartValue = Add(mask, c.StartValue, c.EndValue); else c.EndValue = Add(mask, c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenRotationComponent>())
            {
                ref TweenRotationComponent c = ref entity.Get<TweenRotationComponent>();
                if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
            }
            else if (entity.Has<TweenSizeDeltaComponent>())
            {
                ref TweenSizeDeltaComponent c = ref entity.Get<TweenSizeDeltaComponent>();
                if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
            }
            else if (entity.Has<TweenPivotComponent>())
            {
                ref TweenPivotComponent c = ref entity.Get<TweenPivotComponent>();
                if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
            }
            else if (entity.Has<TweenCustomFloatComponent>())
            {
                ref TweenCustomFloatComponent c = ref entity.Get<TweenCustomFloatComponent>();
                if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
            }
            else if (entity.Has<TweenCustomVectorComponent>())
            {
                ref TweenCustomVectorComponent c = ref entity.Get<TweenCustomVectorComponent>();
                if (from) c.StartValue += c.EndValue; else c.EndValue += c.StartValue;
            }
        }

        /// <summary>Writes the start value to the target, for a From() tween that hasn't started yet.</summary>
        internal static void ShowStartValue(EcsEntity entity)
        {
            ref TweenCoreComponent core = ref entity.Get<TweenCoreComponent>();
            ref TweenTargetComponent target = ref core.Target;
            ref TweenResultComponent result = ref core.Result;

            if (entity.Has<TweenAlphaComponent>())
            {
                float start = entity.Get<TweenAlphaComponent>().StartValue;
                result.AlphaResult = start;
                if (target.CanvasGroup != null)
                    target.CanvasGroup.alpha = start;
            }
            else if (entity.Has<TweenColorComponent>())
            {
                Color start = entity.Get<TweenColorComponent>().StartValue;
                result.ColorResult = start;
                if (target.Image != null)
                    target.Image.color = start;
            }
            else if (target.RectTransform == null)
            {
                return;
            }
            else if (entity.Has<TweenPositionComponent>())
            {
                Vector2 start = entity.Get<TweenPositionComponent>().StartValue;
                result.PositionResult = start;
                target.RectTransform.anchoredPosition = start;
            }
            else if (entity.Has<TweenScaleComponent>())
            {
                Vector3 start = entity.Get<TweenScaleComponent>().StartValue;
                result.ScaleResult = start;
                target.RectTransform.localScale = start;
            }
            else if (entity.Has<TweenRotationComponent>())
            {
                float start = entity.Get<TweenRotationComponent>().StartValue;
                result.RotationResult = start;
                Vector3 euler = target.RectTransform.eulerAngles;
                euler.z = start;
                target.RectTransform.eulerAngles = euler;
            }
            else if (entity.Has<TweenSizeDeltaComponent>())
            {
                Vector2 start = entity.Get<TweenSizeDeltaComponent>().StartValue;
                result.SizeDeltaResult = start;
                target.RectTransform.sizeDelta = start;
            }
            else if (entity.Has<TweenPivotComponent>())
            {
                Vector2 start = entity.Get<TweenPivotComponent>().StartValue;
                result.PivotResult = start;
                target.RectTransform.pivot = start;
            }
        }

        private static void Swap(EcsEntity entity)
        {
            if (entity.Has<TweenAlphaComponent>())
            {
                ref TweenAlphaComponent c = ref entity.Get<TweenAlphaComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenColorComponent>())
            {
                ref TweenColorComponent c = ref entity.Get<TweenColorComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenPositionComponent>())
            {
                ref TweenPositionComponent c = ref entity.Get<TweenPositionComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenScaleComponent>())
            {
                ref TweenScaleComponent c = ref entity.Get<TweenScaleComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenRotationComponent>())
            {
                ref TweenRotationComponent c = ref entity.Get<TweenRotationComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenSizeDeltaComponent>())
            {
                ref TweenSizeDeltaComponent c = ref entity.Get<TweenSizeDeltaComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenPivotComponent>())
            {
                ref TweenPivotComponent c = ref entity.Get<TweenPivotComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenCustomFloatComponent>())
            {
                ref TweenCustomFloatComponent c = ref entity.Get<TweenCustomFloatComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
            else if (entity.Has<TweenCustomVectorComponent>())
            {
                ref TweenCustomVectorComponent c = ref entity.Get<TweenCustomVectorComponent>();
                (c.StartValue, c.EndValue) = (c.EndValue, c.StartValue);
            }
        }

        // Adds only on the animated axes. An axis the tween doesn't animate keeps its value.
        private static Vector2 Add(byte mask, Vector2 value, Vector2 offset)
            => new(
                Animated(mask, TweenClockComponent.AXIS_X) ? value.x + offset.x : value.x,
                Animated(mask, TweenClockComponent.AXIS_Y) ? value.y + offset.y : value.y);

        private static Vector3 Add(byte mask, Vector3 value, Vector3 offset)
            => new(
                Animated(mask, TweenClockComponent.AXIS_X) ? value.x + offset.x : value.x,
                Animated(mask, TweenClockComponent.AXIS_Y) ? value.y + offset.y : value.y,
                Animated(mask, TweenClockComponent.AXIS_Z) ? value.z + offset.z : value.z);

        private static bool Animated(byte mask, byte axis)
            => mask == TweenClockComponent.ALL_AXES || (mask & axis) != 0;
    }
}
