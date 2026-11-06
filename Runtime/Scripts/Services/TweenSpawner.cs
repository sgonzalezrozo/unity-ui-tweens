using Leopotam.Ecs;
using UITweens.Components;
using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Public API facade for spawning and managing UI tweens.
    /// Delegates work to extension methods (one per animation type).
    /// Single Responsibility: Spawn coordination and pool management only.
    /// </summary>
    public class TweenSpawner
    {
        private readonly EcsWorld world;
        private readonly TweenPoolManager poolManager;
        private readonly TweenInitializer initializer;

        public TweenSpawner(EcsWorld world, TweenPoolManager sharedPoolManager = null)
        {
            this.world = world;
            poolManager = sharedPoolManager ?? new TweenPoolManager(world);
            initializer = new TweenInitializer(poolManager);
        }

        /// <summary>
        /// Spawn new tween entity from pool and initialize.
        /// Internal API for extension methods.
        /// </summary>
        internal TweenHandle SpawnTweenInternal(
            RectTransform element,
            float duration,
            EaseType ease,
            float easeParameter,
            CanvasGroup canvasGroup = null,
            UnityEngine.UI.Image image = null)
        {
            EcsEntity entity = poolManager.AcquireEntity();
            initializer.InitializeBaseTween(entity, element, duration, ease, easeParameter, canvasGroup, image);
            entity.Get<TweenActiveMarkerComponent>();
            return new TweenHandle(entity);
        }

        /// <summary>
        /// Initialize position animation. Internal API for extensions.
        /// </summary>
        internal void InitializePositionInternal(TweenHandle handle, Vector2 startPosition, Vector2 endPosition)
        {
            initializer.SetupPosition(handle.Entity, startPosition, endPosition);
        }

        /// <summary>
        /// Initialize alpha animation. Internal API for extensions.
        /// </summary>
        internal void InitializeAlphaInternal(TweenHandle handle, float startAlpha, float endAlpha)
        {
            initializer.SetupAlpha(handle.Entity, startAlpha, endAlpha);
        }

        /// <summary>
        /// Initialize scale animation. Internal API for extensions.
        /// </summary>
        internal void InitializeScaleInternal(TweenHandle handle, Vector3 startScale, Vector3 endScale)
        {
            initializer.SetupScale(handle.Entity, startScale, endScale);
        }

        /// <summary>
        /// Initialize rotation animation. Internal API for extensions.
        /// </summary>
        internal void InitializeRotationInternal(TweenHandle handle, float startRotation, float endRotation)
        {
            initializer.SetupRotation(handle.Entity, startRotation, endRotation);
        }

        /// <summary>
        /// Initialize color animation. Internal API for extensions.
        /// </summary>
        internal void InitializeColorInternal(TweenHandle handle, Color startColor, Color endColor)
        {
            initializer.SetupColor(handle.Entity, startColor, endColor);
        }

        /// <summary>
        /// Initialize size delta animation. Internal API for extensions.
        /// </summary>
        internal void InitializeSizeDeltaInternal(TweenHandle handle, Vector2 startSize, Vector2 endSize)
        {
            initializer.SetupSizeDelta(handle.Entity, startSize, endSize);
        }

        /// <summary>
        /// Initialize pivot animation. Internal API for extensions.
        /// </summary>
        internal void InitializePivotInternal(TweenHandle handle, Vector2 startPivot, Vector2 endPivot)
        {
            initializer.SetupPivot(handle.Entity, startPivot, endPivot);
        }

        /// <summary>
        /// Initialize custom float animation. Internal API for extensions.
        /// </summary>
        internal void InitializeCustomFloatInternal(TweenHandle handle, float startValue, float endValue)
        {
            initializer.SetupCustomFloat(handle.Entity, startValue, endValue);
        }

        /// <summary>
        /// Initialize custom vector animation. Internal API for extensions.
        /// </summary>
        internal void InitializeCustomVectorInternal(TweenHandle handle, Vector3 startValue, Vector3 endValue)
        {
            initializer.SetupCustomVector(handle.Entity, startValue, endValue);
        }

        /// <summary>
        /// Link tweens for sequencing. The second tween waits until the first one finishes.
        /// </summary>
        internal void LinkTweens(TweenHandle firstHandle, TweenHandle secondHandle)
        {
            if (!firstHandle.IsValid || !secondHandle.IsValid)
                return;

            TweenSequencer.Link(firstHandle.Entity, secondHandle.Entity);
        }

        /// <summary>
        /// Execute sequence of tween declarations in order.
        /// </summary>
        public TweenBuilder Sequential(params ITweenDeclaration[] declarations)
        {
            if (declarations == null || declarations.Length == 0)
            {
                Debug.LogWarning("[UITweens] Sequential called with no declarations");
                return new TweenBuilder(this);
            }

            TweenBuilder currentBuilder = new TweenBuilder(this);
            TweenHandle firstHandle = new TweenHandle();
            TweenHandle previousHandle = new TweenHandle();

            for (int i = 0; i < declarations.Length; i++)
            {
                ITweenDeclaration decl = declarations[i];

                if (decl == null)
                {
                    Debug.LogWarning("[UITweens] Null declaration at index " + i);
                    continue;
                }

                currentBuilder = decl.Spawn(this);
                TweenHandle currentHandle = currentBuilder.GetHandle();

                if (!currentHandle.IsValid)
                    continue;

                if (previousHandle.IsValid)
                    LinkTweens(previousHandle, currentHandle);
                else
                    firstHandle = currentHandle;

                previousHandle = currentHandle;
            }

            if (firstHandle.IsValid)
                TweenSequencer.Activate(firstHandle.Entity);

            return currentBuilder;
        }

        /// <summary>
        /// Create wait/delay tween declaration for Sequential().
        /// </summary>
        public TweenWait Wait(float seconds)
        {
            return new TweenWait(seconds);
        }

        /// <summary>
        /// Get the initializer instance.
        /// Used by TweenBuilder.Delay() to set delay on entity.
        /// </summary>
        internal TweenInitializer GetInitializer()
            => initializer;
    }
}
