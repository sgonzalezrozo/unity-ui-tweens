using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Math.Easing
{
    internal static class EasingExponential
    {
        internal static float EvaluateExpoIn(float t)
            => t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);

        internal static float EvaluateExpoOut(float t)
            => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);

        internal static float EvaluateExpoInOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f
                ? Mathf.Pow(2f, 20f * t - 10f) / 2f
                : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }
    }
}

