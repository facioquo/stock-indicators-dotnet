# Performance baselines

Committed BenchmarkDotNet baseline artifacts used for performance regression
checks. For how to run benchmarks and refresh these files, see the canonical
[benchmarking guide](../benchmarking.md).

## What is stored here

Two files per baseline suite:

- `Performance.<Suite>-report-full.json` — machine-readable (regression input)
- `Performance.<Suite>-report-github.md` — human-readable tables (committed for review)

## Baseline set

These suites are the committed baseline (matches the no-arg `dotnet run -c Release`
default in `Program.cs` and `BASELINE_CLASSES` in `perf.sh`):

- `SeriesIndicators`, `BufferIndicators`, `StreamIndicators` — every indicator, per style
- `Utility`, `UtilityNullMath`, `UtilityStdDev` — shared hot paths / helpers

`StyleComparison`, `StreamExternal`, and `ManualTestDirect` are diagnostics and
are **not** baselined here.

## Refresh and check

Run from the repository root:

```bash
# Regenerate all baseline files (run + copy)
bash tools/performance/perf.sh reset

# Compare current results against these baselines
bash tools/performance/perf.sh evaluate
```

## Notes

- The `-github.md` and `-report-full.json` files are committed on purpose; only
  `*.zip` archives are git-ignored here.
- Keep baseline refreshes tied to intentional, verified performance work.
- Historical pre-fix snapshots were retired; use git history/tags for older baselines.
