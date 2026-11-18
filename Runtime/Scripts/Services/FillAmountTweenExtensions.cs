using UITweens.Math;
using UnityEngine;

namespace UITweens.Services
{
    /// <summary>
    /// Fill amount animation extensions for TweenSpawner: progress bars, cooldowns and radial timers on an Image.
    /// </summary>
    public static class FillAmountTweenExtensions
    {
        /// <summary>
        /// Tween from the current fill amount to a target (0 to 1).
        /// </summary>
        public static TweenBuilder FillAmount(
            this TweenSpawner spawner,
            UnityEngine.UI.Image target,
            float targetFill,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Image target is null");
                return new TweenBuilder(spawner);
            }

            return spawner.FillAmountFromTo(target, target.fillAmount, targetFill, duration, ease, easeParameter)
                .StartFromCurrent();
        }

        /// <summary>
        /// Tween the fill amount between two values (0 to 1).
        /// </summary>
        public static TweenBuilder FillAmountFromTo(
            this TweenSpawner spawner,
            UnityEngine.UI.Image target,
            float startFill,
            float endFill,
            float duration,
            EaseType ease = EaseType.Linear,
            float easeParameter = -1f)
        {
            if (target == null)
            {
                Debug.LogError("[UITweens] Image target is null");
                return new TweenBuilder(spawner);
            }

            RectTransform rect = target.rectTransform;
            TweenHandle handle = spawner.SpawnTweenInternal(rect, duration, ease, easeParameter, graphic: target, kind: TweenPoolKind.FillAmount);
            spawner.InitializeFillAmountInternal(handle, startFill, endFill);
            return new TweenBuilder(spawner).SetHandle(handle);
        }
    }
}
