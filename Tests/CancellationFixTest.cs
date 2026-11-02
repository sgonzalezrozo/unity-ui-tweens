using System.Collections;
using UITweens.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Tests
{
    /// <summary>
    /// Comprehensive test for cancellation logic fix.
    /// Tests that new tweens properly cancel previous tweens on same object+property.
    /// </summary>
    public class CancellationFixTest : MonoBehaviour
    {
        [SerializeField] private Image testImage;

        private void Start()
        {
            if (testImage == null)
                testImage = GetComponent<Image>();

            StartCoroutine(RunAllTests());
        }

        private IEnumerator RunAllTests()
        {
            Debug.Log("=== CANCELLATION FIX TESTS START ===");

            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(Test1_MultipleImmediateTweens());

            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(Test2_ImmediateThenDelayed());

            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(Test3_DelayedThenImmediate());

            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(Test4_MultipleDelayed());

            Debug.Log("=== CANCELLATION FIX TESTS COMPLETE ===");
        }

        /// <summary>
        /// TEST 1: Multiple immediate tweens
        /// EXPECTED: Second tween cancels first immediately, only second plays
        /// </summary>
        private IEnumerator Test1_MultipleImmediateTweens()
        {
            Debug.Log("[Test1] Starting - Multiple immediate tweens");

            // Reset alpha
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            Debug.Log($"[Test1] Initial alpha: {testImage.color.a}");

            // First immediate tween (should be cancelled)
            testImage.DOFade(0.2f, 2f);
            Debug.Log("[Test1] Started Tween A: fade to 0.2 over 2s");

            yield return new WaitForSeconds(0.1f);
            Debug.Log($"[Test1] After 0.1s, alpha: {testImage.color.a} (should be ~0.95, starting to fade)");

            // Second immediate tween (should cancel first)
            testImage.DOFade(0.8f, 1f);
            Debug.Log("[Test1] Started Tween B: fade to 0.8 over 1s (should cancel A)");

            yield return new WaitForSeconds(0.1f);
            Debug.Log($"[Test1] After Tween B starts + 0.1s, alpha: {testImage.color.a}");

            yield return new WaitForSeconds(1f);
            float finalAlpha = testImage.color.a;
            Debug.Log($"[Test1] After 1s (Tween B complete), alpha: {finalAlpha}");

            // Verify: Should be ~0.8 (Tween B target), NOT ~0.6 (midway to Tween A target)
            if (Mathf.Abs(finalAlpha - 0.8f) < 0.1f)
            {
                Debug.Log("[Test1] ✅ PASS - Only Tween B played (alpha ~0.8)");
            }
            else
            {
                Debug.LogError($"[Test1] ❌ FAIL - Expected alpha ~0.8, got {finalAlpha}");
            }
        }

        /// <summary>
        /// TEST 2: Immediate tween followed by delayed tween
        /// EXPECTED: Immediate plays, then delayed cancels it when delay finishes
        /// </summary>
        private IEnumerator Test2_ImmediateThenDelayed()
        {
            Debug.Log("[Test2] Starting - Immediate then delayed");

            // Reset alpha
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            Debug.Log($"[Test2] Initial alpha: {testImage.color.a}");

            // Immediate tween (3s duration)
            testImage.DOFade(0.1f, 3f);
            Debug.Log("[Test2] Started Tween A: fade to 0.1 over 3s (immediate)");

            yield return new WaitForSeconds(0.5f);
            float alpha1 = testImage.color.a;
            Debug.Log($"[Test2] After 0.5s, alpha: {alpha1} (should be ~0.85, fading down)");

            // Delayed tween (1s delay, 1s duration)
            testImage.DOFade(0.9f, 1f).Delay(1f);
            Debug.Log("[Test2] Queued Tween B: fade to 0.9 over 1s, 1s delay");

            yield return new WaitForSeconds(0.5f);
            float alpha2 = testImage.color.a;
            Debug.Log($"[Test2] After 1s total (still in delay), alpha: {alpha2} (A still playing)");

            yield return new WaitForSeconds(0.6f);
            float alpha3 = testImage.color.a;
            Debug.Log($"[Test2] After 1.6s (B spawned, cancelled A), alpha: {alpha3}");

            yield return new WaitForSeconds(1f);
            float finalAlpha = testImage.color.a;
            Debug.Log($"[Test2] Final alpha: {finalAlpha}");

            // Verify: Should be ~0.9 (Tween B target)
            if (Mathf.Abs(finalAlpha - 0.9f) < 0.1f)
            {
                Debug.Log("[Test2] ✅ PASS - Tween B cancelled A and completed (alpha ~0.9)");
            }
            else
            {
                Debug.LogError($"[Test2] ❌ FAIL - Expected alpha ~0.9, got {finalAlpha}");
            }
        }

        /// <summary>
        /// TEST 3: Delayed tween followed by immediate tween
        /// EXPECTED: Immediate starts now, delayed cancels it when spawned
        /// </summary>
        private IEnumerator Test3_DelayedThenImmediate()
        {
            Debug.Log("[Test3] Starting - Delayed then immediate");

            // Reset alpha
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            Debug.Log($"[Test3] Initial alpha: {testImage.color.a}");

            // Delayed tween (2s delay, 1s duration)
            testImage.DOFade(0.3f, 1f).Delay(2f);
            Debug.Log("[Test3] Queued Tween A: fade to 0.3 over 1s, 2s delay");

            yield return new WaitForSeconds(0.5f);
            Debug.Log($"[Test3] After 0.5s (in delay), alpha: {testImage.color.a} (should be 1.0, no change)");

            // Immediate tween (2s duration)
            testImage.DOFade(0.7f, 2f);
            Debug.Log("[Test3] Started Tween B: fade to 0.7 over 2s (immediate)");

            yield return new WaitForSeconds(0.5f);
            float alpha1 = testImage.color.a;
            Debug.Log($"[Test3] After 1s total, alpha: {alpha1} (B playing, should be ~0.85)");

            yield return new WaitForSeconds(1.1f);
            float alpha2 = testImage.color.a;
            Debug.Log($"[Test3] After 2.1s (A spawned, cancelled B), alpha: {alpha2}");

            yield return new WaitForSeconds(1f);
            float finalAlpha = testImage.color.a;
            Debug.Log($"[Test3] Final alpha: {finalAlpha}");

            // Verify: Should be ~0.3 (Tween A target)
            if (Mathf.Abs(finalAlpha - 0.3f) < 0.1f)
            {
                Debug.Log("[Test3] ✅ PASS - Tween A cancelled B and completed (alpha ~0.3)");
            }
            else
            {
                Debug.LogError($"[Test3] ❌ FAIL - Expected alpha ~0.3, got {finalAlpha}");
            }
        }

        /// <summary>
        /// TEST 4: Multiple delayed tweens
        /// EXPECTED: Only last queued tween spawns
        /// </summary>
        private IEnumerator Test4_MultipleDelayed()
        {
            Debug.Log("[Test4] Starting - Multiple delayed tweens");

            // Reset alpha
            Color color = testImage.color;
            color.a = 1f;
            testImage.color = color;

            Debug.Log($"[Test4] Initial alpha: {testImage.color.a}");

            // First delayed tween (should be replaced)
            testImage.DOFade(0.2f, 1f).Delay(2f);
            Debug.Log("[Test4] Queued Tween A: fade to 0.2, 2s delay (will be replaced)");

            // Second delayed tween (replaces first in queue)
            testImage.DOFade(0.6f, 1f).Delay(3f);
            Debug.Log("[Test4] Queued Tween B: fade to 0.6, 3s delay (replaces A)");

            yield return new WaitForSeconds(2.5f);
            float alpha1 = testImage.color.a;
            Debug.Log($"[Test4] After 2.5s (past A's delay), alpha: {alpha1} (should be 1.0, A didn't spawn)");

            yield return new WaitForSeconds(1f);
            float alpha2 = testImage.color.a;
            Debug.Log($"[Test4] After 3.5s (B spawned), alpha: {alpha2} (should be fading to 0.6)");

            yield return new WaitForSeconds(1f);
            float finalAlpha = testImage.color.a;
            Debug.Log($"[Test4] Final alpha: {finalAlpha}");

            // Verify: Should be ~0.6 (Tween B target), NOT 0.2 (Tween A target)
            if (Mathf.Abs(finalAlpha - 0.6f) < 0.1f)
            {
                Debug.Log("[Test4] ✅ PASS - Only Tween B spawned (alpha ~0.6)");
            }
            else
            {
                Debug.LogError($"[Test4] ❌ FAIL - Expected alpha ~0.6, got {finalAlpha}");
            }
        }
    }
}

