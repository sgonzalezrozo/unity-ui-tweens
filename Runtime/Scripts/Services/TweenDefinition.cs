using System;
using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    public struct TweenDefinition
    {
        public UnityEngine.Object TargetComponent;
        public TweenPropertyType PropertyType;
        public float Duration;
        public EaseType Ease;
        public float EaseParameter;
        public float RemainingDelay;
        public TweenHandle PredecessorHandle;

        public RectTransform RectTransform;
        public CanvasGroup CanvasGroup;
        public UnityEngine.UI.Image Image;

        public bool HasAlpha;
        public float StartAlpha;
        public float EndAlpha;

        public bool HasPosition;
        public Vector2 StartPosition;
        public Vector2 EndPosition;

        public bool HasScale;
        public Vector3 StartScale;
        public Vector3 EndScale;

        public bool HasRotation;
        public float StartRotation;
        public float EndRotation;

        public bool HasColor;
        public Color StartColor;
        public Color EndColor;

        public bool HasSizeDelta;
        public Vector2 StartSizeDelta;
        public Vector2 EndSizeDelta;

        public bool HasPivot;
        public Vector2 StartPivot;
        public Vector2 EndPivot;

        public Action OnComplete;
        public Action OnStart;
        public Action OnUpdate;
        public Action OnCancelled;

        public int LoopCount;
        public bool IsInfiniteLoop;
    }

    public enum TweenPropertyType
    {
        Alpha,
        Position,
        Scale,
        Rotation,
        Color,
        SizeDelta,
        Pivot,
        Unknown
    }
}

