using Tools.UITweens.Runtime.Scripts.Math;
using UnityEngine;

namespace Tools.UITweens.Runtime.Scripts.Services
{
    /// <summary>
    /// Evaluates easing curves to transform linear progress (0-1) into eased progress.
    /// Single Responsibility: Easing math only
    /// </summary>
    public class EasingCurveEvaluator
    {
        /// <summary>
        /// Evaluate easing function at normalized time (0-1).
        /// Returns eased progress value.
        /// </summary>
        public float Evaluate(float normalizedTime, EaseType easeType, float easeParameter = -1f)
        {
            if (normalizedTime <= 0f)
                return 0f;
            if (normalizedTime >= 1f)
                return 1f;

            return EvaluateEase(normalizedTime, easeType, easeParameter);
        }

        private float EvaluateEase(float t, EaseType ease, float parameter)
        {
            return ease switch
            {
                EaseType.Linear => t,
                EaseType.QuadIn => t * t,
                EaseType.QuadOut => 1f - (1f - t) * (1f - t),
                EaseType.QuadInOut => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
                EaseType.CubicIn => t * t * t,
                EaseType.CubicOut => 1f - Mathf.Pow(1f - t, 3f),
                EaseType.CubicInOut => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f,
                EaseType.ElasticIn => ElasticIn(t, parameter),
                EaseType.ElasticOut => ElasticOut(t, parameter),
                EaseType.ElasticInOut => ElasticInOut(t, parameter),
                EaseType.BounceIn => 1f - BounceOut(1f - t),
                EaseType.BounceOut => BounceOut(t),
                EaseType.BounceInOut => BounceInOut(t),
                _ => t,
            };
        }

        private float ElasticIn(float t, float parameter)
        {
            const float C4 = (2f * Mathf.PI) / 3f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * C4);
        }

        private float ElasticOut(float t, float parameter)
        {
            const float C4 = (2f * Mathf.PI) / 3f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * C4) + 1f;
        }

        private float ElasticInOut(float t, float parameter)
        {
            const float C5 = (2f * Mathf.PI) / 4.5f;
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            if (t < 0.5f)
                return -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * C5)) / 2f;
            return (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * C5)) / 2f + 1f;
        }

        private float BounceOut(float t)
        {
            const float N1 = 7.5625f;
            const float D1 = 2.75f;

            if (t < 1f / D1)
                return N1 * t * t;
            if (t < 2f / D1)
                return N1 * (t -= 1.5f / D1) * t + 0.75f;
            if (t < 2.5f / D1)
                return N1 * (t -= 2.25f / D1) * t + 0.9375f;
            return N1 * (t -= 2.625f / D1) * t + 0.984375f;
        }

        private float BounceInOut(float t)
        {
            if (t < 0.5f)
                return (1f - BounceOut(1f - 2f * t)) / 2f;
            return (1f + BounceOut(2f * t - 1f)) / 2f;
        }
    }
}

