using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UITweens.Extensions;
using UnityEngine;
using UnityEngine.TestTools;

namespace UITweens.Tests
{
    public class CallbackTests : TweenTestBase
    {
        [Test]
        public void OnComplete_RunsOnceAtTheEnd()
        {
            int completions = 0;
            RectTransform rect = CreateRect();

            rect.DOAnchorPosX(100f, 0.5f).OnComplete(() => completions++);

            Step(0.45f);
            Assert.That(completions, Is.EqualTo(0));

            StepPast(0.05f);
            Step(0.5f);
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void OnComplete_ValueIsFinalWhenItRuns()
        {
            RectTransform rect = CreateRect();
            float xAtComplete = float.NaN;

            rect.DOAnchorPosX(100f, 0.5f).OnComplete(() => xAtComplete = rect.anchoredPosition.x);

            StepPast(0.5f);
            AssertNear(100f, xAtComplete);
        }

        [Test]
        public void OnCompleteWithState_PassesTheState()
        {
            RectTransform rect = CreateRect();
            RectTransform received = null;

            rect.DOAnchorPosX(100f, 0.5f).OnComplete(rect, target => received = target);

            StepPast(0.5f);
            Assert.That(received, Is.SameAs(rect));
        }

        [Test]
        public void OnStart_RunsOnce()
        {
            int starts = 0;
            RectTransform rect = CreateRect();

            rect.DOAnchorPosX(100f, 0.5f).OnStart(() => starts++);

            StepFrames(1);
            Assert.That(starts, Is.EqualTo(1));

            StepPast(0.5f);
            Assert.That(starts, Is.EqualTo(1));
        }

        [Test]
        public void OnUpdate_RunsEveryFrameWhilePlayingOnly()
        {
            int updates = 0;
            RectTransform rect = CreateRect();

            rect.DOAnchorPosX(100f, 0.5f).OnUpdate(() => updates++);

            StepPast(0.5f);
            Assert.That(updates, Is.InRange(48, 54));

            int afterEnd = updates;
            Step(0.5f);
            Assert.That(updates, Is.EqualTo(afterEnd));
        }

        [Test]
        public void ThrowingCallback_IsLoggedAndOtherTweensKeepRunning()
        {
            RectTransform faulty = CreateRect();
            RectTransform healthy = CreateRect();
            healthy.anchoredPosition = Vector2.zero;

            faulty.DOAnchorPosX(100f, 0.2f).OnComplete(() => throw new InvalidOperationException("Expected test exception from a callback"));
            healthy.DOAnchorPosX(100f, 0.5f);

            LogAssert.Expect(LogType.Exception, new Regex("Expected test exception"));
            StepPast(0.5f);

            AssertNear(100f, healthy.anchoredPosition.x);
        }
    }
}
