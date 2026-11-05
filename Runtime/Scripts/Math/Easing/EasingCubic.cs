namespace UITweens.Math.Easing
{
    internal static class EasingCubic
    {
        internal static float EvaluateCubicIn(float t)
            => t * t * t;

        internal static float EvaluateCubicOut(float t)
            => 1f + (t - 1f) * (t - 1f) * (t - 1f);

        internal static float EvaluateCubicInOut(float t)
            => t < 0.5f ? 4f * t * t * t : 1f + 4f * (t - 1f) * (t - 1f) * (t - 1f);
    }
}

