namespace Tools.UITweens.Runtime.Scripts.Math.Easing
{
    internal static class EasingBack
    {
        private const float DEFAULT_OVERSHOOT = 1.70158f;

        internal static float EvaluateBackIn(float t, float parameter)
        {
            float overshoot = parameter > 0f ? parameter : DEFAULT_OVERSHOOT;
            return t * t * ((overshoot + 1f) * t - overshoot);
        }

        internal static float EvaluateBackOut(float t, float parameter)
        {
            float overshoot = parameter > 0f ? parameter : DEFAULT_OVERSHOOT;
            return 1f + (t - 1f) * (t - 1f) * ((overshoot + 1f) * (t - 1f) + overshoot);
        }

        internal static float EvaluateBackInOut(float t, float parameter)
        {
            float overshoot = parameter > 0f ? parameter : DEFAULT_OVERSHOOT;
            float c2 = overshoot * 1.525f;

            if (t < 0.5f)
            {
                float v = 2f * t;
                return (v * v * ((c2 + 1f) * 2f * t - c2)) * 0.5f;
            }

            float u = 2f * t - 2f;
            return (u * u * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) * 0.5f;
        }
    }
}

