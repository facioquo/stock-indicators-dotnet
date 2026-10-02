---
title: Pivot Points
description: Pivot Points depict classic support and resistance levels, based on prior calendar windows.  You can specify window size (e.g. month, week, day, etc) and any of the traditional Floor Trading, Camarilla, Demark, Fibonacci, and Woodie variants.
---

# Pivot Points

[Pivot Points](https://en.wikipedia.org/wiki/Pivot_point_(technical_analysis)) depict support and resistance levels, based on prior calendar windows.  You can specify window size (e.g. month, week, day, etc) and any of the traditional Floor Trading, Camarilla, Demark, Fibonacci, and Woodie variants. See [Rolling Pivot Points](/indicators/rolling-pivots) for lookback window variant.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/274 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="PivotPoints" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<PivotPointsResult> results =
  bars.ToPivotPoints(windowSize, pointType);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `windowSize` | BarInterval | Size of the lookback window.  Default is `BarInterval.Month` |
| `pointType` | PivotPointType | Type of Pivot Point.  Default is `PivotPointType.Standard` |

### Historical price bars requirements

You must have at least `2` windows of `bars` to cover the warmup periods.  For example, if you specify a `Week` window size, you need at least 14 calendar days of `bars`.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

### BarInterval options (for windowSize)

**`BarInterval.Month`** - Use the prior month's data to calculate current month's Pivot Points

**`BarInterval.Week`** - [..] weekly

**`BarInterval.Day`** - [..] daily.  Commonly used for intraday data.

**`BarInterval.OneHour`** - [..] hourly

### PivotPointType options

**`PivotPointType.Standard`** - Floor Trading (default)

**`PivotPointType.Camarilla`** - Camarilla

**`PivotPointType.Demark`** - Demark

**`PivotPointType.Fibonacci`** - Fibonacci

**`PivotPointType.Woodie`** - Woodie

## Response

```csharp
IReadOnlyList<PivotPointsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first window will have `null` values since there's not enough data to calculate.

::: warning 🚩
The second window may be inaccurate if the first window contains incomplete data.  For example, this can occur if you specify a `Month` window size and only provide 45 calendar days (1.5 months) of `bars`.
:::

::: warning ️🖌️ Repaint warning
The last window is repainted when it does not contain a full window of data.
:::

### `PivotPointsResult`

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
PivotPointsList pivotPointsList = new(windowSize, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => pivotPointsList.Add(bar);

// based on `ICollection<PivotPointsResult>`
IReadOnlyList<PivotPointsResult> results = pivotPointsList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
PivotPointsHub observer = barHub.ToPivotPointsHub(windowSize, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<PivotPointsResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
