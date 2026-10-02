# Performance patterns

Use this when writing or optimizing a hub's `ToIndicator`, or when a Stream benchmark shows a hub far slower than its Series counterpart. The performance-testing skill owns the overhead targets and how to write and run benchmarks.

Spend optimization effort on per-tick work that grows with history length. Observer notification, locking, and cache management are a fixed per-tick cost of the framework, not the indicator's to remove.

## Anti-pattern: recomputing history

Re-running the Series calculation on a growing subset makes each tick O(n), so the stream is O(n²):

```csharp
// WRONG
List<IReusable> subset = [];
for (int k = 0; k <= i; k++)
{
    subset.Add(ProviderCache[k]);
}
IReadOnlyList<RsiResult> seriesResults = subset.ToRsi(LookbackPeriods);
```

Use the previous result from `Cache` instead (`EmaHub`):

```csharp
double ema = i >= LookbackPeriods - 1
    ? Cache[i - 1].Ema is not null
        ? Ema.Increment(K, Cache[i - 1].Value, item.Value)  // O(1)
        : Sma.Increment(ProviderCache, LookbackPeriods, i)   // O(lookback) re/initialize
    : double.NaN;
```

## Pattern: incremental state in fields

When the previous result does not carry enough to continue, keep the running quantity in a field, update it once per tick, and restore it in `RollbackState`. `RsiHub` keeps Wilder averages inline:

```csharp
_avgGain = ((_avgGain * (LookbackPeriods - 1)) + gain) / LookbackPeriods;
_avgLoss = ((_avgLoss * (LookbackPeriods - 1)) + loss) / LookbackPeriods;
```

Compute constants such as smoothing factors once in the constructor (`K` in `EmaHub`).

## Pattern: bounded windows

For highest/lowest over a lookback window, keep the window in a `CircularDoubleBuffer` (`src/Common/StreamHub/CircularDoubleBuffer.cs`) instead of indexing back through `ProviderCache`. `Add` is O(1) with no allocation; `GetMax`/`GetMin` scan the filled entries, O(window), which is fast for small windows. `DonchianHub`, `WilliamsRHub`, `StochRsiHub`, and `IchimokuHub` use it.

- Declare the field without `readonly`: the buffer is a mutable struct.
- `GetMax`/`GetMin` return `NaN` while empty.

For a rolling sum or average, keep a `Queue<double>` updated with the internal `Update`/`UpdateWithDequeue` extensions (`src/Common/BufferLists/BufferListUtilities.cs`); `SmaHub` is the reference.

## Checklist

- No Series method call and no loop from index 0 inside `ToIndicator`.
- Any loop in `ToIndicator` is bounded by a lookback length, and runs only at initialization or re-initialization where possible.
- Constants are computed in the constructor.
- No per-tick allocation of lists, arrays, or LINQ pipelines.
