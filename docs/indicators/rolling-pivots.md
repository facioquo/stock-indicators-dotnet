---
title: Rolling Pivot Points
description: Rolling Pivot Points is a modern update to traditional fixed calendar window Pivot Points.  It depicts support and resistance levels, based on a defined rolling window and offset.
---

# Rolling Pivot Points

Created by Dave Skender, Rolling Pivot Points is a modern update to traditional fixed calendar window <a href="/indicators/pivot-points/">Pivot Points</a>.  It depicts support and resistance levels, based on a defined _rolling_ window and offset.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/274 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="RollingPivots" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<RollingPivotsResult> results =
  bars.ToRollingPivots(windowPeriods, offsetPeriods, pointType);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `windowPeriods` | _`int`_ | Number of periods (`W`) in the evaluation window.  Must be greater than 0 to calculate; but is typically specified in the 5-20 range. |
| `offsetPeriods` | _`int`_ | Number of periods (`F`) to offset the window from the current period.  Must be greater than or equal to 0 and is typically less than or equal to `W`. |
| `pointType` | PivotPointType | Type of Pivot Point.  Default is `PivotPointType.Standard` |

For example, a window of 8 with an offset of 4 would evaluate bars like: `W W W W W W W W F F  F F C`, where `W` is the window included in the Pivot Point calculation, and `F` is the distance from the current evaluation position `C`.  A `bars` with daily bars using `W/F` values of `20/10` would most closely match the `month` variant of the traditional [Pivot Points](/indicators/pivot-points) indicator.

### Historical price bars requirements

You must have at least `W+F` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

### PivotPointType options

**`PivotPointType.Standard`** - Floor Trading (default)

**`PivotPointType.Camarilla`** - Camarilla

**`PivotPointType.Demark`** - Demark

**`PivotPointType.Fibonacci`** - Fibonacci

**`PivotPointType.Woodie`** - Woodie

## Response

```csharp
IReadOnlyList<RollingPivotsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `W+F-1` periods will have `null` values since there's not enough data to calculate.

::: warning ️🖌️ Repaint warning
Historical results are a function of the rolling window position and will shift as new bars are added.  Each new period causes the window to move forward, recalculating pivot points based on the new window data.
:::

### `RollingPivotsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `R4` | _`double`_ | Resistance level 4 (Camarilla only) |
| `R3` | _`double`_ | Resistance level 3 |
| `R2` | _`double`_ | Resistance level 2 |
| `R1` | _`double`_ | Resistance level 1 |
| `PP` | _`double`_ | Pivot Point |
| `S1` | _`double`_ | Support level 1 |
| `S2` | _`double`_ | Support level 2 |
| `S3` | _`double`_ | Support level 3 |
| `S4` | _`double`_ | Support level 4 (Camarilla only) |

### Utilities

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
RollingPivotsList rollingPivotsList = new(windowPeriods, offsetPeriods, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rollingPivotsList.Add(bar);

// based on `ICollection<RollingPivotsResult>`
IReadOnlyList<RollingPivotsResult> results = rollingPivotsList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
RollingPivotsHub observer = barHub.ToRollingPivotsHub(windowPeriods, offsetPeriods, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<RollingPivotsResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
