using System;
using UITweens.Math;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Components
{
    /// <summary>
    /// Core clock component that tracks time for any tween.
    /// Add this to any entity that needs animation. Timing is shared across all
    /// animation types on the same entity (position + alpha + scale can share one clock).
    /// NEW: Phase 10 - Integrated delay tracking for unified state machine.
    /// </summary>
    public struct TweenClockComponent
    {
        public float ElapsedTime;
        public float Duration;
        public EaseType Ease;
        public float EaseParameter; // For Back overshoot, Elastic amplitude. Default -1 uses preset.

        // NEW: Phase 10 - Delay tracking integrated into clock
        public float ElapsedDelay;     // How much delay has elapsed
        public float DelayDuration;    // Total delay in seconds (0 if no delay)
        public bool HasCapturedStartValues; // Flag: start values captured on Delayed → Playing transition

        public TweenClockComponent(float duration, EaseType ease = EaseType.Linear, float easeParameter = -1f)
        {
            ElapsedTime = 0f;
            Duration = duration;
            Ease = ease;
            EaseParameter = easeParameter;
            ElapsedDelay = 0f;
            DelayDuration = 0f;
            HasCapturedStartValues = false;
        }

        /// <summary>
        /// Gets normalized time [0, 1] clamped to duration.
        /// </summary>
        public float GetNormalizedTime()
            => ElapsedTime >= Duration ? 1f : ElapsedTime / Duration;

        /// <summary>
        /// True when ElapsedTime >= Duration.
        /// </summary>
        public bool IsFinished
            => ElapsedTime >= Duration;

        /// <summary>
        /// NEW: Phase 10 - True when delay has finished (ElapsedDelay >= DelayDuration).
        /// </summary>
        public bool IsDelayFinished
            => ElapsedDelay >= DelayDuration;

        /// <summary>
        /// NEW: Phase 10 - True if this tween has a delay.
        /// </summary>
        public bool HasDelay
            => DelayDuration > 0f;
    }

    /// <summary>
    /// Tween animation for alpha/opacity values.
    /// Only stores start/end values; timing is in TweenClockComponent.
    /// </summary>
    public struct TweenAlphaComponent
    {
        public float StartValue;
        public float EndValue;

        public TweenAlphaComponent(float startValue, float endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Tween animation for position (RectTransform.anchoredPosition).
    /// Only stores start/end values; timing is in TweenClockComponent.
    /// </summary>
    public struct TweenPositionComponent
    {
        public UnityEngine.Vector2 StartValue;
        public UnityEngine.Vector2 EndValue;

        public TweenPositionComponent(UnityEngine.Vector2 startValue, UnityEngine.Vector2 endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Tween animation for scale.
    /// Only stores start/end values; timing is in TweenClockComponent.
    /// </summary>
    public struct TweenScaleComponent
    {
        public UnityEngine.Vector3 StartValue;
        public UnityEngine.Vector3 EndValue;

        public TweenScaleComponent(UnityEngine.Vector3 startValue, UnityEngine.Vector3 endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Tween animation for rotation (z-axis for 2D/UI).
    /// Only stores start/end values; timing is in TweenClockComponent.
    /// </summary>
    public struct TweenRotationComponent
    {
        public float StartValue;
        public float EndValue;

        public TweenRotationComponent(float startValue, float endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Tween animation for color (any UI element).
    /// NEW: Phase 2 - Color animations
    /// </summary>
    public struct TweenColorComponent
    {
        public Color StartValue;
        public Color EndValue;

        public TweenColorComponent(Color startValue, Color endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Tween animation for RectTransform.sizeDelta (UI sizing).
    /// NEW: Phase 2 - Size animations
    /// </summary>
    public struct TweenSizeDeltaComponent
    {
        public Vector2 StartValue;
        public Vector2 EndValue;

        public TweenSizeDeltaComponent(Vector2 startValue, Vector2 endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Tween animation for RectTransform.pivot.
    /// NEW: Phase 2 - Pivot animations
    /// </summary>
    public struct TweenPivotComponent
    {
        public Vector2 StartValue;
        public Vector2 EndValue;

        public TweenPivotComponent(Vector2 startValue, Vector2 endValue)
        {
            StartValue = startValue;
            EndValue = endValue;
        }
    }

    /// <summary>
    /// Computed animation results written by math system.
    /// Applied by apply system (UGUI, UI Toolkit, etc).
    /// </summary>
    public struct TweenResultComponent
    {
        public float AlphaResult;
        public Vector2 PositionResult;
        public Vector3 ScaleResult;
        public float RotationResult;
        public Color ColorResult;       // NEW: Phase 2
        public Vector2 SizeDeltaResult; // NEW: Phase 2
        public Vector2 PivotResult;     // NEW: Phase 2
    }

    /// <summary>
    /// Reference to the next tween entity to spawn when this one finishes.
    /// Enables event-driven sequential animation chains without callbacks.
    /// </summary>
    public struct TweenSequenceLinkComponent
    {
        public int NextTweenEntityId; // Reference to next entity in sequence
    }

    /// <summary>
    /// Control state for tween playback (pause, resume, cancel).
    /// NEW: Phase 10 - Extended state machine: NotStarted → Delayed → Playing → Completed
    /// This replaces Phase 3's simple 3-state enum with explicit lifecycle tracking.
    /// </summary>
    public struct TweenControlComponent
    {
        public enum ControlState : byte
        {
            NotStarted = 0,  // Tween created, waiting for first update
            Delayed = 1,     // Delay phase, holds property at start value
            Playing = 2,     // Active animation, advancing progress
            Paused = 3,      // Paused (can resume from Playing or Delayed)
            Cancelled = 4,   // Killed, will be destroyed
        }

        public ControlState State;
        public float PausedElapsedTime; // Snapshot when paused

        // NEW: Phase 10 - Flag to ensure kill-competitors logic runs exactly once
        // when transitioning from Delayed → Playing
        public bool HasKilledCompetitors;

        // NEW: Phase 12 - Store the tween this one is replacing (for delayed tweens)
        // When a delayed tween's delay finishes, it cancels this predecessor
        public TweenHandle PredecessorHandle;
    }

    /// <summary>
    /// Callback storage for tween completion.
    /// Uses parameterless Action to avoid closure allocations (ZERO-GC).
    /// NEW: Phase 4 - Callback System
    /// </summary>
    public struct TweenCallbackComponent
    {
        public Action OnComplete; // Parameterless Action only (no data passing)

        public bool HasCallback => OnComplete != null;
    }

    /// <summary>
    /// Callback storage for tween cancellation.
    /// Invoked when a cancelled tween is actually destroyed by the ECS system.
    /// NEW: Cancellation callback support
    /// </summary>
    public struct TweenCancelCallbackComponent
    {
        public Action OnCancelled; // Invoked when tween is destroyed after cancellation

        public bool HasCallback => OnCancelled != null;
    }

    /// <summary>
    /// Bridge component that stores the actual UI element target.
    /// For UGUI: stores RectTransform reference.
    /// </summary>
    public struct TweenTargetComponent
    {
        public RectTransform RectTransform;
        public CanvasGroup CanvasGroup;
        public UnityEngine.UI.Image Image; // NEW: Phase 2 (for color tweens)
    }

    /// <summary>
    /// Flags which property types this tween should apply.
    /// Used by apply system to selectively update only active animation targets.
    /// </summary>
    public struct TweenApplyFlagsComponent
    {
        public bool ApplyAlpha;
        public bool ApplyPosition;
        public bool ApplyScale;
        public bool ApplyRotation;
        public bool ApplyColor;      // NEW: Phase 2
        public bool ApplySizeDelta;  // NEW: Phase 2
        public bool ApplyPivot;      // NEW: Phase 2
    }

    /// <summary>
    /// Tracks delay before a tween animation starts.
    /// DelayNotified flag ensures we only process delay completion once per tween.
    /// </summary>
    public struct TweenDelayComponent
    {
        public float DelayTime;        // Total delay in seconds
        public float ElapsedDelay;     // How much delay has elapsed
        public bool DelayNotified;     // Flag: delay completion has been processed (prevents re-processing each frame)

        public bool IsDelayFinished => ElapsedDelay >= DelayTime;
    }

    /// <summary>
    /// Loop settings for repeat behavior.
    /// NEW: Phase 4 - Loop/Repeat support
    /// </summary>
    public struct TweenLoopComponent
    {
        public enum LoopMode : byte
        {
            None = 0,           // Play once (default)
            Repeat = 1,         // Repeat N times
            Infinite = 2,       // Loop forever
            Reverse = 3,        // Play, then reverse
            PingPong = 4,       // Oscillate between start/end
        }

        public LoopMode Mode;
        public int LoopCount;           // For Repeat: how many times to repeat
        public int CurrentLoop;         // Current loop iteration
        public bool ShouldReverse;      // Flag to reverse on next cycle
    }

    /// <summary>
    /// Advanced callback support for OnStart and OnUpdate.
    /// NEW: Phase 4 - Advanced callbacks
    /// </summary>
    public struct TweenAdvancedCallbackComponent
    {
        public Action OnStart;      // Called when delay finishes and tween starts
        public Action OnUpdate;     // Called every frame while tweening
        public bool StartCalled;    // Flag: prevent multiple OnStart calls
    }

    /// <summary>
    /// Custom float property animation (for flexible user-defined properties).
    /// NEW: Phase 4 - Custom properties
    /// </summary>
    public struct TweenCustomFloatComponent
    {
        public float StartValue;
        public float EndValue;
        public float CurrentValue;  // Computed result
    }

    /// <summary>
    /// Custom vector property animation (for flexible user-defined properties).
    /// NEW: Phase 4 - Custom properties
    /// </summary>
    public struct TweenCustomVectorComponent
    {
        public Vector3 StartValue;
        public Vector3 EndValue;
        public Vector3 CurrentValue; // Computed result
    }

    /// <summary>
    /// Marker component that indicates this tween entity is ACTIVE and should be processed.
    /// Pool entities without this component are ignored by the math system.
    /// This prevents processing of 400+ inactive pool entities every frame.
    /// </summary>
    public struct TweenActiveMarkerComponent
    {
        // Empty marker component - presence indicates active tween
    }
}
