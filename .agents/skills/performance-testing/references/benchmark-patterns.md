# Benchmark patterns

## File organization

| File | Purpose |
| ---- | ------- |
| `Perf.Series.cs` | Series-style benchmarks |
| `Perf.Stream.cs` | Stream-style benchmarks |
| `Perf.Buffer.cs` | Buffer-style benchmarks |
| `Perf.StyleComparison.cs` | Cross-style comparisons |
| `Perf.Utility.cs` | Utility function benchmarks |

## Benchmark class structure

```csharp
[MemoryDiagnoser]
[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class SeriesIndicators
{
    private static readonly IReadOnlyList<Bar> q = Data.GetDefault();

    [Benchmark]
    public void ToEmaBatch() => q.ToEma(14);

    [Benchmark]
    public void ToSmaBatch() => q.ToSma(20);

    [Benchmark]
    public void ToRsiBatch() => q.ToRsi(14);
}
```

## Stream benchmark with hub setup

```csharp
[MemoryDiagnoser]
[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class StreamIndicators
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();
    private static readonly BarHub barHub = new();

    [GlobalSetup]
    public void Setup()
    {
        foreach (Bar bar in bars)
            barHub.Add(bar);
    }

    [Benchmark]
    public object EmaHub() => barHub.ToEmaHub(14).Results;

    [Benchmark]
    public object SmaHub() => barHub.ToSmaHub(20).Results;
}
```

## Buffer benchmark with collection initializer

```csharp
[MemoryDiagnoser]
[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class BufferIndicators
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();

    [Benchmark]
    public EmaList EmaList() => new(14) { bars };

    [Benchmark]
    public SmaList SmaList() => new(20) { bars };
}
```

## Style comparison benchmark

```csharp
[MemoryDiagnoser]
[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class EmaStyleComparison
{
    private const int LookbackPeriods = 14;
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();
    private static readonly BarHub barHub = new();

    [GlobalSetup]
    public void Setup()
    {
        foreach (Bar bar in bars)
            barHub.Add(bar);
    }

    [Benchmark(Baseline = true)]
    public IReadOnlyList<EmaResult> Series() => bars.ToEma(LookbackPeriods);

    [Benchmark]
    public IReadOnlyList<EmaResult> Buffer() => new EmaList(LookbackPeriods) { bars };

    [Benchmark]
    public IReadOnlyList<EmaResult> Stream() => barHub.ToEmaHub(LookbackPeriods).Results;
}
```

## Running specific benchmarks

```bash
# Single indicator (Series benchmarks carry a `Batch` suffix; Stream use `Hub`)
dotnet run -c Release -- --filter "*.ToEmaBatch"

# All EMA benchmarks
dotnet run -c Release -- --filter "*Ema*"

# Style category
dotnet run -c Release -- --filter "*Stream*"

# Multiple indicators
dotnet run -c Release -- --filter "*.ToEmaBatch" --filter "*.ToSmaBatch"
```

## Interpreting results

```text
|     Method |     Mean |   Error |  StdDev |   Median | Allocated |
|----------- |---------:|--------:|--------:|---------:|----------:|
|     Series |  25.3 μs | 0.50 μs | 0.47 μs |  25.1 μs |   12.8 KB |
|     Buffer |  28.7 μs | 0.57 μs | 0.64 μs |  28.5 μs |   14.2 KB |
|     Stream |  32.1 μs | 0.63 μs | 0.70 μs |  31.9 μs |   16.1 KB |
```

**Ratio analysis**:

- Buffer/Series: 28.7/25.3 = 1.13x (within 1.2x target ✅)
- Stream/Series: 32.1/25.3 = 1.27x (within 1.5x target ✅)

## Memory profiling

Add `[MemoryDiagnoser]` to see allocations:

- **Allocated**: Total bytes allocated per operation
- Target: Minimize allocations in hot paths
- Watch for: Large object heap allocations (> 85KB)

## Regression detection workflow

Use `perf.sh` (see [benchmarking.md](../../../../tools/performance/benchmarking.md)):

1. Establish baselines after stable, verified perf work: `perf.sh reset`
   (runs the baseline suite and writes both `-report-full.json` and
   `-report-github.md` per suite into `baselines/`).
2. After changes, check with `perf.sh evaluate` (full) or
   `perf.sh spot <name>` (targeted).
3. Investigate any regression above the threshold (default 10%).
