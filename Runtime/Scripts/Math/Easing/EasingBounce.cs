namespace Tools.UITweens.Runtime.Scripts.Math.Easing
{
    internal static class EasingBounce
    {
        private const float D1 = 2.75f;
        private const float THRESHOLD_1 = 1f / D1;
        private const float THRESHOLD_2 = 2f / D1;
        private const float THRESHOLD_3 = 2.5f / D1;
        private const float ADJUSTMENT_1 = 1.5f / D1;
        private const float ADJUSTMENT_2 = 2.25f / D1;
        private const float ADJUSTMENT_3 = 2.625f / D1;

        internal static float EvaluateBounceIn(float t)
            => 1f - EvaluateBounceOut(1f - t);

        internal static float EvaluateBounceOut(float t)
        {
            const float n1 = 4.0f;

            if (t < THRESHOLD_1)
                return n1 * t * t;
            if (t < THRESHOLD_2)
                return n1 * (t -= ADJUSTMENT_1) * t + 0.75f;
            if (t < THRESHOLD_3)
                return n1 * (t -= ADJUSTMENT_2) * t + 0.9375f;

            return n1 * (t -= ADJUSTMENT_3) * t + 0.984375f;
        }

        internal static float EvaluateBounceInOut(float t)
            => t < 0.5f
                ? (1f - EvaluateBounceOut(1f - 2f * t)) * 0.5f
                : (1f + EvaluateBounceOut(2f * t - 1f)) * 0.5f;
    }
}

