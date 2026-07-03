---
name: performance-testing
description: Benchmark indicator performance with BenchmarkDotNet. Use for Series/Buffer/Stream benchmarks, regression detection, and optimization patterns. Target 1.5x Series for StreamHub, 1.2x for BufferList.
---

# Performance testing

## Running benchmarks

Prefer `perf.sh` — one entry point for the three common workflows (requires `jq`
for evaluate/spot). See the canonical guide:
[tools/performance/benchmarking.md](../../../tools/performance/benchmarking.md).

```bash
# 1. Spot check: one indicator vs baseline (fast; recommended dev-loop check)
bash tools/performance/perf.sh spot Ema
bash tools/performance/perf.sh spot Adx Stream        # single style

# 2. Evaluate: full suite, report regressions vs baselines (~1 hour)
bash tools/performance/perf.sh evaluate

# 3. Reset baselines: full suite, replace committed baselines (~1 hour)
bash tools/performance/perf.sh reset
```

Do not add tuning options (`--job`, thresholds, etc.) to these — plain runs stay
comparable to the committed baselines.

Baseline set = `SeriesIndicators`, `BufferIndicators`, `StreamIndicators`,
`Utility`, `UtilityNullMath`, `UtilityStdDev` (same as no-arg `dotnet run`).

Raw BenchmarkDotNet control (from `tools/performance`):

```bash
dotnet run -c Release                                   # full baseline suite
dotnet run -c Release -- --filter "*StreamIndicators*"  # one suite
dotnet run -c Release -- --filter "*.EmaHub"            # one method

# Large-N direct harness (diagnostic; not baselined)
PERF_TEST_KEYWORD=ema PERF_TEST_PERIODS=500000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"
# Exercise pruning path (Cap < Periods)
PERF_TEST_KEYWORD=adl PERF_TEST_PERIODS=500000 PERF_TEST_CAP=100000 dotnet run -c Release -- --filter "Performance.ManualTestDirect*"
```

## Adding benchmarks

### Series pattern

```csharp
[Benchmark]
public void ToMyIndicator() => bars.ToMyIndicator(14);
```

### Stream pattern

```csharp
[Benchmark]
public object MyIndicatorHub() => barHub.ToMyIndicatorHub(14).Results;
```

### Buffer pattern

```csharp
[Benchmark]
public MyIndicatorList MyIndicatorList() => new(14) { bars };
```

### Style comparison

```csharp
[Benchmark]
public IReadOnlyList<MyResult> MyIndicatorSeries() => bars.ToMyIndicator(14);

[Benchmark]
public IReadOnlyList<MyResult> MyIndicatorBuffer() => bars.ToMyIndicatorList(14);

[Benchmark]
public IReadOnlyList<MyResult> MyIndicatorStream() => barHub.ToMyIndicator(14).Results;
```

## Performance targets

**Note**: These are optimization goals for future v3.1+ effort. Current implementations vary by indicator family.

| Style | Target vs Series | Use Case |
| ----- | ---------------- | -------- |
| Series | Baseline | Batch processing |
| BufferList | ≤ 1.2x | Incremental data |
| StreamHub | ≤ 1.5x | Real-time feeds |

## Expected execution times (502 periods)

**Note**: These are optimization targets. Actual execution times vary by indicator complexity and current implementation.

| Complexity | Time | Examples |
| ---------- | ---- | -------- |
| Fast | < 30μs | SMA, EMA, WMA, RSI |
| Medium | 30-60μs | MACD, Bollinger Bands, ATR |
| Complex | 60-100μs | HMA, ADX, Stochastic |
| Advanced | 100-200μs+ | Ichimoku, Hurst |

## Regression detection

`perf.sh evaluate` / `spot` call `detect-regressions.sh`, which pairs each current
result with its same-named baseline and compares per method (requires `jq`).

```bash
# Full evaluate (run + compare)
bash tools/performance/perf.sh evaluate

# Compare existing results only (no run)
bash tools/performance/detect-regressions.sh --threshold 15
```

Exit codes: `0` no regressions, `1` regressions found, `2` usage/IO error.

## Creating / refreshing baselines

```bash
# Runs the full baseline suite and copies results into baselines/
bash tools/performance/perf.sh reset
```

## Required optimization patterns

- Minimize allocations in hot paths
- Avoid LINQ in performance-critical loops
- Use `Span<T>` for zero-copy operations
- Cache calculations when possible
- Test with realistic data sizes (502 periods)

## Prohibited patterns

- Excessive LINQ in hot paths
- Boxing/unboxing of value types
- Unnecessary string allocations
- Redundant calculations in loops
- Poor cache locality

See [references/benchmark-patterns.md](references/benchmark-patterns.md) for detailed patterns.
