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
            RegisterWithTweenManager(element, canvasGroup, image);

            ref var clock = ref entity.Get<TweenClockComponent>();
            clock.ElapsedTime = 0f;
            clock.Duration = duration;
            clock.Ease = ease;
            clock.EaseParameter = easeParameter;
            clock.ElapsedDelay = 0f;
            clock.DelayDuration = 0f;
            clock.HasCapturedStartValues = false;

            ref var target = ref entity.Get<TweenTargetComponent>();
            target.RectTransform = element;
            target.CanvasGroup = canvasGroup;
            target.Image = image;

            ref var result = ref entity.Get<TweenResultComponent>();
            result.AlphaResult = 0f;
            result.PositionResult = Vector2.zero;
            result.ScaleResult = Vector3.one;
            result.RotationResult = 0f;
            result.ColorResult = Color.white;
            result.SizeDeltaResult = Vector2.zero;
            result.PivotResult = Vector2.zero;

            ref var control = ref entity.Get<TweenControlComponent>();
            control.State = TweenControlComponent.ControlState.NotStarted;
            control.PausedElapsedTime = 0f;
            control.HasKilledCompetitors = false;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
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
            ref var alphaComp = ref entity.Get<TweenAlphaComponent>();
            alphaComp.StartValue = startAlpha;
            alphaComp.EndValue = endAlpha;

            // Initialize result with start alpha to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.AlphaResult = startAlpha;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplyAlpha = true;
        }

        /// <summary>
        /// Setup position animation component.
        /// </summary>
        public void SetupPosition(EcsEntity entity, Vector2 startPosition, Vector2 endPosition)
        {
            ref var posComp = ref entity.Get<TweenPositionComponent>();
            posComp.StartValue = startPosition;
            posComp.EndValue = endPosition;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.PositionResult = startPosition;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplyPosition = true;
        }

        /// <summary>
        /// Setup scale animation component.
        /// </summary>
        public void SetupScale(EcsEntity entity, Vector3 startScale, Vector3 endScale)
        {
            ref var scaleComp = ref entity.Get<TweenScaleComponent>();
            scaleComp.StartValue = startScale;
            scaleComp.EndValue = endScale;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.ScaleResult = startScale;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplyScale = true;
        }

        /// <summary>
        /// Setup rotation animation component.
        /// </summary>
        public void SetupRotation(EcsEntity entity, float startRotation, float endRotation)
        {
            ref var rotComp = ref entity.Get<TweenRotationComponent>();
            rotComp.StartValue = startRotation;
            rotComp.EndValue = endRotation;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.RotationResult = startRotation;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplyRotation = true;
        }

        /// <summary>
        /// Setup color animation component.
        /// </summary>
        public void SetupColor(EcsEntity entity, Color startColor, Color endColor)
        {
            ref var colorComp = ref entity.Get<TweenColorComponent>();
            colorComp.StartValue = startColor;
            colorComp.EndValue = endColor;

            // Initialize result with start color to avoid white flash during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.ColorResult = startColor;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplyColor = true;
        }

        /// <summary>
        /// Setup size delta animation component.
        /// </summary>
        public void SetupSizeDelta(EcsEntity entity, Vector2 startSize, Vector2 endSize)
        {
            ref var sizeComp = ref entity.Get<TweenSizeDeltaComponent>();
            sizeComp.StartValue = startSize;
            sizeComp.EndValue = endSize;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.SizeDeltaResult = startSize;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplySizeDelta = true;
        }

        /// <summary>
        /// Setup pivot animation component.
        /// </summary>
        public void SetupPivot(EcsEntity entity, Vector2 startPivot, Vector2 endPivot)
        {
            ref var pivotComp = ref entity.Get<TweenPivotComponent>();
            pivotComp.StartValue = startPivot;
            pivotComp.EndValue = endPivot;

            // Initialize result with start value to avoid unwanted initial value during delay
            ref var result = ref entity.Get<TweenResultComponent>();
            result.PivotResult = startPivot;

            ref var flags = ref entity.Get<TweenApplyFlagsComponent>();
            flags.ApplyPivot = true;
        }

        /// <summary>
        /// Setup custom float animation component.
        /// </summary>
        public void SetupCustomFloat(EcsEntity entity, float startValue, float endValue)
        {
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
            if (!entity.IsAlive())
                return;

            ref var clock = ref entity.Get<TweenClockComponent>();
            clock.DelayDuration = delaySeconds;
            clock.ElapsedDelay = 0f;
            clock.HasCapturedStartValues = false;

            // CRITICAL FIX: Also update the old TweenDelayComponent for backward compatibility
            // This ensures the apply system sees the delay duration
            ref var delayComp = ref entity.Get<TweenDelayComponent>();
            delayComp.DelayTime = delaySeconds;
            delayComp.ElapsedDelay = 0f;

            ref var control = ref entity.Get<TweenControlComponent>();
            if (control.State == TweenControlComponent.ControlState.NotStarted)
            {
                control.State = TweenControlComponent.ControlState.Delayed;
                control.HasKilledCompetitors = false;
            }
        }

        private void InitializeOptionalComponents(EcsEntity entity)
        {
            ref var delayComp = ref entity.Get<TweenDelayComponent>();
            delayComp.DelayTime = 0f;
            delayComp.ElapsedDelay = 0f;
            delayComp.DelayNotified = false; // Explicitly initialize


            ref var loopComp = ref entity.Get<TweenLoopComponent>();
            loopComp.Mode = TweenLoopComponent.LoopMode.None;
            loopComp.LoopCount = 1;
            loopComp.CurrentLoop = 0;

            ref var advCallbacks = ref entity.Get<TweenAdvancedCallbackComponent>();
            advCallbacks.OnStart = null;
            advCallbacks.OnUpdate = null;
            advCallbacks.StartCalled = false;
        }

        private void RegisterWithTweenManager(RectTransform element, CanvasGroup canvasGroup, Image image)
        {
            TweenManager.Instance?.RegisterTransform(element);
            if (canvasGroup != null)
                TweenManager.Instance?.RegisterCanvasGroup(canvasGroup);
            if (image != null)
                TweenManager.Instance?.RegisterImage(image);
        }
    }
}
