using NUnit.Framework;
using UITweens.Extensions;
using UnityEngine;

namespace UITweens.Tests
{
    /// <summary>A new tween on the same property replaces the old one; other properties are left alone.</summary>
    public class ReplacementTests : TweenTestBase
    {
        [Test]
        public void NewTweenOnTheSameProperty_ReplacesTheOldOne()
        {
            int oldCompletions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 1f).OnComplete(() => oldCompletions++);
            Step(0.3f);
            rect.DOAnchorPosX(-50f, 0.5f);

            StepPast(1f);
            AssertNear(-50f, rect.anchoredPosition.x);
            Assert.That(oldCompletions, Is.EqualTo(0));
        }

        [Test]
        public void ReplacementStartsFromTheCurrentValue()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 1f);
            Step(0.5f);
            float current = rect.anchoredPosition.x;
            rect.DOAnchorPosX(0f, 1f);

            StepFrames(1);
            AssertNear(current, rect.anchoredPosition.x, 2f, "no jump back to the old start");
        }

        [Test]
        public void TweensOnDifferentProperties_RunTogether()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.5f);
            rect.DOScale(Vector3.one * 2f, 0.5f);
            rect.DORotate(45f, 0.5f);

            StepPast(0.5f);
            AssertNear(100f, rect.anchoredPosition.x);
            AssertNear(Vector3.one * 2f, rect.localScale);
            AssertNear(45f, rect.eulerAngles.z, 0.05f);
        }

        [Test]
        public void TweensOnDifferentTargets_AreIndependent()
        {
            RectTransform a = CreateRect();
            RectTransform b = CreateRect();

            a.DOAnchorPosX(100f, 0.5f);
            b.DOAnchorPosX(-100f, 0.5f);

            StepPast(0.5f);
            AssertNear(100f, a.anchoredPosition.x);
            AssertNear(-100f, b.anchoredPosition.x);
        }

        [Test]
        public void DelayedReplacement_LetsTheOldTweenRunUntilItStarts()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 2f);
            Step(0.2f);
            rect.DOAnchorPosX(-50f, 0.5f).Delay(0.5f);

            Step(0.3f);
            Assert.That(rect.anchoredPosition.x, Is.GreaterThan(10f), "old tween keeps running during the delay");

            StepPast(1f);
            AssertNear(-50f, rect.anchoredPosition.x);
        }
    }
}
