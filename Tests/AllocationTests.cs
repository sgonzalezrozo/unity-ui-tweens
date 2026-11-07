using NUnit.Framework;
using UITweens.Extensions;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using NoAlloc = UnityEngine.TestTools.Constraints.Is;

namespace UITweens.Tests
{
    /// <summary>
    /// UI Tweens' main promise: no garbage once it's warmed up. These run in the Editor, which
    /// allocates more than a player, so a failure here is worth looking at before trusting it.
    /// </summary>
    public class AllocationTests : TweenTestBase
    {
        private const int Count = 100;
        private static readonly System.Action<RectTransform> NoCapture = target => { };

        [Test]
        public void RunningTweens_DoNotAllocate()
        {
            RectTransform[] rects = CreateRects();
            foreach (RectTransform rect in rects)
                rect.DOAnchorPosX(100f, 0.5f).LoopInfinite();

            StepFrames(10);

            Assert.That(() => StepFrames(60), NoAlloc.Not.AllocatingGCMemory());
        }

        [Test]
        public void CreatingAndFinishingTweens_DoesNotAllocateOnceWarm()
        {
            TweenManager.Prewarm(Count * 2);
            RectTransform[] rects = CreateRects();

            // Warm-up: grows the tracker and pools to the size this test needs.
            RunBatch(rects);
            RunBatch(rects);

            Assert.That(() => RunBatch(rects), NoAlloc.Not.AllocatingGCMemory());
        }

        [Test]
        public void YoyoUnscaledAndRelativeTweens_DoNotAllocateWhileRunning()
        {
            RectTransform[] rects = CreateRects();
            foreach (RectTransform rect in rects)
                rect.DOAnchorPosX(20f, 0.2f).Relative().IgnoreTimeScale().LoopInfinite(LoopType.Yoyo);

            StepFrames(10);

            Assert.That(() => StepFrames(60), NoAlloc.Not.AllocatingGCMemory());
        }

        [Test]
        public void CreatingFromAndRelativeTweens_DoesNotAllocateOnceWarm()
        {
            TweenManager.Prewarm(Count * 2);
            RectTransform[] rects = CreateRects();

            RunFromBatch(rects);
            RunFromBatch(rects);

            Assert.That(() => RunFromBatch(rects), NoAlloc.Not.AllocatingGCMemory());
        }

        private static void RunFromBatch(RectTransform[] rects)
        {
            foreach (RectTransform rect in rects)
                rect.DOScale(Vector3.one * 0.5f, 0.1f).Relative().From();

            StepFrames(15);
        }

        [Test]
        public void IdleFrames_DoNotAllocate()
        {
            StepFrames(10);
            Assert.That(() => StepFrames(120), NoAlloc.Not.AllocatingGCMemory());
        }

        private static void RunBatch(RectTransform[] rects)
        {
            foreach (RectTransform rect in rects)
                rect.DOAnchorPosX(rect.anchoredPosition.x > 50f ? 0f : 100f, 0.1f).OnComplete(rect, NoCapture);

            StepFrames(15);
        }

        private RectTransform[] CreateRects()
        {
            var rects = new RectTransform[Count];
            for (int i = 0; i < Count; i++)
                rects[i] = CreateRect();
            return rects;
        }
    }
}
