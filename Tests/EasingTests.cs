using System;
using System.Collections.Generic;
using NUnit.Framework;
using UITweens.Math;

namespace UITweens.Tests
{
    /// <summary>The easing curves themselves, without any tween running.</summary>
    public class EasingTests
    {
        private static IEnumerable<EaseType> AllEases()
            => (EaseType[])Enum.GetValues(typeof(EaseType));

        private static readonly EaseType[] Monotonic =
        {
            EaseType.Linear,
            EaseType.QuadIn, EaseType.QuadOut, EaseType.QuadInOut,
            EaseType.CubicIn, EaseType.CubicOut, EaseType.CubicInOut,
            EaseType.QuartIn, EaseType.QuartOut, EaseType.QuartInOut,
            EaseType.QuintIn, EaseType.QuintOut, EaseType.QuintInOut,
            EaseType.SineIn, EaseType.SineOut, EaseType.SineInOut,
            EaseType.ExpoIn, EaseType.ExpoOut, EaseType.ExpoInOut,
            EaseType.CircIn, EaseType.CircOut, EaseType.CircInOut,
        };

        private static readonly EaseType[] Symmetric =
        {
            EaseType.QuadInOut, EaseType.CubicInOut, EaseType.QuartInOut, EaseType.QuintInOut,
            EaseType.SineInOut, EaseType.ExpoInOut, EaseType.CircInOut,
            EaseType.BackInOut, EaseType.BounceInOut, EaseType.ElasticInOut,
        };

        [TestCaseSource(nameof(AllEases))]
        public void StartsAtZero(EaseType ease)
            => Assert.That(TweenMath.EvaluateEase(0f, ease), Is.EqualTo(0f).Within(0.001f));

        [TestCaseSource(nameof(AllEases))]
        public void EndsAtOne_ExceptPunch(EaseType ease)
        {
            float expected = ease == EaseType.PunchOut ? 0f : 1f;
            Assert.That(TweenMath.EvaluateEase(1f, ease), Is.EqualTo(expected).Within(0.001f));
        }

        [TestCaseSource(nameof(AllEases))]
        public void ClampsTimeOutsideZeroToOne(EaseType ease)
        {
            Assert.That(TweenMath.EvaluateEase(-0.5f, ease), Is.EqualTo(TweenMath.EvaluateEase(0f, ease)));
            Assert.That(TweenMath.EvaluateEase(1.5f, ease), Is.EqualTo(TweenMath.EvaluateEase(1f, ease)));
        }

        [TestCase(EaseType.Linear, 0.5f)]
        [TestCase(EaseType.QuadIn, 0.25f)]
        [TestCase(EaseType.QuadOut, 0.75f)]
        [TestCase(EaseType.CubicIn, 0.125f)]
        [TestCase(EaseType.CubicOut, 0.875f)]
        [TestCase(EaseType.QuartIn, 0.0625f)]
        [TestCase(EaseType.QuintIn, 0.03125f)]
        public void KnownMidpoints(EaseType ease, float expected)
            => Assert.That(TweenMath.EvaluateEase(0.5f, ease), Is.EqualTo(expected).Within(0.0001f));

        [TestCaseSource(nameof(Symmetric))]
        public void InOutCurvesPassThroughTheMiddle(EaseType ease)
            => Assert.That(TweenMath.EvaluateEase(0.5f, ease), Is.EqualTo(0.5f).Within(0.001f));

        [TestCaseSource(nameof(Monotonic))]
        public void NeverGoesBackwards(EaseType ease)
        {
            float previous = TweenMath.EvaluateEase(0f, ease);
            for (int i = 1; i <= 50; i++)
            {
                float value = TweenMath.EvaluateEase(i / 50f, ease);
                Assert.That(value, Is.GreaterThanOrEqualTo(previous - 0.0001f), $"t = {i / 50f}");
                previous = value;
            }
        }

        [Test]
        public void BackOutOvershootsAndItsParameterControlsHowMuch()
        {
            float defaultPeak = Peak(EaseType.BackOut, -1f);
            float strongPeak = Peak(EaseType.BackOut, 3f);

            Assert.That(defaultPeak, Is.GreaterThan(1f));
            Assert.That(strongPeak, Is.GreaterThan(defaultPeak));
        }

        [Test]
        public void BackInDipsBelowZero()
            => Assert.That(TweenMath.EvaluateEase(0.2f, EaseType.BackIn), Is.LessThan(0f));

        [Test]
        public void ElasticOutOvershoots()
            => Assert.That(Peak(EaseType.ElasticOut, -1f), Is.GreaterThan(1f));

        private static float Peak(EaseType ease, float parameter)
        {
            float peak = float.MinValue;
            for (int i = 0; i <= 100; i++)
                peak = System.Math.Max(peak, TweenMath.EvaluateEase(i / 100f, ease, parameter));
            return peak;
        }
    }
}
