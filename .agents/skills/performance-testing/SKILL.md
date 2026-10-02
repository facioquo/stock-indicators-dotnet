---
name: performance-testing
description: Benchmark indicators with BenchmarkDotNet in `tools/performance` — add Series, Buffer, Stream, and style-comparison benchmarks, spot-check one indicator against committed baselines, evaluate the full suite for regressions, reset baselines, and run the large-N harness. Targets are BufferList ≤ 1.2x and StreamHub ≤ 1.5x the Series time. Use when adding a new indicator, editing a `tools/performance/Perf.*.cs` file, after changing an indicator's hot path, when `perf.sh` or `detect-regressions.sh` reports a regression, or before refreshing files in `tools/performance/baselines/`.
---

# Performance testing

Run every benchmark workflow through `tools/performance/perf.sh` from the repository root. `tools/performance/benchmarking.md` is the human-facing guide to the same tooling.

Load [benchmark patterns](references/benchmark-patterns.md) before adding or editing a `[Benchmark]` method or benchmark class.

## Workflows

`evaluate` and `spot` require `jq`.

```bash
# Spot check: one indicator against its baselines (fast; the dev-loop check)
bash tools/performance/perf.sh spot Ema
bash tools/performance/perf.sh spot Adx Stream   # Series | Buffer | Stream | All (default)

# Evaluate: full baseline suite, report regressions and improvements (~1 hour)
bash tools/performance/perf.sh evaluate

# Reset: full baseline suite, overwrite tools/performance/baselines/ (~1 hour)
bash tools/performance/perf.sh reset
```

- `spot` matches the indicator name as a substring, so `spot Ema` also runs `Dema` and `Tema`.
- VS Code tasks: "Perf: Spot check (indicator vs baseline)", "Perf: Evaluate against baselines", "Perf: Reset baselines".
- `reset --prune` also deletes committed baseline files for suites no longer in the set.

## Baseline set

The baseline set is the no-argument `dotnet run -c Release` list in `tools/performance/Program.cs`: `SeriesIndicators`, `BufferIndicators`, `StreamIndicators`, `StreamObserver`, `Utility`, `UtilityNullMath`, `UtilityStdDev`. `BASELINE_CLASSES` in `perf.sh` must list the same suites; change both together.

`StyleComparison`, `StyleWallTime`, `StreamCrossover`, `StreamExternal`, and `ManualTestDirect` are diagnostics with no baseline. Run them with a raw filter.

## Raw runs

For exploration only, never for baseline comparison. Run from `tools/performance`:

```bash
dotnet run -c Release -- --filter "*StyleComparison*"   # one suite
dotnet run -c Release -- --filter "*.ToEmaBatch"         # one method

# Large-N harness: PERF_TEST_PERIODS bars (default 500000); PERF_TEST_CAP below it exercises pruning
PERF_TEST_KEYWORD=ema PERF_TEST_PERIODS=500000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"
PERF_TEST_KEYWORD=adl PERF_TEST_PERIODS=500000 PERF_TEST_CAP=100000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"
```

Results land in `tools/performance/BenchmarkDotNet.Artifacts/results/` as `Performance.{Suite}-report-full.json` and `-report-github.md`.

## Regression detection

`detect-regressions.sh` pairs each `*-report-full.json` result with the same-named baseline and compares per method, so a spot run compares only what it ran. The default threshold is 10%. Run it alone on existing results with `bash tools/performance/detect-regressions.sh [--threshold 15]`. Exit codes: `0` no regression, `1` regression found, `2` usage or I/O error.

## Targets

| Style | Target time vs Series |
| ----- | --------------------- |
| BufferList | ≤ 1.2x |
| StreamHub | ≤ 1.5x |

These are optimization targets, not merge gates; current implementations vary by indicator. Measure the ratio with the `StyleComparison` suite. Read current per-indicator timings from `tools/performance/baselines/Performance.SeriesIndicators-report-github.md` and its Buffer and Stream siblings.

## Do not do these

- Do not pass tuning options (`--job`, `--warmupCount`, thresholds) to `perf.sh` runs; each suite pins its job in code, and plain runs stay comparable to the committed baselines.
- Do not run `perf.sh reset` except to record an intended, verified performance change, and commit the refreshed baselines with that change.
- Do not compare baselines across machines or gate merges on CI timings; the committed baselines come from a developer machine, and the `test-performance*.yml` workflows are manual and informational.
- Do not add LINQ, boxing, or per-period allocations other than the result itself to an indicator's per-period loop.
