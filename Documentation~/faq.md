# FAQ

[← Back to README](../README.md)

**About the project**
- [Is it right for my project?](#is-it-right-for-my-project)
- [Is it production-ready? Does anyone use it?](#is-it-production-ready-does-anyone-use-it)
- [Why was it built?](#why-was-it-built)
- [Is it free for commercial use?](#is-it-free-for-commercial-use)

**Choosing a tween library**
- [How do I animate UI in Unity without DOTween?](#how-do-i-animate-ui-in-unity-without-dotween)
- [UI Tweens vs DOTween vs PrimeTween vs LitMotion: which should I use?](#ui-tweens-vs-dotween-vs-primetween-vs-litmotion-which-should-i-use)
- [Is there a DOTween alternative for Unity UI?](#is-there-a-dotween-alternative-for-unity-ui)
- [Is it faster than DOTween?](#is-it-faster-than-dotween)

**Compatibility and usage**
- [Does it work with Unity 6?](#does-it-work-with-unity-6)
- [Does it work with IL2CPP, mobile and WebGL?](#does-it-work-with-il2cpp-mobile-and-webgl)
- [Does it work with UI Toolkit?](#does-it-work-with-ui-toolkit)
- [Do tweens run while the game is paused?](#do-tweens-run-while-the-game-is-paused)
- [Why does it need LeoECS?](#why-does-it-need-leoecs)
- [Does it work with AI coding agents?](#does-it-work-with-ai-coding-agents)

---

## About the project

### Is it right for my project?

**Use UI Tweens if you want:**

- Allocation-conscious UI animation
- A small, fluent API focused on Unity UI
- An ECS-based animation runtime
- Per-component pause, resume and cancel control

**Consider something else if you:**

- Need to animate more than Unity UI, such as 3D transforms or materials, or TextMeshPro beyond color and alpha. DOTween, PrimeTween and LitMotion are general-purpose alternatives.
- Don't want LeoECS in your project.

### Is it production-ready? Does anyone use it?

Not yet. UI Tweens is a pre-release package (`0.x`) by a single author. It isn't used in a shipped game, and it has no other users yet, so expect rough edges and API changes before `1.0.0`. Bug reports are very welcome. See [Project Status](project-status.md).

### Why was it built?

I've used DOTween Pro for as long as I can remember. When everything goes well, there's no problem. But finding an issue with a tween can be really time-consuming, and that was the main reason I wanted to make my own.

My first idea was a wrapper around DOTween. Then I wondered whether it would be better to run tweens in an always-running world, an ECS, rather than in something that appears and disappears.

That was meant as a possible performance improvement, and it turned out to matter. DOTween works great, but in my experience it can have FPS issues beyond a couple of hundred tweens, and in gameplay they add up very fast.

I built and refined it on my own time in personal projects. In November 2026 I extracted it into this standalone package, with documentation and tests. Earlier history lives in a private repository.

### Is it free for commercial use?

Yes. It's MIT licensed.

## Choosing a tween library

### How do I animate UI in Unity without DOTween?

Install UI Tweens and LeoECS, then call an extension method on the component: `panel.DOAnchorPos(Vector2.zero, 0.35f, EaseType.BackOut)`. See [Getting Started](getting-started.md#quick-start).

### UI Tweens vs DOTween vs PrimeTween vs LitMotion: which should I use?

UI Tweens focuses only on Unity UI and runs every tween in one always-running ECS world. DOTween, PrimeTween and LitMotion are mature, general-purpose and animate any property. UI Tweens ran 17 benchmark scenarios on four phones without a garbage collection. Choose UI Tweens if you want a small UI-only API with per-component control, or already use LeoECS. See [How it compares](core-concepts.md#how-it-compares).

### Is there a DOTween alternative for Unity UI?

Yes, several. PrimeTween and LitMotion are general-purpose alternatives. UI Tweens is a newer, UI-only option with a DOTween-style API, so `panel.DOFade(0f, 0.2f)` reads the same. See [How it compares](core-concepts.md#how-it-compares).

### Is it faster than DOTween?

This package doesn't publish comparisons with other libraries yet. Its own numbers on four phones are in [Performance](performance.md). Measure your own project before choosing.

## Compatibility and usage

### Does it work with Unity 6?

Yes. It's tested on Unity `6000.3`. The package declares `2022.3` as its minimum, but older versions aren't tested yet.

### Does it work with IL2CPP, mobile and WebGL?

IL2CPP and mobile, yes: it's measured in IL2CPP Release builds on two Android phones (Pixel 5, Galaxy A02s) and two iPhones (iPhone 13 mini, iPhone Air), with no garbage collections in any of the 17 scenarios. WebGL isn't verified yet. See [Requirements](getting-started.md#requirements) and [Performance](performance.md).

### Does it work with UI Toolkit?

No. It animates uGUI components: `RectTransform`, any `Graphic` (including TextMeshPro) and `CanvasGroup`.

### Do tweens run while the game is paused?

Only if you ask them to. Tweens follow `Time.timeScale` by default, so they stop when it's `0`. Add `IgnoreTimeScale()` to a tween, or set `TweenManager.IgnoreTimeScaleByDefault = true`, to keep UI animating on a pause menu.

### Why does it need LeoECS?

Tweens run as entities in a LeoECS world that never shuts down, which avoids creating and destroying tween objects. Install LeoECS first, as shown in [Installation](getting-started.md#installation).

### Does it work with AI coding agents?

It ships a rules file you copy into your project, so agents use its real API instead of guessing. See [AI Coding Agents](ai-agents.md).
