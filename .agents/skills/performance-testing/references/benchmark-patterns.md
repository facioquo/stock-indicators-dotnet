# Benchmark patterns

Use this when adding or editing a `[Benchmark]` method or a benchmark class in `tools/performance/`.

## Per-style suites

A new indicator adds one line to each style suite it supports, in alphabetical order, matching its neighbors:

| File | Class | Method pattern |
| ---- | ----- | -------------- |
| `Perf.Series.cs` | `SeriesIndicators` | `[Benchmark] public void ToEmaBatch() => q.ToEma(20);` |
| `Perf.Buffer.cs` | `BufferIndicators` | `[Benchmark] public EmaList EmaList() => q.ToEmaList(20);` |
| `Perf.Stream.cs` | `StreamIndicators` | `[Benchmark] public object EmaHub() => barHub.ToEmaHub(20).Results;` |
| `Perf.StyleComparison.cs` | `StyleComparison` | `EmaSeries()`, `EmaBuffer()`, `EmaStream()` triplet, below |

- Series arguments equal the catalog default parameter values; Buffer and Stream arguments equal the Series ones.
- Use the class fields: `q` (`Data.GetDefault()`, 502 bars), `n` (14), and, where declared, `o` (`Data.GetCompare()`, for two-series indicators such as Beta and Correlation).
- `StreamIndicators` measures observer cost only: `GlobalSetup` prepopulates `barHub` and `barHubOther`, so each Stream benchmark builds a hub over an already-filled provider.
- `detect-regressions.sh` matches results to baselines by class and method name and silently skips a method with no baseline entry, so a new or renamed benchmark goes unchecked until the next `perf.sh reset`.

## Style comparison triplet

```csharp
[BenchmarkCategory("Ema")]
[Benchmark(Baseline = true)]
public IReadOnlyList<EmaResult> EmaSeries() => bars.ToEma(20);

[BenchmarkCategory("Ema")]
[Benchmark]
public IReadOnlyList<EmaResult> EmaBuffer() => bars.ToEmaList(20);

[BenchmarkCategory("Ema")]
[Benchmark]
public IReadOnlyList<EmaResult> EmaStream() => barHub.ToEmaHub(20).Results;
```

The class groups by category, so the report shows each Buffer and Stream ratio against its own Series baseline; compare those ratios with the [performance targets](../SKILL.md#targets).

## Configuration

`Configurations/DefaultConfig.cs` applies to every run: GitHub Markdown and full JSON exporters, the memory diagnoser with GC columns, and method-name ordering. Do not add `[MemoryDiagnoser]` or exporters to a class.

Job attributes sit on each class and define comparability with the committed baselines: `[ShortRunJob, WarmupCount(5), IterationCount(5)]` on the per-style suites, `[ShortRunJob]` alone on `Utility`, `StreamObserver`, and the diagnostics, and `[Config(typeof(MicrotestConfig))]` on the `UtilityNullMath` and `UtilityStdDev` microbenchmarks. Changing a baselined suite's job attributes invalidates its baselines and requires `perf.sh reset`.

## Filters

Run from `tools/performance`. BenchmarkDotNet matches `--filter` globs against `Namespace.Class.Method`:

```bash
dotnet run -c Release -- --filter "*.ToEmaBatch"                       # one Series method
dotnet run -c Release -- --filter "Performance.BufferIndicators.*Ema*"  # one indicator, one style
dotnet run -c Release -- --filter "*.ToEmaBatch" "*.ToSmaBatch"         # several patterns
```
