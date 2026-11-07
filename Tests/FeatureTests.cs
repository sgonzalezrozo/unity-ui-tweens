using System.Collections;
using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Math;
using UITweens.Services;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UITweens.Tests
{
    /// <summary>IgnoreTimeScale(), Yoyo loops, From() and Relative().</summary>
    public class IgnoreTimeScaleTests : TweenTestBase
    {
        // One frame with the game paused: scaled time stands still, real time moves on.
        private static void StepPaused(float seconds)
        {
            int frames = Mathf.RoundToInt(seconds / Frame);
            for (int i = 0; i < frames; i++)
                TweenManager.Step(0f, Frame);
        }

        [Test]
        public void IgnoreTimeScale_KeepsPlayingWhileThePausedOneStops()
        {
            RectTransform menu = CreateRect();
            RectTransform world = CreateRect();
            menu.anchoredPosition = Vector2.zero;
            world.anchoredPosition = Vector2.zero;

            menu.DOAnchorPosX(100f, 0.5f).IgnoreTimeScale();
            world.DOAnchorPosX(100f, 0.5f);

            StepPaused(0.6f);

            AssertNear(100f, menu.anchoredPosition.x);
            AssertNear(0f, world.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Delay_AlsoUsesUnscaledTime()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Delay(0.3f).IgnoreTimeScale();

            StepPaused(0.6f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void IgnoreTimeScaleByDefault_AppliesToNewTweens()
        {
            TweenManager.IgnoreTimeScaleByDefault = true;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.5f);

            StepPaused(0.6f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void IgnoreTimeScaleFalse_OverridesTheDefault()
        {
            TweenManager.IgnoreTimeScaleByDefault = true;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.5f).IgnoreTimeScale(false);

            StepPaused(0.6f);
            AssertNear(0f, rect.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void ReusedEntity_DoesNotKeepTheSetting()
        {
            RectTransform first = CreateRect();
            first.DOAnchorPosX(100f, 0.1f).IgnoreTimeScale();
            StepPast(0.1f);

            RectTransform second = CreateRect();
            second.anchoredPosition = Vector2.zero;
            second.DOAnchorPosX(100f, 0.5f);

            StepPaused(0.6f);
            AssertNear(0f, second.anchoredPosition.x, 0.001f);
        }

        [UnityTest]
        public IEnumerator RealFrames_PlaysWithTimeScaleZero()
        {
            TweenManager.ManualUpdate = false;
            Time.timeScale = 0f;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).IgnoreTimeScale();
            yield return new WaitForSecondsRealtime(0.5f);

            AssertNear(100f, rect.anchoredPosition.x);
        }
    }

    public class YoyoTests : TweenTestBase
    {
        [Test]
        public void Yoyo_GoesThereAndBack()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Loop(1, LoopType.Yoyo).OnComplete(() => completions++);

            Step(0.1f);
            AssertNear(50f, rect.anchoredPosition.x, 6f);

            Step(0.2f);
            AssertNear(50f, rect.anchoredPosition.x, 6f);
            Assert.That(completions, Is.EqualTo(0));

            StepPast(0.1f);
            AssertNear(0f, rect.anchoredPosition.x);
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void Yoyo_WithAnEvenNumberOfPlays_EndsAtTheTarget()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Loop(2, LoopType.Yoyo);

            StepPast(0.6f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void YoyoInfinite_KeepsAlternatingDirection()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            TweenHandle handle = rect.DOAnchorPosX(100f, 0.2f).LoopInfinite(LoopType.Yoyo).GetHandle();

            Step(0.85f);   // fifth play, forward, a quarter in
            AssertNear(25f, rect.anchoredPosition.x, 8f);

            Step(0.2f);    // sixth play, backward, a quarter in
            AssertNear(75f, rect.anchoredPosition.x, 8f);

            Assert.That(handle.IsPlaying, Is.True);
        }

        [Test]
        public void Yoyo_RetracesTheSameCurveOnTheWayBack()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f, EaseType.QuadIn).Loop(1, LoopType.Yoyo);

            Step(0.1f);
            float forwardHalf = rect.anchoredPosition.x;

            Step(0.2f);
            float backwardHalf = rect.anchoredPosition.x;

            AssertNear(25f, forwardHalf, 3f);
            AssertNear(forwardHalf, backwardHalf, 6f, "the way back passes the same point at the same time");
        }

        [Test]
        public void Yoyo_WorksForCanvasGroupAndCustomFloat()
        {
            CanvasGroup group = CreateGroup();
            group.alpha = 1f;
            float last = -1f;

            group.DOFade(0f, 0.2f).Loop(1, LoopType.Yoyo);
            TweenManager.GetSpawner().CustomFloat(0f, 10f, 0.2f).Loop(1, LoopType.Yoyo).OnFloatUpdate(v => last = v);

            StepPast(0.4f);
            AssertNear(1f, group.alpha);
            AssertNear(0f, last);
        }

        [Test]
        public void RestartStillRestarts()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Loop(1, LoopType.Restart);

            StepPast(0.4f);
            AssertNear(100f, rect.anchoredPosition.x);
        }
    }

    public class FromTests : TweenTestBase
    {
        [Test]
        public void From_JumpsToTheGivenValueAndPlaysBack()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 1f).From();
            AssertNear(100f, rect.anchoredPosition.x, 0.001f, "jumps before the first frame");

            Step(0.5f);
            AssertNear(50f, rect.anchoredPosition.x, 0.5f);

            StepPast(0.5f);
            AssertNear(0f, rect.anchoredPosition.x);
        }

        [Test]
        public void From_LeavesOtherAxesAlone()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(10f, 20f);

            rect.DOAnchorPosY(-100f, 0.5f).From();
            AssertNear(new Vector2(10f, -100f), rect.anchoredPosition);

            StepPast(0.5f);
            AssertNear(new Vector2(10f, 20f), rect.anchoredPosition);
        }

        [Test]
        public void From_WithDelay_HoldsTheGivenValueDuringTheDelay()
        {
            RectTransform rect = CreateRect();
            rect.localScale = Vector3.one;

            rect.DOScale(Vector3.zero, 0.3f, EaseType.BackOut).From().Delay(0.5f);

            Step(0.4f);
            AssertNear(Vector3.zero, rect.localScale, 0.001f);

            StepPast(0.4f);
            AssertNear(Vector3.one, rect.localScale);
        }

        [Test]
        public void From_CanvasGroupAndImageFade()
        {
            CanvasGroup group = CreateGroup();
            Image image = CreateImage();
            group.alpha = 1f;
            image.color = new Color(1f, 0.5f, 0.25f, 1f);

            group.DOFade(0f, 0.5f).From();
            image.DOFade(0f, 0.5f).From();
            AssertNear(0f, group.alpha, 0.001f);
            AssertNear(new Color(1f, 0.5f, 0.25f, 0f), image.color);

            StepPast(0.5f);
            AssertNear(1f, group.alpha);
            AssertNear(new Color(1f, 0.5f, 0.25f, 1f), image.color);
        }

        [Test]
        public void From_RotationSizeAndPivot()
        {
            RectTransform rect = CreateRect();
            rect.sizeDelta = new Vector2(100f, 100f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.DORotate(90f, 0.5f).From();
            rect.DOSizeDelta(Vector2.zero, 0.5f).From();
            rect.DOPivot(Vector2.zero, 0.5f).From();

            StepPast(0.5f);
            AssertNear(0f, Mathf.DeltaAngle(0f, rect.eulerAngles.z), 0.05f);
            AssertNear(new Vector2(100f, 100f), rect.sizeDelta);
            AssertNear(new Vector2(0.5f, 0.5f), rect.pivot);
        }

        [Test]
        public void From_InsideThen_EndsAtTheValueItHadWhenCreated()
        {
            RectTransform first = CreateRect();
            RectTransform popup = CreateRect();
            popup.localScale = Vector3.one;

            first.DOAnchorPosX(100f, 0.2f).Then(popup.DOScale(Vector3.zero, 0.2f).From().GetHandle());

            StepPast(0.2f);
            Assert.That(popup.localScale.x, Is.LessThan(0.5f), "still near the From value as it starts");

            StepPast(0.2f);
            AssertNear(Vector3.one, popup.localScale);
        }
    }

    public class RelativeTests : TweenTestBase
    {
        [Test]
        public void Relative_MovesByTheGivenAmount()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(30f, 5f);

            rect.DOAnchorPosX(50f, 1f).Relative();

            Step(0.5f);
            AssertNear(new Vector2(55f, 5f), rect.anchoredPosition, 0.5f);
            StepPast(0.5f);
            AssertNear(new Vector2(80f, 5f), rect.anchoredPosition);
        }

        [Test]
        public void Relative_AllAxes()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(5f, 5f);

            rect.DOAnchorPos(new Vector2(10f, 20f), 0.5f).Relative();

            StepPast(0.5f);
            AssertNear(new Vector2(15f, 25f), rect.anchoredPosition);
        }

        [Test]
        public void Relative_ScaleRotationAndFade()
        {
            RectTransform rect = CreateRect();
            CanvasGroup group = CreateGroup();
            rect.localScale = Vector3.one;
            rect.eulerAngles = new Vector3(0f, 0f, 10f);
            group.alpha = 1f;

            rect.DOScale(Vector3.one, 0.5f).Relative();
            rect.DORotate(30f, 0.5f).Relative();
            group.DOFade(-0.5f, 0.5f).Relative();

            StepPast(0.5f);
            AssertNear(Vector3.one * 2f, rect.localScale);
            AssertNear(40f, rect.eulerAngles.z, 0.05f);
            AssertNear(0.5f, group.alpha);
        }

        [Test]
        public void Relative_InsideThen_UsesTheValueWhenItStarts()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f).Then(rect.DOAnchorPosX(50f, 0.2f).Relative().GetHandle());

            StepPast(0.5f);
            AssertNear(150f, rect.anchoredPosition.x);
        }

        [Test]
        public void Relative_YoyoInfinite_DoesNotDrift()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(20f, 0.1f).Relative().LoopInfinite(LoopType.Yoyo);

            Step(2f);   // twenty plays, back at the start
            AssertNear(0f, rect.anchoredPosition.x, 4f);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FromAndRelative_InEitherOrder(bool relativeFirst)
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(10f, 0f);

            TweenBuilder builder = rect.DOAnchorPosX(50f, 0.5f);
            if (relativeFirst)
                builder.Relative().From();
            else
                builder.From().Relative();

            AssertNear(60f, rect.anchoredPosition.x, 0.001f, "starts offset from where it was");

            StepPast(0.5f);
            AssertNear(10f, rect.anchoredPosition.x);
        }
    }
}
