# Troubleshooting & Limitations

[← Back to README](../README.md)

## Troubleshooting

| Problem | Try this |
|---|---|
| Compile errors mention `Leopotam` | Install LeoECS by following [Installation](getting-started.md#installation) |
| Tweens stop in a pause menu | Tweens follow `Time.timeScale`. Add `IgnoreTimeScale()` to the tween |
| A declared tween never starts | Pass it to `Sequential()` |

## Limitations

- UI Toolkit isn't supported.
- TextMeshPro text animates color and alpha, like any `Graphic`, but there's no typewriter effect or per-character animation.

There's no public roadmap yet. Planned work is tracked in [issues](https://github.com/sgonzalezrozo/unity-ui-tweens/issues).
