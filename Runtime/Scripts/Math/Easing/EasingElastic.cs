using UnityEngine;

namespace UITweens.Math.Easing
{
    internal static class EasingElastic
    {
        private const float DEFAULT_AMPLITUDE = 0.5f;
        private const float TWO_PI = Mathf.PI * 2f;

        internal static float EvaluateElasticIn(float t, float parameter)
        {
            float amplitude = parameter > 0f ? parameter : DEFAULT_AMPLITUDE;

            if (t == 0f)
                return 0f;

            if (t == 1f)
                return 1f;

            float period = 0.3f;
            float a = Mathf.Max(amplitude, 1f);
            float s = period / TWO_PI * Mathf.Asin(1f / a);

            return -(a * Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1f - s) * TWO_PI / period));
        }

        internal static float EvaluateElasticOut(float t, float parameter)
        {
            float amplitude = parameter > 0f ? parameter : DEFAULT_AMPLITUDE;

            if (t == 0f)
                return 0f;
            if (t == 1f)
                return 1f;

            float period = 0.3f;
            float a = Mathf.Max(amplitude, 1f);
            float s = period / TWO_PI * Mathf.Asin(1f / a);

            return a * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * TWO_PI / period) + 1f;
        }

        internal static float EvaluateElasticInOut(float t, float parameter)
        {
            float amplitude = parameter > 0f ? parameter : DEFAULT_AMPLITUDE;

            if (t == 0f)
                return 0f;
            if (t == 1f)
                return 1f;

            float period = 0.45f;
            float a = Mathf.Max(amplitude, 1f);
            float s = period / TWO_PI * Mathf.Asin(1f / a);

            t = 2f * t - 1f;

            return t < 0f
                ? -(a * Mathf.Pow(2f, 10f * t) * Mathf.Sin((t - s) * TWO_PI / period)) * 0.5f
                : a * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * TWO_PI / period) * 0.5f + 1f;
        }
    }
}

