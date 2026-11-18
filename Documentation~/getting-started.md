# Getting Started

[← Back to README](../README.md)

- [Requirements](#requirements)
- [Installation](#installation)
- [Setup](#setup)
- [Quick start](#quick-start)
- [Usage example](#usage-example)
- [Next steps](#next-steps)

---

## Requirements

| Requirement | Supported |
|---|---|
| Unity | Tested on Unity 6 (`6000.3.16f1`). The package declares `2022.3` as its minimum, but older versions aren't tested yet. |
| Scripting backend | IL2CPP (Release) measured on Android and iOS with Unity `6000.3.6f1`. Mono player builds aren't tested yet. |
| Platforms | Android (Pixel 5, Galaxy A02s) and iOS (iPhone 13 mini, iPhone Air), each running the full benchmark. See [Performance](performance.md). WebGL and desktop aren't verified yet. |
| UI targets | `RectTransform`, any `Graphic` (`Image`, `RawImage`, `Text`, TextMeshPro), `CanvasGroup` |
| Enter Play Mode with domain reload disabled | Not verified yet. The package has no static reset for this mode, so keep domain reload on until you've tested it. |
| Dependencies | [LeoECS Core](https://github.com/LeoECSCommunity/ecs) `2022.2.22`, uGUI |

## Installation

Git must be installed on your machine.

### 1. Install LeoECS

UI Tweens requires [LeoECS Core](https://github.com/LeoECSCommunity/ecs) version `2022.2.22`.

1. In Unity, open **Window > Package Manager**.
2. Select **+ > Add package from git URL**.
3. Enter:

```text
https://github.com/LeoECSCommunity/ecs.git#91cc7e390fa0eb4fc88a5cf111df88b1f45af37e
```

Use this exact URL. It points to the last LeoECS release under the standard MIT license. Later releases from other sources use a different license.

UI Tweens uses classic LeoECS (`com.leopotam.ecs`), not LeoECS Lite.

### 2. Install UI Tweens

1. In Unity, open **Window > Package Manager**.
2. Select **+ > Add package from git URL**.
3. Enter:

```text
https://github.com/sgonzalezrozo/unity-ui-tweens.git
```

## Setup

There's nothing to set up, and nothing runs until you need it. The first tween creates `TweenManager` and sets it up, so don't add it to a scene. To keep that work off your first animation, call `TweenManager.Initialize()` or `TweenManager.Prewarm()` during a loading screen. See [Startup and the tween pool](core-concepts.md#startup-and-the-tween-pool).

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

This component slides a panel in each time its GameObject is enabled, for example when a screen opens. It cancels the tween when the GameObject is disabled, so a late animation can't move the hidden panel.

```csharp
using UITweens.Extensions;
using UITweens.Math;
using UnityEngine;

public class SlideInPanel : MonoBehaviour
{
    private const float Duration = 0.65f;
    private const float EntranceDelay = 0.25f;

    [SerializeField] private RectTransform panel;
    [SerializeField] private EaseType ease = EaseType.QuadOut;

    private void OnEnable()
    {
        panel.anchoredPosition = OffscreenPosition;
        panel.DOAnchorPos(Vector2.zero, Duration, ease).Delay(EntranceDelay);
    }

    private void OnDisable()
    {
        panel.DoCancel();
        panel.anchoredPosition = OffscreenPosition;
    }

    private Vector2 OffscreenPosition => new(0f, panel.rect.height);
}
```

## Next steps

- [Code Examples](code-examples.md): sequences, loops, pause menus, progress bars, custom values
- [API Reference](api-reference.md): every tween method, option and control
- [Core Concepts](core-concepts.md): the tween pool, architecture and how it compares
