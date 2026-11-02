namespace UITweens.Math.Easing
{
    internal static class EasingQuintic
    {
        internal static float EvaluateQuintIn(float t)
            => t * t * t * t * t;

        internal static float EvaluateQuintOut(float t)
        {
            float delta = 1f - t;
            return 1f - delta * delta * delta * delta * delta;
        }

        internal static float EvaluateQuintInOut(float t)
        {
            if (t < 0.5f)
                return 16f * t * t * t * t * t;

            float v = -2f * t + 2f;
            return 1f - v * v * v * v * v / 2f;
        }
    }
}

