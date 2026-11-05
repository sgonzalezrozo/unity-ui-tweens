# Benchmark evidence format

All benchmark numbers published for this package must be backed by raw JSON files that follow `benchmark-result.schema.json`.

## Required fields

- `tool`: package identifier (`"ui-tweens"` for this package)
- `schemaVersion`: schema version (currently `"1.0.0"`)
- `timestampUtc`: ISO-8601 UTC timestamp
- `session`: environment metadata (device, OS, Unity, build mode, backend, commit)
- `workload`: benchmark target metadata (scenario, variant, run index, warmup frames, measured frames)
- `metrics`: numeric measurements by name and unit

## Summary rules

- Publish medians and p95 from repeated runs per scenario/variant.
- Keep raw run JSON files in-repo and reference them from any README claim.
- Include unfavorable results; do not drop outliers silently.

## Where files go

- `results/raw/`: copy the JSON files from the device here.
- `results/summary/`: written by `tools/summarize_results.py`.

Run `python3 tools/summarize_results.py` from this folder. It stops on any invalid file, and groups results by commit, device and build.
