# RollbackState patterns

Use this when adding a field that carries state between `ToIndicator` calls, or when a hub test fails Series parity after late arrival, removal, or rebuild.

The base class computes `restoreIndex` as the last `ProviderCache` index before the rollback timestamp (`-1` when none), calls `RollbackState`, then removes later results and replays from `restoreIndex + 1` through `ToIndicator`. Restore state to exactly what it was after processing `ProviderCache[restoreIndex]`, reading only `ProviderCache[0..restoreIndex]`.

## When to override

| State | Override | Reference |
| ----- | -------- | --------- |
| Window buffer (`CircularDoubleBuffer`, `Queue<double>`) | Yes | `DonchianHub`, `SmaHub` |
| Smoothed average (Wilder, EMA kept in a field) | Yes | `RsiHub`, `AdxHub` |
| Previous-value scalar (`_prevClose`) | Yes | `ChandelierHub` |
| Counter or position tracker | Yes, plus `PruneState` | `EpmaHub` |
| Only reads `ProviderCache` and `Cache[i - 1]` | No | `EmaHub`, `GatorHub` |

Prefer reading `Cache[i - 1]` inside `ToIndicator` over storing a copy of the previous result; it removes the need for an override.

## Pattern: window buffer

Refill the last `LookbackPeriods` provider values, inclusive of `restoreIndex` (`DonchianHub`, `SmaHub`):

```csharp
protected override void RollbackState(int restoreIndex)
{
    _highBuffer.Clear();
    _lowBuffer.Clear();

    if (restoreIndex < 0)
    {
        return;
    }

    int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);
    for (int p = startIdx; p <= restoreIndex; p++)
    {
        IBar bar = ProviderCache[p];
        _highBuffer.Add((double)bar.High);
        _lowBuffer.Add((double)bar.Low);
    }
}
```

A second-stage buffer (smoothing over computed values) is prefilled the same way, recomputing each of its last `SmoothPeriods` entries from the provider window; `StochHub` is the reference.

## Pattern: path-dependent smoothing

Wilder and EMA smoothing depend on every value since initialization, so a truncated replay window breaks Series parity. Recover state in two tiers (`RsiHub`, `AdxHub`):

1. Record a snapshot after each item in `ToIndicator`: `_rollback.Add(item.Timestamp, (_avgGain, _avgLoss));` with `RollbackRing<TState>` (`src/Common/StreamHub/RollbackRing.cs`, 32 entries by default).
2. In `RollbackState`, reset fields, then try the ring. On a miss, replay from the first calculable index to `restoreIndex`.

```csharp
private readonly RollbackRing<(double AvgGain, double AvgLoss)> _rollback = new();

protected override void RollbackState(int restoreIndex)
{
    _avgGain = double.NaN;
    _avgLoss = double.NaN;

    if (restoreIndex < LookbackPeriods)
    {
        return;
    }

    if (_rollback.TryGet(
        ProviderCache[restoreIndex].Timestamp,
        out (double AvgGain, double AvgLoss) snapshot))
    {
        _avgGain = snapshot.AvgGain;
        _avgLoss = snapshot.AvgLoss;
        return;
    }

    // full replay: seed at the first calculable index, then smooth through restoreIndex
}
```

The ring makes near-tail rollbacks (live corrections, forming-bar updates) O(1); deeper rollbacks fall back to the O(n) replay. A same-timestamp `Add` replaces the newest snapshot, so repeated forming-bar updates occupy one slot.

## Pattern: previous-value scalar

Read the scalar back from the provider at `restoreIndex` instead of replaying (`ChandelierHub`):

```csharp
_prevClose = restoreIndex >= 0
    ? (double)ProviderCache[restoreIndex].Close
    : double.NaN;
```

## Pattern: position trackers and pruning

`ProviderCache` indexes shift when the cache front is pruned. A hub that tracks absolute positions sets its counters from `restoreIndex` in `RollbackState` (`itemsAdded = restoreIndex + 1`) and accumulates the pruned offset in `PruneState(DateTime)`. `RollbackState` must not reset the pruning offset; `EpmaHub` is the reference.

## Compound hubs

A compound hub replays its own processing over the inner hub's cached results; see [compound hubs](compound-hubs.md#rollbackstate).
