using Leopotam.Ecs;
using Tools.UITweens.Runtime.Scripts.Components;
using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Services
{
    /// <summary>
    /// Interface for tween strategies (ISP: Interface Segregation).
    /// Each strategy handles one type of tween animation.
    /// </summary>
    public interface ITweenStrategy
    {
        /// <summary>
        /// Initialize strategy-specific data on tween entity.
        /// </summary>
        void Initialize(EcsEntity entity, TweenInitializer initializer);

        /// <summary>
        /// Execute strategy-specific logic during tween update.
        /// </summary>
        void Update(EcsEntity entity, float easedProgress);

        /// <summary>
        /// Clean up strategy-specific resources.
        /// </summary>
        void Cleanup(EcsEntity entity);
    }

    /// <summary>
    /// Factory for creating tween strategies.
    /// Single Responsibility: Strategy instantiation only
    /// </summary>
    public class TweenStrategyFactory
    {
        /// <summary>
        /// Create position tween strategy.
        /// </summary>
        public ITweenStrategy CreatePositionStrategy(Vector2 startPos, Vector2 endPos)
        {
            return new PositionTweenStrategy(startPos, endPos);
        }

        /// <summary>
        /// Create alpha tween strategy.
        /// </summary>
        public ITweenStrategy CreateAlphaStrategy(float startAlpha, float endAlpha)
        {
            return new AlphaTweenStrategy(startAlpha, endAlpha);
        }

        /// <summary>
        /// Create scale tween strategy.
        /// </summary>
        public ITweenStrategy CreateScaleStrategy(Vector3 startScale, Vector3 endScale)
        {
            return new ScaleTweenStrategy(startScale, endScale);
        }

        /// <summary>
        /// Create rotation tween strategy.
        /// </summary>
        public ITweenStrategy CreateRotationStrategy(float startRotation, float endRotation)
        {
            return new RotationTweenStrategy(startRotation, endRotation);
        }

        /// <summary>
        /// Create color tween strategy.
        /// </summary>
        public ITweenStrategy CreateColorStrategy(Color startColor, Color endColor)
        {
            return new ColorTweenStrategy(startColor, endColor);
        }

        /// <summary>
        /// Create size delta tween strategy.
        /// </summary>
        public ITweenStrategy CreateSizeStrategy(Vector2 startSize, Vector2 endSize)
        {
            return new SizeTweenStrategy(startSize, endSize);
        }

        /// <summary>
        /// Create pivot tween strategy.
        /// </summary>
        public ITweenStrategy CreatePivotStrategy(Vector2 startPivot, Vector2 endPivot)
        {
            return new PivotTweenStrategy(startPivot, endPivot);
        }

        /// <summary>
        /// Create custom float tween strategy.
        /// </summary>
        public ITweenStrategy CreateCustomFloatStrategy(float startValue, float endValue)
        {
            return new CustomFloatTweenStrategy(startValue, endValue);
        }

        /// <summary>
        /// Create custom vector tween strategy.
        /// </summary>
        public ITweenStrategy CreateCustomVectorStrategy(Vector3 startValue, Vector3 endValue)
        {
            return new CustomVectorTweenStrategy(startValue, endValue);
        }
    }

    #region Strategy Implementations

    internal class PositionTweenStrategy : ITweenStrategy
    {
        private Vector2 startPos;
        private Vector2 endPos;

        public PositionTweenStrategy(Vector2 start, Vector2 end)
        {
            startPos = start;
            endPos = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupPosition(entity, startPos, endPos);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.PositionResult = Vector2.Lerp(startPos, endPos, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class AlphaTweenStrategy : ITweenStrategy
    {
        private float startAlpha;
        private float endAlpha;

        public AlphaTweenStrategy(float start, float end)
        {
            startAlpha = start;
            endAlpha = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupAlpha(entity, startAlpha, endAlpha);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.AlphaResult = Mathf.Lerp(startAlpha, endAlpha, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class ScaleTweenStrategy : ITweenStrategy
    {
        private Vector3 startScale;
        private Vector3 endScale;

        public ScaleTweenStrategy(Vector3 start, Vector3 end)
        {
            startScale = start;
            endScale = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupScale(entity, startScale, endScale);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.ScaleResult = Vector3.Lerp(startScale, endScale, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class RotationTweenStrategy : ITweenStrategy
    {
        private float startRotation;
        private float endRotation;

        public RotationTweenStrategy(float start, float end)
        {
            startRotation = start;
            endRotation = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupRotation(entity, startRotation, endRotation);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.RotationResult = Mathf.Lerp(startRotation, endRotation, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class ColorTweenStrategy : ITweenStrategy
    {
        private Color startColor;
        private Color endColor;

        public ColorTweenStrategy(Color start, Color end)
        {
            startColor = start;
            endColor = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupColor(entity, startColor, endColor);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.ColorResult = Color.Lerp(startColor, endColor, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class SizeTweenStrategy : ITweenStrategy
    {
        private Vector2 startSize;
        private Vector2 endSize;

        public SizeTweenStrategy(Vector2 start, Vector2 end)
        {
            startSize = start;
            endSize = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupSizeDelta(entity, startSize, endSize);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.SizeDeltaResult = Vector2.Lerp(startSize, endSize, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class PivotTweenStrategy : ITweenStrategy
    {
        private Vector2 startPivot;
        private Vector2 endPivot;

        public PivotTweenStrategy(Vector2 start, Vector2 end)
        {
            startPivot = start;
            endPivot = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupPivot(entity, startPivot, endPivot);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var result = ref entity.Get<TweenResultComponent>();
            result.PivotResult = Vector2.Lerp(startPivot, endPivot, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class CustomFloatTweenStrategy : ITweenStrategy
    {
        private float startValue;
        private float endValue;

        public CustomFloatTweenStrategy(float start, float end)
        {
            startValue = start;
            endValue = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupCustomFloat(entity, startValue, endValue);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var customFloat = ref entity.Get<TweenCustomFloatComponent>();
            customFloat.CurrentValue = Mathf.Lerp(startValue, endValue, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    internal class CustomVectorTweenStrategy : ITweenStrategy
    {
        private Vector3 startValue;
        private Vector3 endValue;

        public CustomVectorTweenStrategy(Vector3 start, Vector3 end)
        {
            startValue = start;
            endValue = end;
        }

        public void Initialize(EcsEntity entity, TweenInitializer initializer)
        {
            initializer.SetupCustomVector(entity, startValue, endValue);
        }

        public void Update(EcsEntity entity, float easedProgress)
        {
            ref var customVec = ref entity.Get<TweenCustomVectorComponent>();
            customVec.CurrentValue = Vector3.Lerp(startValue, endValue, easedProgress);
        }

        public void Cleanup(EcsEntity entity) { }
    }

    #endregion
}
