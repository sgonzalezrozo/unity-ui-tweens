using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Math.Easing
{
    internal static class EasingCircular
    {
        internal static float EvaluateCircIn(float t)
            => 1f - Mathf.Sqrt(1f - (t * t));

        internal static float EvaluateCircOut(float t)
        {
            float delta = t - 1f;
            return Mathf.Sqrt(1f - delta * delta);
        }

        internal static float EvaluateCircInOut(float t)
        {
            if (t < 0.5f)
            {
                float v = 2f * t;
                return (1f - Mathf.Sqrt(1f - v * v)) / 2f;
            }

            float u = -2f * t + 2f;
            return (Mathf.Sqrt(1f - u * u) + 1f) / 2f;
        }
    }
}

