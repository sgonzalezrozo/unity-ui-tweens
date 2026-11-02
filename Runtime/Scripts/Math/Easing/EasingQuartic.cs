namespace Tools.UITweens.Runtime.Scripts.Math.Easing
{
    internal static class EasingQuartic
    {
        internal static float EvaluateQuartIn(float t)
            => t * t * t * t;

        internal static float EvaluateQuartOut(float t)
        {
            float delta = 1f - t;
            return 1f - delta * delta * delta * delta;
        }

        internal static float EvaluateQuartInOut(float t)
        {
            if (t < 0.5f)
                return 8f * t * t * t * t;

            float v = -2f * t + 2f;
            return 1f - v * v * v * v / 2f;
        }
    }
}

