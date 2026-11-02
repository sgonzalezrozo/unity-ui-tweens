using UnityEngine;

namespace UITweens.Math.Easing
{
    public static class EasingCurve
    {

        public static float Evaluate(float t, EaseType easeType, float parameter = -1f)
        {
            t = Mathf.Clamp01(t);

            return easeType switch
            {
                EaseType.Linear => t,

                EaseType.QuadIn => EasingQuadratic.EvaluateQuadIn(t),
                EaseType.QuadOut => EasingQuadratic.EvaluateQuadOut(t),
                EaseType.QuadInOut => EasingQuadratic.EvaluateQuadInOut(t),

                EaseType.CubicIn => EasingCubic.EvaluateCubicIn(t),
                EaseType.CubicOut => EasingCubic.EvaluateCubicOut(t),
                EaseType.CubicInOut => EasingCubic.EvaluateCubicInOut(t),

                EaseType.QuartIn => EasingQuartic.EvaluateQuartIn(t),
                EaseType.QuartOut => EasingQuartic.EvaluateQuartOut(t),
                EaseType.QuartInOut => EasingQuartic.EvaluateQuartInOut(t),

                EaseType.QuintIn => EasingQuintic.EvaluateQuintIn(t),
                EaseType.QuintOut => EasingQuintic.EvaluateQuintOut(t),
                EaseType.QuintInOut => EasingQuintic.EvaluateQuintInOut(t),

                EaseType.SineIn => EasingSine.EvaluateSineIn(t),
                EaseType.SineOut => EasingSine.EvaluateSineOut(t),
                EaseType.SineInOut => EasingSine.EvaluateSineInOut(t),

                EaseType.ExpoIn => EasingExponential.EvaluateExpoIn(t),
                EaseType.ExpoOut => EasingExponential.EvaluateExpoOut(t),
                EaseType.ExpoInOut => EasingExponential.EvaluateExpoInOut(t),

                EaseType.CircIn => EasingCircular.EvaluateCircIn(t),
                EaseType.CircOut => EasingCircular.EvaluateCircOut(t),
                EaseType.CircInOut => EasingCircular.EvaluateCircInOut(t),

                EaseType.BackIn => EasingBack.EvaluateBackIn(t, parameter),
                EaseType.BackOut => EasingBack.EvaluateBackOut(t, parameter),
                EaseType.BackInOut => EasingBack.EvaluateBackInOut(t, parameter),

                EaseType.BounceIn => EasingBounce.EvaluateBounceIn(t),
                EaseType.BounceOut => EasingBounce.EvaluateBounceOut(t),
                EaseType.BounceInOut => EasingBounce.EvaluateBounceInOut(t),

                EaseType.ElasticIn => EasingElastic.EvaluateElasticIn(t, parameter),
                EaseType.ElasticOut => EasingElastic.EvaluateElasticOut(t, parameter),
                EaseType.ElasticInOut => EasingElastic.EvaluateElasticInOut(t, parameter),

                EaseType.SpringOut => EasingSpecial.EvaluateSpringOut(t, parameter),
                EaseType.PunchOut => EasingSpecial.EvaluatePunchOut(t, parameter),

                _ => t,
            };
        }
    }
}

