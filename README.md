<div align="center">

# Unity UI Tweens

<sub>DESIGNED & BUILT BY</sub><br>
**✦ [`SANTIAGO GONZÁLEZ ROZO (@sgonzalezrozo)`](https://github.com/sgonzalezrozo) ✦**

An ECS-based tween library for Unity UI with a DOTween-style API. On four phones: 0.6–3.7 ms per frame for 1,000 tweens and zero garbage collections across 17 scenarios.

[![Unity](https://img.shields.io/badge/Unity-6000.3%20tested-black?style=for-the-badge&logo=unity)](Documentation~/getting-started.md#requirements)
[![Version](https://img.shields.io/badge/version-0.4.6-blue?style=for-the-badge)](CHANGELOG.md)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

[**Installation**](#installation) • [**Key Features**](#key-features) • [**Quickstart**](#quickstart) • [**Code Examples**](Documentation~/code-examples.md) • [**Core Concepts**](Documentation~/core-concepts.md) • [**API Reference**](Documentation~/api-reference.md)

<sub>[Getting Started](Documentation~/getting-started.md) • [Performance](Documentation~/performance.md) • [AI Agents](Documentation~/ai-agents.md) • [Troubleshooting](Documentation~/troubleshooting.md) • [FAQ](Documentation~/faq.md) • [Project Status](Documentation~/project-status.md)</sub>

</div>

---

## Installation

UI Tweens depends on [LeoECS Core](https://github.com/LeoECSCommunity/ecs). Unity doesn't install Git dependencies automatically, so add both, in this order:

1. Open **Window > Package Manager** in Unity.
2. Click **`+`** in the top-left corner and select **Add package from git URL...**
3. Paste each URL below and click **Add**:

```text
https://github.com/LeoECSCommunity/ecs.git#91cc7e390fa0eb4fc88a5cf111df88b1f45af37e
```

```text
https://github.com/sgonzalezrozo/unity-ui-tweens.git
```

Use the LeoECS URL exactly as written: it points to the last release under the standard MIT license. There's nothing to set up after installing. Requirements and tested platforms: [Getting Started](Documentation~/getting-started.md#requirements).

---

## Key Features

* **[Fluent, DOTween-style API](Documentation~/api-reference.md#tween-methods)**: `DOAnchorPos`, `DOScale`, `DOFade`, `DOColor`, `DOFillAmount` and more.
  Works on `RectTransform`, any `Graphic` (`Image`, `RawImage`, `Text`, TextMeshPro) and `CanvasGroup`.
* **[Allocation-conscious](Documentation~/performance.md)**: tweens are pooled entities in an always-running ECS world.
  No garbage collections ran in any of 17 benchmark scenarios on four phones.
* **[Per-component control](Documentation~/api-reference.md#controlling-tweens)**: pause, resume, cancel, complete, restart or rewind every tween on a component.
  Choose what happens when its GameObject is disabled.
* **[Sequences and loops](Documentation~/code-examples.md#run-tweens-in-order)**: `Then` and `Sequential`, delays, restart or yoyo loops, `From()` and `Relative()` values.
* **[Built for UI](Documentation~/api-reference.md#easing)**: 33 easing types, unscaled time for pause menus, custom float and vector tweens, and automatic cleanup when targets are destroyed.

---

## Quickstart

Call an extension method on any UI component:

```csharp
panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut)
    .Delay(0.05f)
    .OnComplete(() => Debug.Log("Panel shown"));
```

<details>
<summary><strong>View full sample script</strong></summary>

```csharp
using UITweens.Extensions;
using UITweens.Math;
using UnityEngine;

// Slides a panel in each time its GameObject is enabled, for example when a screen opens.
// Cancels the tween on disable, so a late animation can't move the hidden panel.
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

</details>

The full walkthrough is in [Getting Started](Documentation~/getting-started.md).
