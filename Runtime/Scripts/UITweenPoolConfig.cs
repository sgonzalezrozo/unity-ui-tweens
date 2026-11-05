using UnityEngine;

namespace UITweens
{
    public class UITweenPoolConfig : ScriptableObject
    {
        [Header("Pool")]
        [Tooltip("Tween entities created at startup. Lower values reduce startup work.")]
        [Min(0)] public int InitialPoolSize = 32;

        [Tooltip("When this many entities or fewer are left, the pool refills on the next update.")]
        [Min(0)] public int RefillThreshold = 2;

        [Tooltip("Entities added by each refill. 0 turns refills off.")]
        [Min(0)] public int GrowthBatchSize = 12;

        [Header("Advanced")]
        [Tooltip("If true, logs the pool size at startup.")]
        public bool LogPoolAllocations = false;

        public static UITweenPoolConfig Instance { get; private set; }

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}
