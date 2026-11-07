using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Services;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Tests
{
    /// <summary>Pause, resume, cancel, per tween and per component.</summary>
    public class ControlTests : TweenTestBase
    {
        [Test]
        public void Handle_ReportsPlayingThenFinished()
        {
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 0.5f).GetHandle();

            Assert.That(handle.IsValid, Is.True);
            StepFrames(1);
            Assert.That(handle.IsPlaying, Is.True);

            StepPast(0.5f);
            Assert.That(handle.IsPlaying, Is.False);
        }

        [Test]
        public void Pause_FreezesAndResume_Continues()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).GetHandle();

            Step(0.3f);
            TweenManager.Pause(handle);
            float frozen = rect.anchoredPosition.x;

            Step(1f);
            Assert.That(handle.IsPaused, Is.True);
            AssertNear(frozen, rect.anchoredPosition.x, 0.001f);

            TweenManager.Resume(handle);
            Step(0.5f);
            Assert.That(handle.IsPlaying, Is.True);
            Assert.That(rect.anchoredPosition.x, Is.GreaterThan(frozen));

            StepPast(0.2f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void TogglePause_SwitchesBetweenPausedAndPlaying()
        {
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).GetHandle();
            StepFrames(1);

            TweenManager.TogglePause(handle);
            Assert.That(handle.IsPaused, Is.True);

            TweenManager.TogglePause(handle);
            Assert.That(handle.IsPlaying, Is.True);
        }

        [Test]
        public void Cancel_StopsWhereItIsWithoutCompleting()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).OnComplete(() => completions++).GetHandle();

            Step(0.3f);
            TweenManager.Cancel(handle);
            StepFrames(1);
            float stopped = rect.anchoredPosition.x;

            Step(1f);
            AssertNear(stopped, rect.anchoredPosition.x, 0.001f);
            Assert.That(stopped, Is.LessThan(100f));
            Assert.That(completions, Is.EqualTo(0));
            Assert.That(handle.IsPlaying, Is.False);
        }

        [Test]
        public void DoPause_PausesEveryTweenOnTheComponent_DoPlayResumesThem()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            rect.DOAnchorPosX(100f, 1f);
            rect.DOScale(Vector3.one * 2f, 1f);

            Step(0.3f);
            rect.DoPause();
            Vector2 position = rect.anchoredPosition;
            Vector3 scale = rect.localScale;

            Step(1f);
            AssertNear(position, rect.anchoredPosition, 0.001f);
            AssertNear(scale, rect.localScale, 0.001f);

            rect.DoPlay();
            StepPast(0.7f);
            AssertNear(100f, rect.anchoredPosition.x);
            AssertNear(Vector3.one * 2f, rect.localScale);
        }

        [Test]
        public void DoTogglePause_PausesThenResumes()
        {
            CanvasGroup group = CreateGroup();
            group.alpha = 1f;
            group.DOFade(0f, 1f);

            Step(0.3f);
            group.DoTogglePause();
            float frozen = group.alpha;
            Step(0.5f);
            AssertNear(frozen, group.alpha, 0.001f);

            group.DoTogglePause();
            StepPast(0.7f);
            AssertNear(0f, group.alpha);
        }

        [Test]
        public void DoCancel_RunsItsCallbackOnceProcessed()
        {
            int cancelled = 0;
            Image image = CreateImage();
            image.DOFade(0f, 1f);
            Step(0.2f);

            image.DoCancel().OnComplete(() => cancelled++);
            StepFrames(2);

            Assert.That(cancelled, Is.EqualTo(1));
        }

        [Test]
        public void DoCancel_WithNothingToCancel_RunsTheCallbackAtOnce()
        {
            int cancelled = 0;
            RectTransform rect = CreateRect();

            rect.DoCancel().OnComplete(() => cancelled++);

            Assert.That(cancelled, Is.EqualTo(1));
        }

        [Test]
        public void DoCancel_OnlyAffectsThatComponent()
        {
            RectTransform cancelled = CreateRect();
            RectTransform other = CreateRect();
            other.anchoredPosition = Vector2.zero;

            cancelled.DOAnchorPosX(100f, 1f);
            other.DOAnchorPosX(100f, 1f);

            Step(0.2f);
            cancelled.DoCancel();
            StepPast(0.8f);

            Assert.That(cancelled.anchoredPosition.x, Is.LessThan(100f));
            AssertNear(100f, other.anchoredPosition.x);
        }
    }
}
