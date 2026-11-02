namespace UITweens.Math.Easing
{
    internal static class EasingQuadratic
    {
        internal static float EvaluateQuadIn(float t)
            => t * t;

        internal static float EvaluateQuadOut(float t)
            => t * (2f - t);

        internal static float EvaluateQuadInOut(float t)
            => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }
}

