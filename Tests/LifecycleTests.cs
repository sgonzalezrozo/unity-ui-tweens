using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Services;
using UnityEngine;
using UnityEngine.TestTools;

namespace UITweens.Tests
{
    /// <summary>Startup, pooling, destroyed targets and stale handles.</summary>
    public class LifecycleTests : TweenTestBase
    {
        [Test]
        public void Initialize_IsSafeToCallTwiceAndMakesItReady()
        {
            TweenManager.Initialize();
            TweenManager.Initialize();

            Assert.That(TweenManager.IsReady, Is.True);
            Assert.That(Object.FindObjectsByType<TweenManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }

        [Test]
        public void FirstTween_StartsUITweensOnItsOwn()
        {
            TweenManager.ResetForTests();
            TweenManager.ManualUpdate = true;
            Assert.That(TweenManager.Instance == null, Is.True, "nothing runs before it's needed");

            RectTransform rect = CreateRect();
            rect.DOAnchorPosX(100f, 0.5f);

            Assert.That(TweenManager.IsReady, Is.True);
            StepPast(0.5f);
            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void Prewarm_ThenTweensWork()
        {
            TweenManager.Prewarm(100);

            RectTransform rect = CreateRect();
            rect.DOAnchorPosX(100f, 0.5f);
            StepPast(0.5f);

            AssertNear(100f, rect.anchoredPosition.x);
        }

        [Test]
        public void MoreTweensThanThePool_AllFinish()
        {
            TweenManager.ResetForTests();
            TweenManager.Configure(initialPoolSize: 4, growthBatchSize: 2);
            TweenManager.ManualUpdate = true;
            TweenManager.Initialize();

            var rects = new RectTransform[300];
            for (int i = 0; i < rects.Length; i++)
            {
                rects[i] = CreateRect();
                rects[i].DOAnchorPosX(100f, 0.3f);
            }

            StepPast(0.3f);

            for (int i = 0; i < rects.Length; i++)
                AssertNear(100f, rects[i].anchoredPosition.x, 0.01f, $"rect {i}");
        }

        [Test]
        public void DestroyedTarget_StopsTheTweenWithoutErrors()
        {
            RectTransform rect = CreateRect();
            TweenHandle handle = rect.DOAnchorPosX(100f, 1f).OnComplete(() => Assert.Fail("should not complete")).GetHandle();

            Step(0.2f);
            Object.DestroyImmediate(rect.gameObject);
            Step(1.5f);

            Assert.That(handle.IsPlaying, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void StaleHandle_DoesNotAffectTheTweenThatReusedItsEntity()
        {
            RectTransform first = CreateRect();
            TweenHandle old = first.DOAnchorPosX(100f, 0.1f).GetHandle();
            StepPast(0.1f);

            RectTransform second = CreateRect();
            second.anchoredPosition = Vector2.zero;
            second.DOAnchorPosX(100f, 0.5f);

            Assert.That(old.IsPlaying, Is.False);
            TweenManager.Cancel(old);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("invalid or dead tween handle"));
            TweenManager.Pause(old);

            StepPast(0.5f);
            AssertNear(100f, second.anchoredPosition.x);
        }

        [Test]
        public void ZeroDuration_JumpsToTheEnd()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0f);
            StepFrames(2);

            AssertNear(100f, rect.anchoredPosition.x);
        }
    }
}
