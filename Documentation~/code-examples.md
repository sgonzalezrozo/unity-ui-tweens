# Code Examples

[← Back to README](../README.md)

- [Run tweens in order](#run-tweens-in-order)
- [Loop a tween](#loop-a-tween)
- [Pop in from nothing](#pop-in-from-nothing)
- [Animate a pause menu](#animate-a-pause-menu)
- [Fill a progress bar](#fill-a-progress-bar)
- [Fade a TextMeshPro label](#fade-a-textmeshpro-label)
- [Animate a custom value](#animate-a-custom-value)
- [Callbacks without allocations](#callbacks-without-allocations)

---

## Run tweens in order

Use `Then` to start a tween when another one finishes. It works on the same property too, and the second tween starts from wherever the first one ended:

```csharp
panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut)
    .Then(panel.DOAnchorPosX(40f, 0.2f).GetHandle())
    .OnComplete(() => Debug.Log("Both finished"));
```

`Then` returns a builder for the next tween, so `OnComplete` here belongs to the second tween. Cancelling a tween also cancels the ones waiting for it.

For longer sequences, declare each tween, then pass them to `Sequential`. A declared tween waits until it's passed in:

```csharp
TweenSpawner spawner = TweenManager.GetSpawner();

spawner.Sequential(
    panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut).DeclareOnly(),
    spawner.Wait(0.2f),
    content.DOFade(1f, 0.2f).DeclareOnly());
```

To overlap two tweens on different properties, use `Delay`:

```csharp
panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut);
content.DOFade(1f, 0.2f).Delay(0.25f);
```

## Loop a tween

```csharp
icon.DOScale(Vector3.one * 1.1f, 0.4f, EaseType.SineInOut).LoopInfinite();

// Later:
icon.DoCancel();
```

Each loop restarts from the beginning of the tween. To play back and forth instead, pass `LoopType.Yoyo`:

```csharp
arrow.DOAnchorPosY(10f, 0.4f, EaseType.SineInOut).Relative().LoopInfinite(LoopType.Yoyo);
```

`LoopType` is in the `UITweens` namespace.

## Pop in from nothing

```csharp
popup.DOScale(Vector3.zero, 0.3f, EaseType.BackOut).From();
```

`From()` sets the value straight away, even with `Delay()`, and ends at the value the target had when the tween was created.

## Animate a pause menu

```csharp
Time.timeScale = 0f;
pauseMenu.DOFade(1f, 0.2f).IgnoreTimeScale();
```

To make every tween ignore the time scale, set `TweenManager.IgnoreTimeScaleByDefault = true`. A single tween can still opt out with `IgnoreTimeScale(false)`.

## Fill a progress bar

Set the `Image`'s **Image Type** to **Filled**, then animate its fill amount:

```csharp
progressBar.DOFillAmount(0.75f, 0.4f, EaseType.QuadOut);

// A cooldown that empties in 5 seconds:
cooldown.fillAmount = 1f;
cooldown.DOFillAmount(0f, 5f);
```

## Fade a TextMeshPro label

TextMeshPro text is a `Graphic`, so `DOFade` and `DOColor` work on it like on an `Image`:

```csharp
label.DOFade(0f, 0.3f);              // label is a TMP_Text
label.DOColor(Color.red, 0.2f);
```

## Animate a custom value

`CustomFloat` and `CustomVector` animate a value that isn't tied to a UI component:

```csharp
using UITweens;
using UITweens.Math;
using UITweens.Services;
```

```csharp
TweenManager.GetSpawner()
    .CustomFloat(0f, 1000f, 0.5f, EaseType.QuadOut)
    .OnFloatUpdate(value => scoreLabel.text = Mathf.RoundToInt(value).ToString());
```

You can also read the current value with `TweenHandle.FloatValue` or `TweenHandle.VectorValue`.

## Callbacks without allocations

A callback that captures variables creates a closure. To avoid that, pass the value as state and use a `static` lambda:

```csharp
panel.DOFade(0f, 0.2f).OnComplete(panel, static target => target.gameObject.SetActive(false));
```

`Sequential()` doesn't allocate with up to four `DeclareOnly()` or `Wait()` steps. Longer sequences allocate one array, and so do declarations stored as `ITweenDeclaration`.
