using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Services;
using UnityEngine;

namespace UITweens.Tests
{
    /// <summary>Delay and loops.</summary>
    public class TimingTests : TweenTestBase
    {
        [Test]
        public void Delay_WaitsBeforeMoving()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 1f).Delay(0.5f);

            Step(0.4f);
            AssertNear(0f, rect.anchoredPosition.x, 0.001f);

            Step(0.6f);
            AssertNear(50f, rect.anchoredPosition.x, 2f);

            StepPast(0.5f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void Delay_OnStartRunsWhenTheDelayEnds()
        {
            int starts = 0;
            RectTransform rect = CreateRect();

            rect.DOAnchorPosX(100f, 0.5f).Delay(0.5f).OnStart(() => starts++);

            Step(0.4f);
            Assert.That(starts, Is.EqualTo(0));

            StepPast(0.6f);
            Assert.That(starts, Is.EqualTo(1));
        }

        [Test]
        public void ZeroOrNegativeDelay_StartsAtOnce()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 1f).Delay(0f);
            Step(0.5f);

            AssertNear(50f, rect.anchoredPosition.x, 0.5f);
        }

        [Test]
        public void Loop_PlaysOneMoreTimeThanTheRepeatCount()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Loop(2).OnComplete(() => completions++);

            Step(0.55f);
            Assert.That(completions, Is.EqualTo(0), "finished before its third play");

            StepPast(0.05f);
            Assert.That(completions, Is.EqualTo(1));
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void Loop_RestartsFromTheBeginning()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Loop(1);

            Step(0.25f);
            Assert.That(rect.anchoredPosition.x, Is.LessThan(50f), "second play should start again near 0");
        }

        [Test]
        public void LoopInfinite_KeepsPlayingUntilCancelled()
        {
            int completions = 0;
            RectTransform rect = CreateRect();

            TweenHandle handle = rect.DOAnchorPosX(100f, 0.2f).LoopInfinite().OnComplete(() => completions++).GetHandle();

            Step(3f);
            Assert.That(handle.IsPlaying, Is.True);
            Assert.That(completions, Is.EqualTo(0));

            rect.DoCancel();
            StepFrames(2);
            Assert.That(handle.IsPlaying, Is.False);
        }
    }
}
