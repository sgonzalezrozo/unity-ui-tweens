# UI Tweens rules for AI coding agents

Copy this file into your project's agent rules (for example `AGENTS.md`, `CLAUDE.md` or `.cursor/rules/ui-tweens.md`).

## How UI animation works here

- This project animates uGUI with **UI Tweens** (`com.sgonzalezrozo.ui-tweens`). Don't add DOTween, PrimeTween, LitMotion, coroutines or `Update()` loops for UI animation.
- Tweens are extension methods on `RectTransform`, `Image` and `CanvasGroup`. Add `using UITweens.Extensions;` and `using UITweens.Math;` (for `EaseType`).
- Don't add `TweenManager` to a scene. The first tween creates it. To move setup off the first animation, call `TweenManager.Initialize()` or `TweenManager.Prewarm(count)` during a loading screen.

## Start a tween

Every method ends with `float duration, EaseType ease = EaseType.Linear, float easeParameter = -1f`:

```csharp
panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut)
    .Delay(0.05f)
    .OnComplete(OnPanelShown);
```

| Target | Methods |
|---|---|
| `RectTransform` | `DOAnchorPos`, `DOAnchorPosX`, `DOAnchorPosY`, `DOScale`, `DOScaleX`, `DOScaleY`, `DOScaleZ`, `DORotate`, `DOSizeDelta`, `DOPivot` |
| `Image` | `DOFade`, `DOColor`, `DOColorFromTo` |
| `CanvasGroup` | `DOFade`, `DOFadeFromTo` |

Options: `Delay`, `Loop(repeatCount)`, `LoopInfinite`, `Then(handle)`, `OnStart`, `OnUpdate`, `OnComplete`, `GetHandle`, `DeclareOnly`. `Loop(2)` plays three times. There are no back-and-forth (yoyo) loops.

## Control and clean up

- Per component: `DoPause()`, `DoPlay()`, `DoTogglePause()`, `DoCancel()`.
- One tween: keep `TweenHandle handle = ...GetHandle();`, then `TweenManager.Pause(handle)`, `Resume(handle)` or `Cancel(handle)`.
- Cancel tweens when the object is hidden, for example in `OnDisable`, so a late tween can't move a hidden object. Tweens on destroyed targets are cancelled automatically.
- Sequences: `TweenManager.GetSpawner().Sequential(a.DeclareOnly(), spawner.Wait(0.2f), b.DeclareOnly())`.
- Custom values: `TweenManager.GetSpawner().CustomFloat(from, to, duration, ease).OnFloatUpdate(callback)`.

## Don't

- Don't invent methods. If a method isn't listed here or in the README, read the package source.
- Don't expect tweens to run while `Time.timeScale` is `0`.
- Don't use it for UI Toolkit elements or non-UI transforms.
