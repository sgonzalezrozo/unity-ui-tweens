using UITweens.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Tests
{
    /// <summary>
    /// Quick test for the delayed tween queue.
    /// Place this on a GameObject with an Image component.
    /// </summary>
    public class DelayedTweenTest : MonoBehaviour
    {
        [SerializeField] private Image testImage;

        private void Start()
        {
            if (testImage == null)
                testImage = GetComponent<Image>();

            RunTests();
        }

        private void RunTests()
        {
            Debug.Log("[DelayedTweenTest] Starting tests...");

            // Invoke each test with delay
            Invoke(nameof(Test1_MultipleDelayedTweens), 1f);
            Invoke(nameof(Test2_ImmediateThenDelayed), 5f);
            Invoke(nameof(Test3_DelayedWithCallbacks), 9f);
        }

        /// <summary>
        /// Test 1: Multiple delayed tweens to same property
        /// EXPECTED: Only the second tween (Delay 3s) should spawn and animate.
        /// The first tween (Delay 2s) should be replaced in queue before spawning.
        /// </summary>
        private void Test1_MultipleDelayedTweens()
        {
            Debug.Log("[Test1] Starting - Two delayed tweens, should only see second one");

            // Reset alpha to 1
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            // First delayed tween (should be replaced before it spawns)
            testImage.DOFade(0.5f, 1f).Delay(2f);
            Debug.Log("[Test1] Queued tween to alpha=0.5 with 2s delay (will be replaced)");

            // Second delayed tween (should replace first in queue)
            testImage.DOFade(0.2f, 1f).Delay(3f);
            Debug.Log("[Test1] Queued tween to alpha=0.2 with 3s delay (will spawn)");

            Debug.Log("[Test1] Expected: After 3s, alpha fades from 1.0 → 0.2. Should NOT fade to 0.5 first.");
        }

        /// <summary>
        /// Test 2: Immediate tween followed by delayed tween
        /// EXPECTED: Immediate tween starts now, delayed tween cancels it after 1s.
        /// </summary>
        private void Test2_ImmediateThenDelayed()
        {
            Debug.Log("[Test2] Starting - Immediate tween, then delayed");

            // Reset alpha
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            // Immediate tween (starts now, lasts 3s)
            testImage.DOFade(0.1f, 3f);
            Debug.Log("[Test2] Started immediate tween to alpha=0.1 over 3s");

            // Delayed tween (spawns after 1s, cancels immediate)
            testImage.DOFade(1f, 1f).Delay(1f);
            Debug.Log("[Test2] Queued delayed tween to alpha=1.0 with 1s delay");

            Debug.Log("[Test2] Expected: Alpha drops for 1s, then reverses back to 1.0");
        }

        /// <summary>
        /// Test 3: Delayed tween with callbacks
        /// EXPECTED: OnStart fires when delay finishes, OnComplete fires when animation ends.
        /// </summary>
        private void Test3_DelayedWithCallbacks()
        {
            Debug.Log("[Test3] Starting - Delayed tween with callbacks");

            // Reset alpha
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            testImage.DOFade(0.5f, 1f)
                .Delay(2f)
                .OnStart(OnTestStart)
                .OnComplete(OnTestComplete);

            Debug.Log("[Test3] Queued delayed tween with OnStart and OnComplete callbacks");
            Debug.Log("[Test3] Expected: OnStart after 2s, OnComplete after 3s total");
        }

        private void OnTestStart()
        {
            Debug.Log("[Test3] ✅ OnStart fired! Tween animation beginning now.");
        }

        private void OnTestComplete()
        {
            Debug.Log("[Test3] ✅ OnComplete fired! Tween animation finished.");
        }
    }
}

