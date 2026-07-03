# Performance benchmarking guide

Canonical guide for running indicator performance benchmarks, refreshing
baselines, and checking for regressions. Uses
[BenchmarkDotNet](https://benchmarkdotnet.org/) under `tools/performance`.

> **Not the same as correctness (regression) baselines** in `tools/baselining/`,
> which capture expected indicator *output values*
> (`dotnet run --project tools/baselining -- --all`). This guide is only about
> *performance* (timing) baselines.

## TL;DR — one script, three workflows

Run everything through `perf.sh` from the repository root. Requires `jq` for
`evaluate`/`spot`.

```bash
# 1. Spot check: one indicator vs baseline (fast; recommended dev-loop check)
bash tools/performance/perf.sh spot Ema
bash tools/performance/perf.sh spot Adx Stream        # single style

# 2. Evaluate: full suite, report regressions/improvements vs baselines
bash tools/performance/perf.sh evaluate

# 3. Reset baselines: full suite, replace committed baselines
bash tools/performance/perf.sh reset
```

`reset` and `evaluate` run the full suite (~1 hour). `spot` is quick.

**Keep runs comparable.** The commands above intentionally take no tuning options
(no `--job`, `--warmupCount`, threshold, etc.). Each suite pins its own
BenchmarkDotNet job in code, so plain runs compare apples-to-apples with the
committed baselines. Only add flags for raw exploration (see below), never for
baseline comparison.

## The baseline set

The **baseline set** is exactly what `dotnet run -c Release` (no arguments)
produces, and what `perf.sh reset`/`evaluate` cover:

| Suite | File (`Perf.*.cs`) | Coverage |
| ----- | ------------------ | -------- |
| `SeriesIndicators` | `Perf.Series.cs` | every indicator, Series style |
| `BufferIndicators` | `Perf.Buffer.cs` | every indicator, BufferList style |
| `StreamIndicators` | `Perf.Stream.cs` | every indicator, StreamHub style |
| `Utility` | `Perf.Utility.cs` | shared conversion/utility hot paths |
| `UtilityNullMath` | `Perf.Utility.NullMath.cs` | null-math helpers |
| `UtilityStdDev` | `Perf.Utility.StdDev.cs` | standard-deviation helper |

The list lives in two places that must stay in sync: the no-arg run in
`Program.cs` and `BASELINE_CLASSES` in `perf.sh`.

### Not baselined (diagnostics)

Useful but intentionally **not** committed as baselines. Run ad-hoc with
`--filter`:

- `StyleComparison` (`Perf.StyleComparison.cs`) — cross-style ratio view; overlaps
  the core three suites, so it adds no new regression signal.
- `StreamExternal` (`Perf.StreamExternal.cs`) — EMA series-vs-stream microcheck.
- `ManualTestDirect` (`Perf.ManualTestDirect.cs`) — large-N spot harness (below).

## Manual / large-N spot harness

`ManualTestDirect` validates a single indicator at large bar counts without the
full catalog overhead. It is separate from `perf.sh spot` (which compares the real
suites to baselines); `ManualTestDirect` has no baseline.

```bash
cd tools/performance

# 500k bars for EMA across enabled styles
PERF_TEST_KEYWORD=ema PERF_TEST_PERIODS=500000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"

# Force the steady-state pruning path (cap < periods)
PERF_TEST_KEYWORD=adl PERF_TEST_PERIODS=500000 PERF_TEST_CAP=100000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"
```

## Raw BenchmarkDotNet runs

For exploration only (not baseline comparison). Always `-c Release`; pass BDN args
after `--`:

```bash
cd tools/performance

dotnet run -c Release                                # full baseline suite
dotnet run -c Release -- --filter "*SeriesIndicators*"  # one suite
dotnet run -c Release -- --filter "*.ToEmaBatch"     # one method
```

Artifacts land in `BenchmarkDotNet.Artifacts/results/`:

- `Performance.*-report-full.json` — machine-readable (regression input)
- `Performance.*-report-github.md` — human-readable tables

## Regression detection details

`perf.sh evaluate` and `perf.sh spot` call `detect-regressions.sh`, which pairs
each `*-report-full.json` in `BenchmarkDotNet.Artifacts/results/` with the
same-named file in `baselines/` and compares per method. Only suites present in
the results are compared, so a spot run compares just what it ran.

Run it directly if you already have results (requires `jq`):

```bash
# Directory mode (default): pair all current results with baselines
bash tools/performance/detect-regressions.sh

# Custom threshold (default 10%)
bash tools/performance/detect-regressions.sh --threshold 15

# Explicit single-suite comparison
bash tools/performance/detect-regressions.sh \
  --baseline-file baselines/Performance.StreamIndicators-report-full.json \
  --current-file  BenchmarkDotNet.Artifacts/results/Performance.StreamIndicators-report-full.json
```

Exit codes: `0` no regressions, `1` regressions found, `2` usage/IO error.

## VS Code tasks

- **Perf: Spot check (indicator vs baseline)** → `perf.sh spot` (prompts indicator + style)
- **Perf: Evaluate against baselines** → `perf.sh evaluate`
- **Perf: Reset baselines** → `perf.sh reset`

## CI workflows

All are `workflow_dispatch` (manual) and informational only. **Do not gate merges
on absolute CI timings** — baselines are captured on a developer machine and CI
runners differ, so cross-machine comparisons are noisy.

- `test-performance.yml` — full baseline suite
- `test-performance-comparison.yml` — `StyleComparison` diagnostic
- `test-performance-manual.yml` — targeted `ManualTestDirect` for one indicator

## Best practices

- Use `spot` in the dev loop; use `reset` only for intentional, verified perf work.
- Never add tuning options to baseline/evaluate/spot runs — it breaks comparability.
- Keep baseline refreshes paired with the perf-shifting change that motivated them.

## References

- [Baselines README](baselines/README.md) — file conventions
- [BenchmarkDotNet documentation](https://benchmarkdotnet.org/)
