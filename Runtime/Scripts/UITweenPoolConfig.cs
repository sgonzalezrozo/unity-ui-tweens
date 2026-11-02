using UnityEngine;

namespace UITweens
{
    public class UITweenPoolConfig : ScriptableObject
    {
        [Header("Pool Warm-Up Configuration")]
        [Tooltip("Maximum concurrent alpha tweens. Default ~40 for typical mobile UI.")]
        public int MaxConcurrentAlphaTweens = 40;

        [Tooltip("Maximum concurrent position tweens. Default ~60 for typical mobile UI.")]
        public int MaxConcurrentPositionTweens = 60;

        [Tooltip("Maximum concurrent scale tweens. Default ~30 for typical mobile UI.")]
        public int MaxConcurrentScaleTweens = 30;

        [Tooltip("Maximum concurrent rotation tweens. Default ~20 for typical mobile UI.")]
        public int MaxConcurrentRotationTweens = 20;

        // NEW: Phase 2 - Color animations
        [Tooltip("Maximum concurrent color tweens. Default ~30 for typical mobile UI.")]
        public int MaxConcurrentColorTweens = 30;

        // NEW: Phase 2 - Size animations
        [Tooltip("Maximum concurrent size delta tweens. Default ~20 for typical mobile UI.")]
        public int MaxConcurrentSizeTweens = 20;

        // NEW: Phase 2 - Pivot animations
        [Tooltip("Maximum concurrent pivot tweens. Default ~10 for typical mobile UI.")]
        public int MaxConcurrentPivotTweens = 10;

        [Header("Advanced")]
        [Tooltip("If true, logs pool allocations at startup for profiling.")]
        public bool LogPoolAllocations = false;

        public static UITweenPoolConfig Instance { get; private set; }

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
        }

        public int TotalEstimatedEntities
            => MaxConcurrentAlphaTweens + MaxConcurrentPositionTweens + MaxConcurrentScaleTweens +
               MaxConcurrentRotationTweens + MaxConcurrentColorTweens + MaxConcurrentSizeTweens +
               MaxConcurrentPivotTweens;
    }
}

