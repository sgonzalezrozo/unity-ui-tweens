# UI Tweens rules for AI coding agents

Copy this file into your project's agent rules (for example `AGENTS.md`, `CLAUDE.md` or `.cursor/rules/ui-tweens.md`).

## How UI animation works here

- This project animates uGUI with **UI Tweens** (`com.sgonzalezrozo.ui-tweens`). Don't add DOTween, PrimeTween, LitMotion, coroutines or `Update()` loops for UI animation.
- Tweens are extension methods on `RectTransform`, any `Graphic` (`Image`, `RawImage`, `Text`, TextMeshPro) and `CanvasGroup`. Add `using UITweens.Extensions;` and `using UITweens.Math;` (for `EaseType`).
- Don't add `TweenManager` to a scene. The first tween creates it. To move setup off the first animation, call `TweenManager.Initialize()` or `TweenManager.Prewarm(count)` during a loading screen.

## Assembly references

Unity doesn't add a package's assemblies to your assembly definitions. Every `.asmdef` whose code uses this package MUST list these in its **Assembly Definition References** (`references` in the JSON), in the same change that adds the first `using`. Without them the build fails with `CS0246: The type or namespace name 'UITweens' could not be found`. Code in `Assembly-CSharp` (no `.asmdef`) needs nothing.

| Assembly | Needed for |
|---|---|
| `UITweens` | `DOAnchorPos`, `DOFade`, `EaseType` and the rest (namespaces `UITweens.Extensions`, `UITweens.Math`) |
| `Leopotam.Ecs` | Only if your code uses LeoECS types directly |

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
| `Graphic` (`Image`, `RawImage`, `Text`, TextMeshPro) | `DOFade`, `DOColor`, `DOColorFromTo` |
| `Image` | `DOFillAmount` (0 to 1) |
| `CanvasGroup` | `DOFade`, `DOFadeFromTo` |

Options: `Delay`, `Loop(repeatCount, LoopType)`, `LoopInfinite(LoopType)`, `From`, `Relative`, `IgnoreTimeScale`, `WhenDisabled`, `Then(handle)`, `OnStart`, `OnUpdate`, `OnComplete`, `GetHandle`, `DeclareOnly`. `Loop(2)` plays three times. `LoopType.Yoyo` (namespace `UITweens`) plays back and forth; the default `LoopType.Restart` starts each loop over. `From()` plays from the given value to the current one and sets the given value at once. `Relative()` treats the value as an offset from where the tween starts.

## Control and clean up

- Per component: `DoPause()`, `DoPlay()`, `DoTogglePause()`, `DoCancel()`, `DoComplete()`, `DoRestart()`, `DoRewind()`.
- One tween: keep `TweenHandle handle = ...GetHandle();`, then `TweenManager.Pause(handle)`, `Resume(handle)`, `Cancel(handle)`, `Complete(handle)`, `Restart(handle)` or `Rewind(handle)`.
- For objects that get disabled or pooled, use `.WhenDisabled(DisableBehaviour.Pause)` (or `Cancel`, `Complete`), or set `TweenManager.DisableBehaviourByDefault`. Otherwise cancel tweens in `OnDisable`, so a late tween can't move a hidden object. Tweens on destroyed targets are cancelled automatically.
- Sequences: `TweenManager.GetSpawner().Sequential(a.DeclareOnly(), spawner.Wait(0.2f), b.DeclareOnly())`.
- Custom values: `TweenManager.GetSpawner().CustomFloat(from, to, duration, ease).OnFloatUpdate(callback)`.

## Don't

- Don't invent methods. If a method isn't listed here or in the [API Reference](api-reference.md), read the package source.
- Don't expect tweens to run while `Time.timeScale` is `0` unless they use `IgnoreTimeScale()` or `TweenManager.IgnoreTimeScaleByDefault` is true.
- Don't use it for UI Toolkit elements or non-UI transforms.
