using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Math.Easing
{
    internal static class EasingSpecial
    {
        private const float HALF_PI = Mathf.PI * 0.5f;
        private const float TWO_PI = Mathf.PI * 2f;
        private const float DEFAULT_BOUNCINESS = 2.5f;

        internal static float EvaluateSpringOut(float t, float parameter)
        {
            return Mathf.Sin(-13f * HALF_PI * (t + 1f)) * Mathf.Pow(2f, -10f * t) + 1f;
        }

        internal static float EvaluatePunchOut(float t, float parameter)
        {
            float strength = parameter > 0f ? parameter : DEFAULT_BOUNCINESS;
            if (t == 0f)
                return 0f;

            if (t == 1f)
                return 0f;

            return -strength * Mathf.Pow(2f, 10f * (t - 1f)) *
                   Mathf.Sin((t - strength / 4f) * (TWO_PI) / strength);
        }
    }
}



