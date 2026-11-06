using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Math;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Services
{
    /// <summary>
    /// Initializes tween entities with components and values.
    /// Single Responsibility: Tween setup only
    /// </summary>
    public class TweenInitializer
    {
        private readonly TweenPoolManager poolManager;

        public TweenInitializer(TweenPoolManager poolManager)
        {
            this.poolManager = poolManager;
        }

        /// <summary>
        /// Initialize tween entity with base configuration and timing.
        /// Initialize with NotStarted state for proper state machine flow.
        /// </summary>
        public void InitializeBaseTween(
            EcsEntity entity,
            RectTransform element,
            float duration,
            EaseType ease,
            float easeParameter,
            CanvasGroup canvasGroup = null,
            Image image = null)
        {
            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            clock.ElapsedTime = 0f;
            clock.Duration = duration;
            clock.Ease = ease;
            clock.EaseParameter = easeParameter;
            clock.ElapsedDelay = 0f;
            clock.DelayDuration = 0f;
            clock.HasCapturedStartValues = false;

            ref var target = ref entity.Get<TweenCoreComponent>().Target;
            target.RectTransform = element;
            target.CanvasGroup = canvasGroup;
            target.Image = image;

            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.AlphaResult = 0f;
            result.PositionResult = Vector2.zero;
            result.ScaleResult = Vector3.one;
            result.RotationResult = 0f;
            result.ColorResult = Color.white;
            result.SizeDeltaResult = Vector2.zero;
            result.PivotResult = Vector2.zero;

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            control.State = TweenControlComponent.ControlState.NotStarted;
            control.PausedElapsedTime = 0f;
            control.HasKilledCompetitors = false;
            control.PredecessorHandle = default;
            control.HasTrackingKey = false;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyAlpha = false;
            flags.ApplyPosition = false;
            flags.ApplyScale = false;
            flags.ApplyRotation = false;
            flags.ApplyColor = false;
            flags.ApplySizeDelta = false;
            flags.ApplyPivot = false;

            InitializeOptionalComponents(entity);
        }

        /// <summary>
        /// Setup alpha animation component.
        /// </summary>
        public void SetupAlpha(EcsEntity entity, float startAlpha, float endAlpha)
        {
            poolManager.Filters.RequireAlpha();

            ref var alphaComp = ref entity.Get<TweenAlphaComponent>();
            alphaComp.StartValue = startAlpha;
            alphaComp.EndValue = endAlpha;

            // Initialize result with start alpha to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.AlphaResult = startAlpha;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyAlpha = true;
        }

        /// <summary>
        /// Setup position animation component.
        /// </summary>
        public void SetupPosition(EcsEntity entity, Vector2 startPosition, Vector2 endPosition)
        {
            poolManager.Filters.RequirePosition();

            ref var posComp = ref entity.Get<TweenPositionComponent>();
            posComp.StartValue = startPosition;
            posComp.EndValue = endPosition;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.PositionResult = startPosition;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyPosition = true;
        }

        /// <summary>
        /// Setup scale animation component.
        /// </summary>
        public void SetupScale(EcsEntity entity, Vector3 startScale, Vector3 endScale)
        {
            poolManager.Filters.RequireScale();

            ref var scaleComp = ref entity.Get<TweenScaleComponent>();
            scaleComp.StartValue = startScale;
            scaleComp.EndValue = endScale;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.ScaleResult = startScale;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyScale = true;
        }

        /// <summary>
        /// Setup rotation animation component.
        /// </summary>
        public void SetupRotation(EcsEntity entity, float startRotation, float endRotation)
        {
            poolManager.Filters.RequireRotation();

            ref var rotComp = ref entity.Get<TweenRotationComponent>();
            rotComp.StartValue = startRotation;
            rotComp.EndValue = endRotation;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.RotationResult = startRotation;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyRotation = true;
        }

        /// <summary>
        /// Setup color animation component.
        /// </summary>
        public void SetupColor(EcsEntity entity, Color startColor, Color endColor)
        {
            poolManager.Filters.RequireColor();

            ref var colorComp = ref entity.Get<TweenColorComponent>();
            colorComp.StartValue = startColor;
            colorComp.EndValue = endColor;

            // Initialize result with start color to avoid white flash during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.ColorResult = startColor;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyColor = true;
        }

        /// <summary>
        /// Setup size delta animation component.
        /// </summary>
        public void SetupSizeDelta(EcsEntity entity, Vector2 startSize, Vector2 endSize)
        {
            poolManager.Filters.RequireSizeDelta();

            ref var sizeComp = ref entity.Get<TweenSizeDeltaComponent>();
            sizeComp.StartValue = startSize;
            sizeComp.EndValue = endSize;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.SizeDeltaResult = startSize;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplySizeDelta = true;
        }

        /// <summary>
        /// Setup pivot animation component.
        /// </summary>
        public void SetupPivot(EcsEntity entity, Vector2 startPivot, Vector2 endPivot)
        {
            poolManager.Filters.RequirePivot();

            ref var pivotComp = ref entity.Get<TweenPivotComponent>();
            pivotComp.StartValue = startPivot;
            pivotComp.EndValue = endPivot;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenCoreComponent>().Result;
            result.PivotResult = startPivot;

            ref var flags = ref entity.Get<TweenCoreComponent>().Flags;
            flags.ApplyPivot = true;
        }

        /// <summary>
        /// Setup custom float animation component.
        /// </summary>
        public void SetupCustomFloat(EcsEntity entity, float startValue, float endValue)
        {
            poolManager.Filters.RequireCustomFloat();

            ref var customFloat = ref entity.Get<TweenCustomFloatComponent>();
            customFloat.StartValue = startValue;
            customFloat.EndValue = endValue;
            customFloat.CurrentValue = startValue;
        }

        /// <summary>
        /// Setup custom vector animation component.
        /// </summary>
        public void SetupCustomVector(EcsEntity entity, Vector3 startValue, Vector3 endValue)
        {
            poolManager.Filters.RequireCustomVector();

            ref var customVec = ref entity.Get<TweenCustomVectorComponent>();
            customVec.StartValue = startValue;
            customVec.EndValue = endValue;
            customVec.CurrentValue = startValue;
        }

        /// <summary>
        /// Set delay duration and transition state from NotStarted to Delayed.
        /// This is called by TweenBuilder.Delay() to integrate delay into the unified state machine.
        /// </summary>
        public void SetDelay(EcsEntity entity, float delaySeconds)
        {
            poolManager.Filters.RequireDelayed();

            if (!entity.IsAlive())
                return;

            ref var clock = ref entity.Get<TweenCoreComponent>().Clock;
            clock.DelayDuration = delaySeconds;
            clock.ElapsedDelay = 0f;
            clock.HasCapturedStartValues = false;

            // CRITICAL FIX: Also update the old TweenDelayComponent for backward compatibility
            // This ensures the apply system sees the delay duration
            ref var delayComp = ref entity.Get<TweenCoreComponent>().Delay;
            delayComp.DelayTime = delaySeconds;
            delayComp.ElapsedDelay = 0f;

            ref var control = ref entity.Get<TweenCoreComponent>().Control;
            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = TweenControlComponent.ControlState.Delayed;
                control.HasKilledCompetitors = false;
            }
        }

        private void InitializeOptionalComponents(EcsEntity entity)
        {
            ref var delayComp = ref entity.Get<TweenCoreComponent>().Delay;
            delayComp.DelayTime = 0f;
            delayComp.ElapsedDelay = 0f;
            delayComp.DelayNotified = false; // Explicitly initialize
        }

        /// <summary>Adds a clean loop component the first time a tween needs one.</summary>
        internal static void EnsureLoopComponent(EcsEntity entity)
        {
            if (entity.Has<TweenLoopComponent>())
                return;

            ref TweenLoopComponent loop = ref entity.Get<TweenLoopComponent>();
            loop = default;
            loop.LoopCount = 1;
        }

        /// <summary>Adds a clean callback component the first time a tween needs one.</summary>
        internal static void EnsureAdvancedCallbackComponent(EcsEntity entity)
        {
            if (entity.Has<TweenAdvancedCallbackComponent>())
                return;

            ref TweenAdvancedCallbackComponent callbacks = ref entity.Get<TweenAdvancedCallbackComponent>();
            callbacks = default;
        }
    }
}
