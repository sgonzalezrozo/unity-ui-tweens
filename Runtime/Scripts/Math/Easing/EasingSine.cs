using UnityEngine;

namespace UITweens.Math.Easing
{
    internal static class EasingSine
    {
        internal static float EvaluateSineIn(float t)
            => 1f - Mathf.Cos((t * Mathf.PI) / 2f);

        internal static float EvaluateSineOut(float t)
            => Mathf.Sin((t * Mathf.PI) / 2f);

        internal static float EvaluateSineInOut(float t)
            => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
    }
}

