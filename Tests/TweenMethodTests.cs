using NUnit.Framework;
using UITweens.Extensions;
using UITweens.Math;
using UITweens.Services;
using UnityEngine;
using UnityEngine.UI;

namespace UITweens.Tests
{
    /// <summary>Every tween method reaches the right value halfway and at the end.</summary>
    public class TweenMethodTests : TweenTestBase
    {
        [Test]
        public void DOAnchorPos()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPos(new Vector2(100f, 200f), 1f);

            Step(0.5f);
            AssertNear(new Vector2(50f, 100f), rect.anchoredPosition, 0.5f);
            StepPast(0.5f);
            AssertNear(new Vector2(100f, 200f), rect.anchoredPosition);
        }

        [Test]
        public void DOAnchorPosX_LeavesYAlone()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(10f, 20f);

            rect.DOAnchorPosX(110f, 1f);

            Step(0.5f);
            AssertNear(new Vector2(60f, 20f), rect.anchoredPosition, 0.5f);
            StepPast(0.5f);
            AssertNear(new Vector2(110f, 20f), rect.anchoredPosition);
        }

        [Test]
        public void DOAnchorPosY_LeavesXAlone()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = new Vector2(10f, 20f);

            rect.DOAnchorPosY(-80f, 1f);

            Step(0.5f);
            AssertNear(new Vector2(10f, -30f), rect.anchoredPosition, 0.5f);
            StepPast(0.5f);
            AssertNear(new Vector2(10f, -80f), rect.anchoredPosition);
        }

        [Test]
        public void DOScale()
        {
            RectTransform rect = CreateRect();
            rect.localScale = Vector3.one;

            rect.DOScale(new Vector3(2f, 3f, 1f), 1f);

            Step(0.5f);
            AssertNear(new Vector3(1.5f, 2f, 1f), rect.localScale);
            StepPast(0.5f);
            AssertNear(new Vector3(2f, 3f, 1f), rect.localScale);
        }

        [Test]
        public void DOScaleX_Y_Z_ChangeOneAxisEach()
        {
            RectTransform x = CreateRect();
            RectTransform y = CreateRect();
            RectTransform z = CreateRect();

            x.DOScaleX(2f, 1f);
            y.DOScaleY(2f, 1f);
            z.DOScaleZ(2f, 1f);

            StepPast(1f);
            AssertNear(new Vector3(2f, 1f, 1f), x.localScale);
            AssertNear(new Vector3(1f, 2f, 1f), y.localScale);
            AssertNear(new Vector3(1f, 1f, 2f), z.localScale);
        }

        [Test]
        public void DORotate()
        {
            RectTransform rect = CreateRect();

            rect.DORotate(90f, 1f);

            Step(0.5f);
            AssertNear(45f, rect.eulerAngles.z, 0.5f);
            StepPast(0.5f);
            AssertNear(90f, rect.eulerAngles.z, 0.05f);
        }

        [Test]
        public void DOSizeDelta()
        {
            RectTransform rect = CreateRect();
            rect.sizeDelta = new Vector2(100f, 100f);

            rect.DOSizeDelta(new Vector2(200f, 50f), 1f);

            Step(0.5f);
            AssertNear(new Vector2(150f, 75f), rect.sizeDelta, 0.5f);
            StepPast(0.5f);
            AssertNear(new Vector2(200f, 50f), rect.sizeDelta);
        }

        [Test]
        public void DOPivot()
        {
            RectTransform rect = CreateRect();
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.DOPivot(new Vector2(0f, 1f), 1f);

            Step(0.5f);
            AssertNear(new Vector2(0.25f, 0.75f), rect.pivot);
            StepPast(0.5f);
            AssertNear(new Vector2(0f, 1f), rect.pivot);
        }

        [Test]
        public void Image_DOFade_KeepsTheColor()
        {
            Image image = CreateImage();
            image.color = new Color(1f, 0.5f, 0.25f, 1f);

            image.DOFade(0f, 1f);

            Step(0.5f);
            AssertNear(new Color(1f, 0.5f, 0.25f, 0.5f), image.color);
            StepPast(0.5f);
            AssertNear(new Color(1f, 0.5f, 0.25f, 0f), image.color);
        }

        [Test]
        public void Image_DOColor()
        {
            Image image = CreateImage();
            image.color = Color.white;

            image.DOColor(Color.red, 1f);

            Step(0.5f);
            AssertNear(new Color(1f, 0.5f, 0.5f, 1f), image.color);
            StepPast(0.5f);
            AssertNear(Color.red, image.color);
        }

        [Test]
        public void Image_DOColorFromTo_IgnoresTheCurrentColor()
        {
            Image image = CreateImage();
            image.color = Color.red;

            image.DOColorFromTo(Color.black, Color.white, 1f);

            Step(0.5f);
            AssertNear(new Color(0.5f, 0.5f, 0.5f, 1f), image.color);
            StepPast(0.5f);
            AssertNear(Color.white, image.color);
        }

        [Test]
        public void CanvasGroup_DOFade()
        {
            CanvasGroup group = CreateGroup();
            group.alpha = 1f;

            group.DOFade(0f, 1f);

            Step(0.5f);
            AssertNear(0.5f, group.alpha);
            StepPast(0.5f);
            AssertNear(0f, group.alpha);
        }

        [Test]
        public void CanvasGroup_DOFadeFromTo_IgnoresTheCurrentAlpha()
        {
            CanvasGroup group = CreateGroup();
            group.alpha = 0.3f;

            group.DOFadeFromTo(0f, 1f, 1f);

            Step(0.5f);
            AssertNear(0.5f, group.alpha);
            StepPast(0.5f);
            AssertNear(1f, group.alpha);
        }

        [Test]
        public void CustomFloat_ReportsItsValue()
        {
            float last = float.NaN;
            TweenHandle handle = TweenManager.GetSpawner()
                .CustomFloat(0f, 10f, 1f)
                .OnFloatUpdate(value => last = value)
                .GetHandle();

            Step(0.5f);
            AssertNear(5f, handle.FloatValue, 0.05f);
            AssertNear(5f, last, 0.05f);

            StepPast(0.5f);
            AssertNear(10f, last);
        }

        [Test]
        public void CustomVector_ReportsItsValue()
        {
            Vector3 last = Vector3.zero;
            TweenHandle handle = TweenManager.GetSpawner()
                .CustomVector(Vector3.zero, new Vector3(10f, 20f, 30f), 1f)
                .OnVectorUpdate(value => last = value)
                .GetHandle();

            Step(0.5f);
            AssertNear(new Vector3(5f, 10f, 15f), handle.VectorValue, 0.1f);

            StepPast(0.5f);
            AssertNear(new Vector3(10f, 20f, 30f), last);
        }

        [Test]
        public void EaseIsApplied()
        {
            RectTransform rect = CreateRect();
            rect.anchoredPosition = Vector2.zero;

            rect.DOAnchorPosX(100f, 1f, EaseType.QuadIn);

            Step(0.5f);
            AssertNear(25f, rect.anchoredPosition.x, 0.5f);
        }

        [Test]
        public void EaseParameterIsApplied()
        {
            RectTransform soft = CreateRect();
            RectTransform strong = CreateRect();

            soft.DOAnchorPosX(100f, 1f, EaseType.BackOut);
            strong.DOAnchorPosX(100f, 1f, EaseType.BackOut, 3f);

            Step(0.6f);
            Assert.That(strong.anchoredPosition.x, Is.GreaterThan(soft.anchoredPosition.x));
        }

        [Test]
        public void TweenOnNullTargetWarnsAndDoesNotThrow()
        {
            RectTransform missing = null;

            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("null"));
            Assert.DoesNotThrow(() => missing.DOAnchorPosX(10f, 1f));
            StepPast(1f);
        }
    }
}
