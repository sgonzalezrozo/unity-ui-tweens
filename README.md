# UI Tweens

**An ECS-based animation engine for Unity UI, running 350+ simultaneous tweens with zero GC allocations.**

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

UI Tweens runs on LeoECS and provides a fluent API for `RectTransform`, `Image` and `CanvasGroup`, with chaining, loops, callbacks, 33 easing types and per-component pause, resume and cancel controls.

> **TODO:** Add a GIF of tweens running together.

## Why I built it

On mobile games, smooth UI often comes down to allocation discipline. On World War Doh, keeping scrolling UI at 60 FPS came mostly from cutting GC allocations.

I wanted UI animation that kept that same discipline, with a simple API and explicit tracking of each component's tweens.

> **TODO:** Replace this with your own story, including why you chose to build this instead of using DOTween or another existing library.

## Is it right for your project?

**Use UI Tweens if you want:**

- Allocation-conscious UI animation
- A small, fluent API focused on Unity UI
- An ECS-based animation runtime
- Per-component pause, resume and cancel control

**Consider something else if you:**

- Need a mature, general-purpose animation ecosystem. DOTween, PrimeTween and LitMotion are established alternatives.
- Don't want LeoECS in your project.
- Need animations to keep running while `Time.timeScale` is `0`.

## Features

- Runs on LeoECS
- Fluent API for `RectTransform`, `Image` and `CanvasGroup`
- Delays, finite and infinite loops, chaining and callbacks
- 33 built-in easing types
- Per-component pause, resume, toggle and cancel controls
- Custom float and vector tweens
- Automatic cleanup when targets are destroyed
- Configurable pool sizes with `UITweenPoolConfig`

## Compatibility

| Requirement | Supported |
|---|---|
| Unity | 2022.3 or newer. **TODO:** add tested versions. |
| Scripting backend | **TODO:** confirm Mono and IL2CPP |
| Platforms | **TODO:** list tested platforms |
| UI targets | `RectTransform`, `Image`, `CanvasGroup` |
| Enter Play Mode with domain reload disabled | **TODO:** confirm support |
| Dependencies | LeoECS Core, uGUI |

## Installation

Git must be installed on your machine.

UI Tweens requires [LeoECS Core](https://openupm.com/packages/com.leopotam.ecs).

### 1. Install LeoECS

1. Open **Edit > Project Settings > Package Manager**.
2. Under **Scoped Registries**, add:
   - **Name:** `OpenUPM`
   - **URL:** `https://package.openupm.com`
   - **Scope(s):** `com.leopotam`
3. Select **Save** or **Apply**.
4. Open **Window > Package Manager**, select **My Registries**, then install LeoECS (`com.leopotam.ecs`).

> **TODO:** Add the LeoECS version you tested, and test these steps in a clean project.

### 2. Install UI Tweens

1. In Unity, open **Window > Package Manager**.
2. Select **+ > Add package from git URL**.
3. Enter:

```text
https://github.com/sgonzalezrozo/unity-ui-tweens.git
```

To pin a release, add its tag:

```text
https://github.com/sgonzalezrozo/unity-ui-tweens.git#v0.2.0
```

## Setup

UI Tweens loads a `TweenManager` prefab after the first scene loads.

1. Run **Tools > UI Tweens > Setup**.
2. Confirm that `Assets/Resources/Tools/UITweens/TweenManager.prefab` exists.

The prefab loads automatically after the first scene loads, so don't add it to a scene.

If the prefab is missing, the Console shows a `TweenManager prefab not found` warning. Start tweens from `Start()` or later, not `Awake()`.

## Quick start

```csharp
using UITweens.Extensions;
using UITweens.Math;
using UnityEngine;

public class PanelAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform panel;

    public void Show()
    {
        panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut)
            .Delay(0.05f)
            .OnComplete(() => Debug.Log("Panel shown"));
    }
}
```

## Usage example

This component slides a panel in when its screen opens, using [Screen System](https://github.com/sgonzalezrozo/unity-screen-system). It cancels the tween when the screen closes, so a late animation can't move the hidden panel.

```csharp
using ScreenSystem;
using UITweens.Extensions;
using UITweens.Math;
using UnityEngine;

public class SlideInPanel : MonoBehaviour, IScreenLifecycle
{
    private const float Duration = 0.65f;
    private const float EntranceDelay = 0.25f;

    [SerializeField] private RectTransform panel;
    [SerializeField] private EaseType ease = EaseType.QuadOut;

    public void OnScreenShown()
    {
        panel.anchoredPosition = OffscreenPosition;
        panel.DOAnchorPos(Vector2.zero, Duration, ease).Delay(EntranceDelay);
    }

    public void OnScreenHidden()
    {
        panel.DoCancel();
        panel.anchoredPosition = OffscreenPosition;
    }

    private Vector2 OffscreenPosition => new(0f, panel.rect.height);
}
```

To use it without Screen System, remove `IScreenLifecycle` and call these methods yourself.

## Tween methods

| Target | Methods |
|---|---|
| `RectTransform` | `DOAnchorPos`, `DOAnchorPosX`, `DOAnchorPosY`, `DOScale`, `DOScaleX`, `DOScaleY`, `DOScaleZ`, `DORotate`, `DOSizeDelta`, `DOPivot` |
| `Image` | `DOFade`, `DOColor`, `DOColorFromTo` |
| `CanvasGroup` | `DOFade`, `DOFadeFromTo` |

For custom values, use `TweenManager.GetSpawner().CustomFloat(...)` or `CustomVector(...)`.

## Tween options

| Method | Purpose |
|---|---|
| `Delay(seconds)` | Wait before starting |
| `Loop(repeatCount)` | Repeat a set number of times |
| `LoopInfinite()` | Repeat until cancelled |
| `Then(handle)` | Run another tween next |
| `OnStart(callback)` | Run code when the tween starts |
| `OnUpdate(callback)` | Run code on each update |
| `OnComplete(callback)` | Run code when the tween finishes |
| `GetHandle()` | Get a `TweenHandle` for later control |
| `DeclareOnly()` | Create a declaration to spawn later |

## Controlling tweens

These work on `RectTransform`, `Image` and `CanvasGroup`, and apply to the tweens tracked for that component:

```csharp
panel.DoPause();
panel.DoPlay();
panel.DoTogglePause();
panel.DoCancel().OnComplete(() => Debug.Log("Cancelled"));
```

After `DoCancel()`, `OnComplete` is the cancellation callback. It runs when the cancellation is processed, or immediately if there's nothing to cancel.

To control one tween, keep its handle:

```csharp
TweenHandle handle = panel.DOAnchorPosX(200f, 0.5f, EaseType.QuadOut).GetHandle();

TweenManager.Pause(handle);
TweenManager.Resume(handle);
TweenManager.Cancel(handle);
```

`TweenHandle` also has `IsPlaying`, `IsPaused`, `IsCancelled` and `IsFinished`.

## Easing

| Family | Types |
|---|---|
| Linear | `Linear` |
| Quad, Cubic, Quart, Quint | `In`, `Out` and `InOut` of each, such as `QuadIn` |
| Sine, Expo, Circ | `In`, `Out` and `InOut` of each, such as `SineOut` |
| Back, Bounce, Elastic | `In`, `Out` and `InOut` of each, such as `BackOut` |
| Other | `SpringOut`, `PunchOut` |

## Runtime behavior

- Tweens use `Time.deltaTime`, so they stop when `Time.timeScale` is `0`.
- `TweenManager` persists across scene loads.
- Tweens are cancelled when their target is destroyed.
- When a delayed tween starts on a `RectTransform`, it cancels other tweens animating the same property.

## Architecture

- `TweenManager` owns the execution loop and lifecycle cleanup.
- `TweenBuilder` composes tween behavior and callbacks.
- `TweenSpawner` creates tween entities in the LeoECS world.
- Systems such as `TweenMathSystem` and `UGUITweenApplySystem` update and apply values.
- `TweenTracker` connects components to their active tweens.

## Performance

| Test | Result |
|---|---|
| Simultaneous tweens | 350+ |
| GC allocations | 0 |

> **TODO:** Add the device, Unity version, scripting backend, build type and a link to the benchmark project. State whether the zero-allocation result covers tween creation and callbacks, or the update loop only.

## Troubleshooting

| Problem | Try this |
|---|---|
| Compile errors mention `Leopotam` | Install LeoECS by following [Installation](#installation) |
| `TweenManager prefab not found` | Run Tools > UI Tweens > Setup. |
| Tweens stop in a pause menu | Tweens follow `Time.timeScale` |
| A tween doesn't start in `Awake()` | Start it from `Start()` or later |

## Limitations

- Tweens can't use unscaled time.
- TextMeshPro and UI Toolkit aren't supported directly.

## Project status

This is a pre-release package at version `0.2.0`. Its API and behavior may change before `1.0.0`.

> **TODO:** Add a roadmap link and breaking-change policy.

## Contributing

Bug reports and pull requests are welcome. Please open an issue before starting a large change.

## Related packages

| Package | Purpose |
|---|---|
| [Fast IoC](https://github.com/sgonzalezrozo/unity-fast-ioc) | Code-generated dependency injection |
| [Screen System](https://github.com/sgonzalezrozo/unity-screen-system) | Declarative UI navigation |
| [Recyclable Scroller](https://github.com/sgonzalezrozo/unity-recyclable-scroller) | Virtualized lists |
| [Enhanced Console](https://github.com/sgonzalezrozo/unity-enhanced-console) | Unity Editor log triage |
| [Tools Core](https://github.com/sgonzalezrozo/unity-tools-core) | Shared injection interfaces |

## License

Released under the [MIT License](LICENSE).

UI Tweens depends on [LeoECS Core](https://openupm.com/packages/com.leopotam.ecs), a separate project under its own license.

> **TODO:** Confirm the LeoECS license for the version you use, and credit any easing code derived from other work.
