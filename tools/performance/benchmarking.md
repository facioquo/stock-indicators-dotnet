# Performance benchmarking guide

How to run indicator performance benchmarks, refresh their baselines, and check for regressions with [BenchmarkDotNet](https://benchmarkdotnet.org/) in `tools/performance`. For baseline file conventions, see the [baselines README](baselines/README.md).

> [!NOTE]
> This guide covers timing baselines only. Regression baselines of indicator output values come from the [baseline generator](../baselining/README.md).

## Workflows

Run everything through `perf.sh` from the repository root. `evaluate` and `spot` need `jq`.

```bash
# Spot check: one indicator against the baselines (quick dev-loop check)
bash tools/performance/perf.sh spot Ema
bash tools/performance/perf.sh spot Adx Stream        # one style: Series, Buffer, Stream, or All

# Evaluate: full suite, report regressions and improvements against the baselines
bash tools/performance/perf.sh evaluate

# Reset: full suite, replace the committed baselines
bash tools/performance/perf.sh reset
bash tools/performance/perf.sh reset --prune          # also delete baseline files no longer in the set
```

`reset` and `evaluate` run the full suite, about an hour; `spot` runs one indicator.

These commands take no tuning options (`--job`, `--warmupCount`, thresholds). Each suite pins its BenchmarkDotNet job in code, so plain runs stay comparable with the committed baselines. Add options only for exploratory runs.

## The baseline set

The baseline set is what `dotnet run -c Release` runs with no arguments, and what `perf.sh reset` and `evaluate` cover:

| Suite | File | Coverage |
| ----- | ---- | -------- |
| `SeriesIndicators` | `Perf.Series.cs` | every indicator, Series style |
| `BufferIndicators` | `Perf.Buffer.cs` | every indicator, BufferList style |
| `StreamIndicators` | `Perf.Stream.cs` | every indicator, StreamHub style |
| `StreamObserver` | `Perf.StreamObserver.cs` | per-tick StreamHub delivery: provider alone, an `OnAdd`-only observer, and EMA/SMA/RSI/MACD hubs |
| `Utility` | `Perf.Utility.cs` | shared conversion and utility hot paths |
| `UtilityNullMath` | `Perf.Utility.NullMath.cs` | null-math helpers |
| `UtilityStdDev` | `Perf.Utility.StdDev.cs` | standard-deviation helper |

Two lists define the set and must stay in sync: the no-argument run in `Program.cs` and `BASELINE_CLASSES` in `perf.sh`.

### Diagnostics (not baselined)

Run these with `--filter` (see [raw runs](#raw-benchmarkdotnet-runs)):

- `StyleComparison` (`Perf.StyleComparison.cs`) — cross-style ratios; overlaps the three per-style suites, so it adds no regression signal.
- `StreamExternal` (`Perf.StreamExternal.cs`) — EMA Series-versus-stream microcheck.
- `StreamCrossover` (`Perf.StreamCrossover.cs`) — cost of one new bar at each history length: re-running Series versus adding to a warm hub. It backs the incremental-arrival guidance in [`docs/guide/styles/index.md`](../../docs/guide/styles/index.md).
- `StyleWallTime` (`Perf.StyleWallTime.cs`) — total time to process a whole dataset in each style, from 10 to 5,000 bars.
- `ManualTestDirect` (`Perf.ManualTestDirect.cs`) — large-N harness for one indicator, below.

## Large-N harness

`ManualTestDirect` runs one indicator at large bar counts without the full suite. Unlike `perf.sh spot`, it has no baseline. Environment variables select the indicator (`PERF_TEST_KEYWORD`, default `sma`), bar count (`PERF_TEST_PERIODS`, default 500,000), and cache cap (`PERF_TEST_CAP`, default the bar count).

```bash
cd tools/performance

# 500k bars of EMA
PERF_TEST_KEYWORD=ema PERF_TEST_PERIODS=500000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"

# cap below the bar count to exercise steady-state pruning
PERF_TEST_KEYWORD=adl PERF_TEST_PERIODS=500000 PERF_TEST_CAP=100000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"
```

## Raw BenchmarkDotNet runs

For exploration, not baseline comparison. Always use `-c Release`, and pass BenchmarkDotNet arguments after `--`:

```bash
cd tools/performance

dotnet run -c Release                                   # full baseline suite
dotnet run -c Release -- --filter "*SeriesIndicators*"  # one suite
dotnet run -c Release -- --filter "*.ToEmaBatch"        # one method
dotnet run -c Release -- --list flat                    # list every benchmark
```

Results land in `tools/performance/BenchmarkDotNet.Artifacts/results/`:

- `Performance.*-report-full.json` — machine-readable; the regression input
- `Performance.*-report-github.md` — human-readable tables

## Regression detection

`perf.sh evaluate` and `perf.sh spot` call `detect-regressions.sh`, which pairs each `*-report-full.json` in the results folder with the same-named file in `baselines/` and compares mean time per method. It compares only the suites present in the results, so a spot run compares just what it ran.

To compare results you already have:

```bash
# pair all current results with baselines (default threshold 10%)
bash tools/performance/detect-regressions.sh

# custom threshold, in percent
bash tools/performance/detect-regressions.sh --threshold 15

# one explicit pair; file paths resolve from the current directory
bash tools/performance/detect-regressions.sh \
  --baseline-file tools/performance/baselines/Performance.StreamIndicators-report-full.json \
  --current-file  tools/performance/BenchmarkDotNet.Artifacts/results/Performance.StreamIndicators-report-full.json
```

Exit codes: `0` no regressions, `1` regressions found, `2` usage or I/O error.

## VS Code tasks

- `Perf: Spot check (indicator vs baseline)` — `perf.sh spot`, prompting for indicator and style
- `Perf: Evaluate against baselines` — `perf.sh evaluate`
- `Perf: Reset baselines` — `perf.sh reset`
- `Test: Performance (all)`, `(Series)`, `(Buffer)`, `(Stream)` — raw runs of the full baseline suite or one per-style suite

## CI workflows

All three run on manual dispatch and are informational. Baselines come from a developer machine and CI runners differ, so do not gate merges on CI timings.

- `test-performance.yml` — full baseline suite
- `test-performance-comparison.yml` — `StyleComparison` diagnostic
- `test-performance-manual.yml` — `ManualTestDirect` for one indicator

## Practices

- Use `spot` in the dev loop; run `reset` only for intentional, verified performance work.
- Commit a baseline refresh with the change that shifted performance.
