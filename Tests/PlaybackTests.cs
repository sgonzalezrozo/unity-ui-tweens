using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Services;
using UnityEngine;
using UnityEngine.TestTools;

namespace UITweens.Tests
{
    public class CompleteTests : TweenTestBase
    {
        [Test]
        public void Complete_JumpsToTheEndAtOnceAndRunsOnCompleteNextUpdate()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).OnComplete(() => completions++).GetHandle();

            Step(0.3f);
            TweenManager.Complete(handle);
            AssertNear(100f, rect.anchoredPosition.x, 0.001f);

            StepFrames(1);
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(handle.IsPlaying, Is.False);

            Step(0.5f);
            Assert.That(completions, Is.EqualTo(1));
            AssertNear(100f, rect.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Complete_WorksOnPausedAndDelayedTweens()
        {
            RectTransform paused = CreateRect();
            RectTransform delayed = CreateRect();
            TweenHandle a = paused.DOAnchorPosX(100f, 1f).GetHandle();
            TweenHandle b = delayed.DOAnchorPosX(100f, 1f).Delay(5f).GetHandle();

            Step(0.2f);
            TweenManager.Pause(a);
            TweenManager.Complete(a);
            TweenManager.Complete(b);
            StepFrames(1);

            AssertNear(100f, paused.anchoredPosition.x);
            AssertNear(100f, delayed.anchoredPosition.x);
        }

        [Test]
        public void Complete_FiniteRestartLoop_EndsAtTheTargetAndCompletesOnce()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 0.2f).Loop(3).OnComplete(() => completions++).GetHandle();

            Step(0.1f);
            TweenManager.Complete(handle);
            Step(0.5f);

            AssertNear(100f, rect.anchoredPosition.x);
            Assert.That(completions, Is.EqualTo(1));
        }

        [TestCase(1, 0f)]
        [TestCase(2, 100f)]
        [TestCase(3, 0f)]
        public void Complete_YoyoLoop_EndsWhereItsLastPlayWould(int repeats, float expected)
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 0.2f).Loop(repeats, LoopType.Yoyo).GetHandle();

            Step(0.25f);   // second play, on the way back
            TweenManager.Complete(handle);
            AssertNear(expected, rect.anchoredPosition.x, 0.001f);

            Step(0.5f);
            AssertNear(expected, rect.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Complete_InfiniteLoop_StopsAtTheTarget()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 0.2f).LoopInfinite(LoopType.Yoyo).OnComplete(() => completions++).GetHandle();

            Step(0.3f);
            TweenManager.Complete(handle);
            Step(0.5f);

            AssertNear(100f, rect.anchoredPosition.x);
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(handle.IsPlaying, Is.False);
        }

        [Test]
        public void Complete_RelativeTweenThatHasNotStarted_UsesTheOffset()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(10f, 0f);

            TweenHandle handle = rect.DOAnchorPosX(50f, 1f).Relative().GetHandle();
            TweenManager.Complete(handle);

            AssertNear(60f, rect.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Complete_FromTween_EndsAtTheOriginalValue()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).From().GetHandle();
            TweenManager.Complete(handle);

            AssertNear(0f, rect.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Complete_StartsTheNextTweenInASequence()
        {
            RectTransform first = CreateRect();
            RectTransform second = CreateRect();
            second.anchoredPosition = Vector2.zero;

            TweenBuilder builder = first.DOAnchorPosX(100f, 5f);
            TweenHandle firstHandle = builder.GetHandle();
            builder.Then(second.DOAnchorPosX(100f, 0.2f).GetHandle());

            TweenManager.Complete(firstHandle);
            StepFrames(1);
            StepPast(0.2f);

            AssertNear(100f, first.anchoredPosition.x);
            AssertNear(100f, second.anchoredPosition.x);
        }

        [Test]
        public void DoComplete_CompletesEveryTweenOnTheComponent()
        {
            RectTransform rect = CreateRect();
            rect.DOAnchorPosX(100f, 1f);
            rect.DOScale(Vector3.one * 2f, 1f);
            Step(0.2f);

            rect.DoComplete();

            AssertNear(100f, rect.anchoredPosition.x, 0.001f);
            AssertNear(Vector3.one * 2f, rect.localScale, 0.001f);
        }

        [Test]
        public void Complete_OnAnEndedTween_DoesNothing()
        {
            RectTransform rect = CreateRect();
            TweenHandle old = rect.DOAnchorPosX(100f, 0.1f).GetHandle();
            StepPast(0.1f);

            rect.DOAnchorPosX(-100f, 1f);
            TweenManager.Complete(old);
            Step(0.5f);

            AssertNear(0f, rect.anchoredPosition.x, 2f, "the new tween is untouched, halfway from 100 to -100");
            LogAssert.NoUnexpectedReceived();
        }
    }

    public class RestartAndRewindTests : TweenTestBase
    {
        [Test]
        public void Restart_GoesBackToTheStartAndPlaysAgain()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).OnComplete(() => completions++).GetHandle();

            Step(0.6f);
            TweenManager.Restart(handle);
            AssertNear(0f, rect.anchoredPosition.x, 0.001f);

            Step(0.6f);
            AssertNear(60f, rect.anchoredPosition.x, 1f);
            Assert.That(completions, Is.EqualTo(0));

            StepPast(0.4f);
            AssertNear(100f, rect.anchoredPosition.x);
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void Restart_ReplaysTheDelayUnlessToldNotTo()
        {
            RectTransform withDelay = CreateRect();
            RectTransform withoutDelay = CreateRect();
            TweenHandle a = withDelay.DOAnchorPosX(100f, 1f).Delay(0.5f).GetHandle();
            TweenHandle b = withoutDelay.DOAnchorPosX(100f, 1f).Delay(0.5f).GetHandle();

            Step(1f);
            TweenManager.Restart(a);
            TweenManager.Restart(b, includeDelay: false);
            Step(0.3f);

            AssertNear(0f, withDelay.anchoredPosition.x, 0.001f);
            AssertNear(30f, withoutDelay.anchoredPosition.x, 1f);
        }

        [Test]
        public void Restart_YoyoOnTheWayBack_StartsForwardAgain()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 0.2f).Loop(1, LoopType.Yoyo).GetHandle();

            Step(0.3f);
            TweenManager.Restart(handle);
            AssertNear(0f, rect.anchoredPosition.x, 0.001f);

            Step(0.1f);
            AssertNear(50f, rect.anchoredPosition.x, 6f);
            Step(0.2f);
            AssertNear(50f, rect.anchoredPosition.x, 6f);
            StepPast(0.1f);
            AssertNear(0f, rect.anchoredPosition.x);
        }

        [Test]
        public void Restart_PausedTween_PlaysAgain()
        {
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 0.5f).GetHandle();
            Step(0.2f);
            TweenManager.Pause(handle);

            TweenManager.Restart(handle);
            StepPast(0.5f);

            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void Rewind_GoesBackToTheStartAndPauses()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).GetHandle();

            Step(0.6f);
            TweenManager.Rewind(handle);
            Step(0.5f);

            AssertNear(0f, rect.anchoredPosition.x, 0.001f);
            Assert.That(handle.IsPaused, Is.True);

            TweenManager.Resume(handle);
            Step(0.5f);
            AssertNear(50f, rect.anchoredPosition.x, 1f);
            StepPast(0.5f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void DoRewindThenDoPlay_OnAComponent()
        {
            CanvasGroup group = CreateGroup();
            group.alpha = 1f;
            group.DOFade(0f, 1f);
            Step(0.5f);

            group.DoRewind();
            AssertNear(1f, group.alpha, 0.001f);

            group.DoPlay();
            StepPast(1f);
            AssertNear(0f, group.alpha);
        }

        [Test]
        public void DoRestart_OnAComponent()
        {
            RectTransform rect = CreateRect();
            rect.localScale = Vector3.one;
            rect.DOScale(Vector3.one * 3f, 1f);
            Step(0.5f);

            rect.DoRestart();
            AssertNear(Vector3.one, rect.localScale, 0.001f);
        }
    }

    public class DisableBehaviourTests : TweenTestBase
    {
        [Test]
        public void Default_KeepsPlayingWhileDisabled()
        {
            RectTransform rect = CreateRect();
            rect.DOAnchorPosX(100f, 0.5f);

            Step(0.1f);
            rect.gameObject.SetActive(false);
            StepPast(0.4f);

            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void Pause_FreezesWhileDisabledAndCarriesOnWhenEnabled()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            rect.DOAnchorPosX(100f, 1f).WhenDisabled(DisableBehaviour.Pause).OnComplete(() => completions++);

            Step(0.3f);
            rect.gameObject.SetActive(false);
            StepFrames(1);
            float frozen = rect.anchoredPosition.x;
            Step(2f);
            AssertNear(frozen, rect.anchoredPosition.x, 0.001f);

            rect.gameObject.SetActive(true);
            Step(0.5f);
            Assert.That(completions, Is.EqualTo(0));
            StepPast(0.3f);
            AssertNear(100f, rect.anchoredPosition.x);
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void Pause_AlsoReactsToADisabledParent()
        {
            RectTransform parent = CreateRect("Parent");
            RectTransform child = CreateRect("Child");
            child.SetParent(parent, false);
            child.anchoredPosition = Vector2.zero;
            child.DOAnchorPosX(100f, 1f).WhenDisabled(DisableBehaviour.Pause);

            Step(0.3f);
            parent.gameObject.SetActive(false);
            StepFrames(1);
            float frozen = child.anchoredPosition.x;
            Step(1f);

            AssertNear(frozen, child.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void Pause_HoldsTheDelayToo()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            rect.DOAnchorPosX(100f, 0.5f).Delay(0.5f).WhenDisabled(DisableBehaviour.Pause);

            Step(0.2f);
            rect.gameObject.SetActive(false);
            Step(2f);
            rect.gameObject.SetActive(true);
            Step(0.2f);

            AssertNear(0f, rect.anchoredPosition.x, 0.001f);
            StepPast(0.6f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void Cancel_StopsTheTweenWhenDisabled()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).WhenDisabled(DisableBehaviour.Cancel).OnComplete(() => completions++).GetHandle();

            Step(0.3f);
            rect.gameObject.SetActive(false);
            StepFrames(2);
            float stopped = rect.anchoredPosition.x;
            rect.gameObject.SetActive(true);
            Step(1f);

            Assert.That(handle.IsPlaying, Is.False);
            AssertNear(stopped, rect.anchoredPosition.x, 0.001f);
            Assert.That(completions, Is.EqualTo(0));
        }

        [Test]
        public void Complete_JumpsToTheEndWhenDisabled()
        {
            int completions = 0;
            RectTransform rect = CreateRect();
            rect.DOAnchorPosX(100f, 1f).WhenDisabled(DisableBehaviour.Complete).OnComplete(() => completions++);

            Step(0.3f);
            rect.gameObject.SetActive(false);
            StepFrames(2);

            AssertNear(100f, rect.anchoredPosition.x);
            Assert.That(completions, Is.EqualTo(1));
        }

        [Test]
        public void DisableBehaviourByDefault_AppliesToNewTweens()
        {
            TweenManager.DisableBehaviourByDefault = DisableBehaviour.Pause;
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            rect.DOAnchorPosX(100f, 1f);

            Step(0.3f);
            rect.gameObject.SetActive(false);
            StepFrames(1);
            float frozen = rect.anchoredPosition.x;
            Step(1f);

            AssertNear(frozen, rect.anchoredPosition.x, 0.001f);
        }

        [Test]
        public void WhenDisabled_OverridesTheDefault()
        {
            TweenManager.DisableBehaviourByDefault = DisableBehaviour.Pause;
            RectTransform rect = CreateRect();
            rect.DOAnchorPosX(100f, 0.5f).WhenDisabled(DisableBehaviour.KeepPlaying);

            Step(0.1f);
            rect.gameObject.SetActive(false);
            StepPast(0.4f);

            AssertNear(100f, rect.anchoredPosition.x);
        }
    }
}
