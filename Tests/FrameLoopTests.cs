using System.Collections;
using NUnit.Framework;
using UITweens.Extensions;
using UnityEngine;
using UnityEngine.TestTools;

namespace UITweens.Tests
{
    /// <summary>A few checks with real frames, so the link to Unity's Update and Time is covered too.</summary>
    public class FrameLoopTests : TweenTestBase
    {
        [SetUp]
        public void UseRealFrames()
            => TweenManager.ManualUpdate = false;

        [UnityTest]
        public IEnumerator TweensAdvanceOnRealFrames()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 0.2f);
            yield return new WaitForSecondsRealtime(0.5f);

            AssertNear(100f, rect.anchoredPosition.x);
        }

        [UnityTest]
        public IEnumerator TimeScaleZero_FreezesTweens()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;
            Time.timeScale = 0f;

            rect.DOAnchorPosX(100f, 0.2f);
            yield return new WaitForSecondsRealtime(0.3f);

            AssertNear(0f, rect.anchoredPosition.x, 0.001f);
        }

        [UnityTest]
        public IEnumerator TweenManager_LivesAcrossSceneLoads()
        {
            yield return null;
            Assert.That(TweenManager.Instance.gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
        }
    }
}
