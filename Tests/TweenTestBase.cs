using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Tests
{
    /// <summary>
    /// Gives every test a fresh TweenManager and a canvas. Time is advanced by hand with Step(),
    /// so tests are fast and give the same result on every run.
    /// </summary>
    public abstract class TweenTestBase
    {
        /// <summary>Length of one simulated frame.</summary>
        protected const float Frame = 0.01f;

        private readonly List<GameObject> created = new();
        protected Canvas Canvas;

        [SetUp]
        public void BaseSetUp()
        {
            TweenManager.ResetForTests();
            TweenManager.ManualUpdate = true;
            TweenManager.Initialize();

            Canvas = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
            created.Add(Canvas.gameObject);
        }

        [TearDown]
        public void BaseTearDown()
        {
            TweenManager.ResetForTests();
            Time.timeScale = 1f;

            foreach (GameObject go in created)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }

            created.Clear();
        }

        protected RectTransform CreateRect(string name = "Rect")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(Canvas.transform, false);
            created.Add(go);
            return (RectTransform)go.transform;
        }

        protected Image CreateImage()
            => CreateRect("Image").gameObject.AddComponent<Image>();

        protected CanvasGroup CreateGroup()
            => CreateRect("Group").gameObject.AddComponent<CanvasGroup>();

        /// <summary>Advances all tweens by the given time, one simulated frame at a time.</summary>
        protected static void Step(float seconds)
        {
            int frames = Mathf.RoundToInt(seconds / Frame);
            for (int i = 0; i < frames; i++)
                TweenManager.Step(Frame);
        }

        /// <summary>Advances a little past the given time, so a tween of that length has surely finished.</summary>
        protected static void StepPast(float seconds)
            => Step(seconds + 3 * Frame);

        protected static void StepFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
                TweenManager.Step(Frame);
        }

        protected static void AssertNear(float expected, float actual, float tolerance = 0.01f, string message = null)
            => Assert.That(actual, Is.EqualTo(expected).Within(tolerance), message);

        protected static void AssertNear(Vector2 expected, Vector2 actual, float tolerance = 0.01f)
        {
            AssertNear(expected.x, actual.x, tolerance, $"x of {actual}");
            AssertNear(expected.y, actual.y, tolerance, $"y of {actual}");
        }

        protected static void AssertNear(Vector3 expected, Vector3 actual, float tolerance = 0.01f)
        {
            AssertNear(expected.x, actual.x, tolerance, $"x of {actual}");
            AssertNear(expected.y, actual.y, tolerance, $"y of {actual}");
            AssertNear(expected.z, actual.z, tolerance, $"z of {actual}");
        }

        protected static void AssertNear(Color expected, Color actual, float tolerance = 0.01f)
        {
            AssertNear(expected.r, actual.r, tolerance, $"r of {actual}");
            AssertNear(expected.g, actual.g, tolerance, $"g of {actual}");
            AssertNear(expected.b, actual.b, tolerance, $"b of {actual}");
            AssertNear(expected.a, actual.a, tolerance, $"a of {actual}");
        }
    }
}
