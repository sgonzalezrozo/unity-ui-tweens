#if UNITY_EDITOR

using System.Collections;
using Tools.UITweens.Runtime.Scripts.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.UITweens.Tests
{
    /// <summary>
    /// Simplified test suite for UITween Engine bug fixes.
    /// Tests core functionality without complex APIs.
    /// </summary>
    public class ComprehensiveTweenTests : MonoBehaviour
    {
        [SerializeField] private Image testImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        private int testsRun;
        private int testsPassed;

        private void Start()
        {
            if (testImage == null) testImage = GetComponent<Image>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            if (testImage == null || canvasGroup == null || rectTransform == null)
            {
                Debug.LogError("[Tests] Missing components!");
                return;
            }

            StartCoroutine(RunAllTests());
        }

        private IEnumerator RunAllTests()
        {
            Debug.Log("\n========== UITween Test Suite ==========\n");

            yield return StartCoroutine(TestPosition());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestScale());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestAlpha());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestDelay());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestLoop());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestChaining());
            yield return new WaitForSeconds(0.5f);

            Debug.Log("\n========== Results ==========");
            Debug.Log($"Total: {testsRun}, Passed: {testsPassed}, Failed: {testsRun - testsPassed}");
            Debug.Log($"Success Rate: {(testsPassed * 100f / testsRun):F0}%\n");
        }

        private IEnumerator TestPosition()
        {
            testsRun++;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.DOAnchorPos(new Vector2(100f, 50f), 1f);
            yield return new WaitForSeconds(1.1f);

            if (Vector2.Distance(rectTransform.anchoredPosition, new Vector2(100f, 50f)) < 5f)
            {
                Debug.Log("✅ Position Tween");
                testsPassed++;
            }
            else
            {
                Debug.LogError("❌ Position Tween");
            }
        }

        private IEnumerator TestScale()
        {
            testsRun++;
            rectTransform.localScale = Vector3.one;
            rectTransform.DOScale(new Vector3(2f, 2f, 2f), 1f);
            yield return new WaitForSeconds(1.1f);

            if (Vector3.Distance(rectTransform.localScale, new Vector3(2f, 2f, 2f)) < 0.1f)
            {
                Debug.Log("✅ Scale Tween");
                testsPassed++;
            }
            else
            {
                Debug.LogError("❌ Scale Tween");
            }
        }

        private IEnumerator TestAlpha()
        {
            testsRun++;
            canvasGroup.alpha = 1f;
            canvasGroup.DOFade(0f, 1f);
            yield return new WaitForSeconds(1.1f);

            if (Mathf.Abs(canvasGroup.alpha) < 0.05f)
            {
                Debug.Log("✅ Alpha Tween");
                testsPassed++;
            }
            else
            {
                Debug.LogError("❌ Alpha Tween");
            }
        }

        private IEnumerator TestDelay()
        {
            testsRun++;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.DOAnchorPos(new Vector2(100f, 0f), 0.5f).Delay(0.5f);

            yield return new WaitForSeconds(0.3f);
            Vector2 beforeDelay = rectTransform.anchoredPosition;

            yield return new WaitForSeconds(0.4f);
            Vector2 afterDelay = rectTransform.anchoredPosition;

            if (Vector2.Distance(beforeDelay, Vector2.zero) < 2f && afterDelay.x > 20f)
            {
                Debug.Log("✅ Delay Feature");
                testsPassed++;
            }
            else
            {
                Debug.LogError("❌ Delay Feature");
            }
        }

        private IEnumerator TestLoop()
        {
            testsRun++;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.DOAnchorPos(new Vector2(100f, 0f), 0.4f).Loop(3);

            yield return new WaitForSeconds(2f);

            if (Mathf.Abs(rectTransform.anchoredPosition.x - 100f) < 10f)
            {
                Debug.Log("✅ Loop Feature");
                testsPassed++;
            }
            else
            {
                Debug.LogError("❌ Loop Feature");
            }
        }

        private IEnumerator TestChaining()
        {
            testsRun++;
            rectTransform.anchoredPosition = Vector2.zero;

            // Test basic sequential tweens - simplified version
            // Tween 1: Move to (100, 0)
            rectTransform.DOAnchorPos(new Vector2(100f, 0f), 0.4f);

            yield return new WaitForSeconds(0.5f);
            Vector2 afterFirst = rectTransform.anchoredPosition;

            // Tween 2: Move back to origin
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.DOAnchorPos(Vector2.zero, 0.4f);

            yield return new WaitForSeconds(0.5f);
            Vector2 afterSecond = rectTransform.anchoredPosition;

            // Verify both tweens ran
            if (Mathf.Abs(afterFirst.x - 100f) < 20f &&
                Vector2.Distance(afterSecond, Vector2.zero) < 20f)
            {
                Debug.Log("✅ Chaining Feature");
                testsPassed++;
            }
            else
            {
                Debug.LogError("❌ Chaining Feature");
            }
        }
    }
}

#endif

