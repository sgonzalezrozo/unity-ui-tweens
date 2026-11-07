using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Math;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Tests
{
    /// <summary>Then() and Sequential().</summary>
    public class SequenceTests : TweenTestBase
    {
        [Test]
        public void Then_OnTheSameProperty_StartsWhereTheFirstEnded()
        {
            RectTransform panel = CreateRect();
            panel.anchoredPosition = new Vector2(100f, 100f);
            int completions = 0;

            // The README example.
            panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut)
                .Then(panel.DOAnchorPosX(40f, 0.2f).GetHandle())
                .OnComplete(() => completions++);

            // A few frames past the first tween: it has ended at (0, 0) and the second one has
            // just started moving X from there, not from the first tween's start at 100.
            StepPast(0.35f);
            AssertNear(0f, panel.anchoredPosition.y, 0.5f, "first tween finished");
            Assert.That(panel.anchoredPosition.x, Is.InRange(0f, 10f), "second tween starts where the first ended");
            Assert.That(completions, Is.EqualTo(0), "OnComplete belongs to the second tween");

            StepPast(0.2f);
            AssertNear(new Vector2(40f, 0f), panel.anchoredPosition);
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void Then_SecondTweenWaitsForTheFirst()
        {
            RectTransform first = CreateRect();
            RectTransform second = CreateRect();
            second.anchoredPosition = Vector2.zero;

            first.DOAnchorPosX(100f, 0.5f).Then(second.DOAnchorPosX(100f, 0.5f).GetHandle());

            Step(0.4f);
            AssertNear(0f, second.anchoredPosition.x, 0.001f);

            StepPast(0.6f);
            AssertNear(100f, second.anchoredPosition.x);
        }

        [Test]
        public void Then_CancellingTheFirstCancelsTheNext()
        {
            RectTransform first = CreateRect();
            RectTransform second = CreateRect();
            second.anchoredPosition = Vector2.zero;

            first.DOAnchorPosX(100f, 0.5f).Then(second.DOAnchorPosX(100f, 0.5f).GetHandle());

            Step(0.2f);
            first.DoCancel();
            Step(1.5f);

            AssertNear(0f, second.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Sequential_RunsInOrderWithWaits()
        {
            RectTransform a = CreateRect();
            RectTransform b = CreateRect();
            a.anchoredPosition = Vector2.zero;
            b.anchoredPosition = Vector2.zero;
            TweenSpawner spawner = TweenManager.GetSpawner();

            spawner.Sequential(
                a.DOAnchorPosX(100f, 0.2f).DeclareOnly(),
                spawner.Wait(0.3f),
                b.DOAnchorPosX(100f, 0.2f).DeclareOnly());

            Step(0.1f);
            AssertNear(50f, a.anchoredPosition.x, 6f);
            AssertNear(0f, b.anchoredPosition.x, 0.001f);

            Step(0.35f);
            AssertNear(100f, a.anchoredPosition.x);
            AssertNear(0f, b.anchoredPosition.x, 0.001f);

            Step(0.4f);
            AssertNear(100f, b.anchoredPosition.x);
        }

        [Test]
        public void DeclaredTween_DoesNotStartUntilSequenced()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).DeclareOnly();
            Step(0.5f);

            AssertNear(0f, rect.anchoredPosition.x, 0.001f);
        }
    }
}
