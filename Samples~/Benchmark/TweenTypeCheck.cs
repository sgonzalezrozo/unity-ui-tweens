using System;
using System.Collections;
using UITweens;
using UITweens.Extensions;
using UITweens.Math;
using UITweens.Services;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays every tween type and logs PASS or FAIL for each one. Add it to an empty scene and press
/// Play. It takes about 10 seconds.
/// </summary>
public sealed class TweenTypeCheck : MonoBehaviour
{
    private const float Duration = 0.2f;
    private const float Settle = 0.15f;

    private RectTransform rect;
    private Image image;
    private CanvasGroup group;
    private int passed;
    private int failed;

    private IEnumerator Start()
    {
        CreateTarget();

        TweenManager.Prewarm(8);
        Report("Prewarm", TweenManager.IsReady);

        yield return Check("Position", () => rect.DOAnchorPos(new Vector2(40f, 20f), Duration),
            () => Near(rect.anchoredPosition, new Vector2(40f, 20f)));
        yield return Check("Position X", () => rect.DOAnchorPosX(90f, Duration),
            () => Near(rect.anchoredPosition.x, 90f) && Near(rect.anchoredPosition.y, 20f));
        yield return Check("Position Y", () => rect.DOAnchorPosY(70f, Duration),
            () => Near(rect.anchoredPosition.y, 70f) && Near(rect.anchoredPosition.x, 90f));
        yield return Check("Scale", () => rect.DOScale(Vector3.one * 1.5f, Duration),
            () => Near(rect.localScale.x, 1.5f));
        yield return Check("Rotation", () => rect.DORotate(45f, Duration),
            () => Near(rect.eulerAngles.z, 45f));
        yield return Check("Size", () => rect.DOSizeDelta(new Vector2(160f, 120f), Duration),
            () => Near(rect.sizeDelta, new Vector2(160f, 120f)));
        yield return Check("Pivot", () => rect.DOPivot(new Vector2(0.25f, 0.75f), Duration),
            () => Near(rect.pivot, new Vector2(0.25f, 0.75f)));
        yield return Check("Color", () => image.DOColor(Color.red, Duration),
            () => Near(image.color.g, 0f));
        yield return Check("Image fade", () => image.DOFade(0.5f, Duration),
            () => Near(image.color.a, 0.5f));
        yield return Check("CanvasGroup fade", () => group.DOFade(0.25f, Duration),
            () => Near(group.alpha, 0.25f));

        float customFloat = 0f;
        yield return Check("Custom float",
            () => TweenManager.GetSpawner().CustomFloat(0f, 7f, Duration).OnFloatUpdate(value => customFloat = value),
            () => Near(customFloat, 7f));

        Vector3 customVector = Vector3.zero;
        yield return Check("Custom vector",
            () => TweenManager.GetSpawner().CustomVector(Vector3.zero, Vector3.one * 3f, Duration).OnVectorUpdate(value => customVector = value),
            () => Near(customVector.x, 3f));

        int starts = 0;
        int updates = 0;
        int completes = 0;
        yield return Check("OnStart, OnUpdate, OnComplete",
            () => rect.DOAnchorPosX(0f, Duration).OnStart(() => starts++).OnUpdate(() => updates++).OnComplete(() => completes++),
            () => starts == 1 && updates > 1 && completes == 1);

        RectTransform stateTarget = null;
        yield return Check("OnComplete with state",
            () => rect.DOAnchorPosX(5f, Duration).OnComplete(rect, target => stateTarget = target),
            () => stateTarget == rect);

        yield return CheckDelay();
        yield return CheckLoop();
        yield return CheckThen();
        yield return CheckSequential();
        yield return CheckPauseAndCancel();
        yield return CheckStaleHandle();

        string summary = $"[TweenTypeCheck] Finished: {passed} passed, {failed} failed.";
        if (failed > 0)
            Debug.LogError(summary);
        else
            Debug.Log(summary);
    }

    private IEnumerator Check(string name, Action start, Func<bool> isCorrect)
    {
        start();
        yield return new WaitForSecondsRealtime(Duration + Settle);
        Report(name, isCorrect());
    }

    private IEnumerator Check(string name, Func<TweenBuilder> start, Func<bool> isCorrect)
        => Check(name, () => { start(); }, isCorrect);

    private IEnumerator CheckDelay()
    {
        rect.anchoredPosition = Vector2.zero;
        rect.DOAnchorPosX(50f, Duration).Delay(0.3f);

        yield return new WaitForSecondsRealtime(0.15f);
        bool waited = Near(rect.anchoredPosition.x, 0f);

        yield return new WaitForSecondsRealtime(0.3f + Duration);
        Report("Delay", waited && Near(rect.anchoredPosition.x, 50f));
    }

    private IEnumerator CheckLoop()
    {
        rect.anchoredPosition = Vector2.zero;
        int completes = 0;
        rect.DOAnchorPosX(30f, Duration).Loop(2).OnComplete(() => completes++);

        yield return new WaitForSecondsRealtime(Duration * 3f + Settle);
        Report("Loop", completes == 1 && Near(rect.anchoredPosition.x, 30f));

        rect.DOAnchorPosX(60f, 0.1f).LoopInfinite();
        yield return new WaitForSecondsRealtime(0.35f);
        bool stillRunning = TweenManager.GetComponentTweenHandlesCount() > 0;
        rect.DoCancel();
        yield return null;
        Report("Infinite loop", stillRunning);
    }

    private IEnumerator CheckThen()
    {
        rect.anchoredPosition = Vector2.zero;
        TweenHandle second = rect.DOAnchorPosY(25f, Duration).GetHandle();
        rect.DOAnchorPosX(25f, Duration).Then(second);

        yield return new WaitForSecondsRealtime(Duration * 2f + Settle);
        Report("Then", Near(rect.anchoredPosition, new Vector2(25f, 25f)));
    }

    private IEnumerator CheckSequential()
    {
        rect.anchoredPosition = Vector2.zero;
        TweenSpawner spawner = TweenManager.GetSpawner();
        spawner.Sequential(
            rect.DOAnchorPosX(15f, Duration).DeclareOnly(),
            spawner.Wait(0.05f),
            rect.DOAnchorPosY(15f, Duration).DeclareOnly());

        yield return new WaitForSecondsRealtime(Duration * 2f + 0.05f + Settle);
        Report("Sequential and Wait", Near(rect.anchoredPosition, new Vector2(15f, 15f)));
    }

    private IEnumerator CheckPauseAndCancel()
    {
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.DOAnchorPosX(100f, 1f);
        rect.DOScale(Vector3.one * 2f, 1f);

        yield return new WaitForSecondsRealtime(0.2f);
        rect.DoPause();
        yield return null;
        float pausedX = rect.anchoredPosition.x;
        float pausedScale = rect.localScale.x;

        yield return new WaitForSecondsRealtime(0.2f);
        Report("DoPause stops both tweens", Near(rect.anchoredPosition.x, pausedX) && Near(rect.localScale.x, pausedScale));

        rect.DoPlay();
        yield return new WaitForSecondsRealtime(0.2f);
        Report("DoPlay resumes both tweens", rect.anchoredPosition.x > pausedX && rect.localScale.x > pausedScale);

        rect.DoCancel();
        yield return null;
        yield return null;
        float cancelledX = rect.anchoredPosition.x;
        float cancelledScale = rect.localScale.x;

        yield return new WaitForSecondsRealtime(0.2f);
        Report("DoCancel stops both tweens", Near(rect.anchoredPosition.x, cancelledX) && Near(rect.localScale.x, cancelledScale));
    }

    // A finished tween's entity is reused, so its old handle must not reach the next tween.
    private IEnumerator CheckStaleHandle()
    {
        rect.anchoredPosition = Vector2.zero;
        TweenHandle oldHandle = rect.DOAnchorPosX(10f, 0.05f).GetHandle();
        yield return new WaitForSecondsRealtime(0.2f);

        bool oldHandleEnded = !oldHandle.IsPlaying;
        for (int i = 0; i < 64; i++)
            rect.DOAnchorPosX(40f, Duration);

        TweenManager.Cancel(oldHandle);
        TweenManager.Pause(oldHandle);

        yield return new WaitForSecondsRealtime(Duration + Settle);
        Report("Old handle can't touch a new tween", oldHandleEnded && Near(rect.anchoredPosition.x, 40f));
    }

    private void CreateTarget()
    {
        GameObject canvasObject = new("Check Canvas", typeof(Canvas));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject target = new("Check Target", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        target.transform.SetParent(canvasObject.transform, false);

        rect = target.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 100f);
        image = target.GetComponent<Image>();
        group = target.GetComponent<CanvasGroup>();
    }

    private void Report(string name, bool isCorrect)
    {
        if (isCorrect)
        {
            passed++;
            Debug.Log($"[TweenTypeCheck] PASS: {name}");
        }
        else
        {
            failed++;
            Debug.LogError($"[TweenTypeCheck] FAIL: {name}");
        }
    }

    private static bool Near(float a, float b)
        => Mathf.Abs(a - b) < 0.05f;

    private static bool Near(Vector2 a, Vector2 b)
        => Near(a.x, b.x) && Near(a.y, b.y);
}
