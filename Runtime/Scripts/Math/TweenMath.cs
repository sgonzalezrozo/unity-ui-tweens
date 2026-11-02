using UITweens.Math.Easing;
using UnityEngine;

namespace UITweens.Math
{
    public enum EaseType : byte
    {
        Linear,
        QuadIn, QuadOut, QuadInOut,
        CubicIn, CubicOut, CubicInOut,
        QuartIn, QuartOut, QuartInOut,
        QuintIn, QuintOut, QuintInOut,
        SineIn, SineOut, SineInOut,
        ExpoIn, ExpoOut, ExpoInOut,
        CircIn, CircOut, CircInOut,
        BackIn, BackOut, BackInOut,
        BounceIn, BounceOut, BounceInOut,
        ElasticIn, ElasticOut, ElasticInOut,
        SpringOut, PunchOut,
    }

    public static class TweenMath
    {
        public static float EvaluateEase(float t, EaseType easeType, float parameter = -1f)
            => EasingCurve.Evaluate(t, easeType, parameter);

        public static float Lerp(float start, float end, float easedT)
            => start + (end - start) * easedT;

        public static Vector2 LerpVector2(Vector2 start, Vector2 end, float easedT)
            => new Vector2(
                start.x + (end.x - start.x) * easedT,
                start.y + (end.y - start.y) * easedT
            );

        public static Vector3 LerpVector3(Vector3 start, Vector3 end, float easedT)
            => new Vector3(
                start.x + (end.x - start.x) * easedT,
                start.y + (end.y - start.y) * easedT,
                start.z + (end.z - start.z) * easedT
            );

        public static Color LerpColor(Color start, Color end, float easedT)
            => new Color(
                start.r + (end.r - start.r) * easedT,
                start.g + (end.g - start.g) * easedT,
                start.b + (end.b - start.b) * easedT,
                start.a + (end.a - start.a) * easedT
            );
    }
}
