---
title: Pivot Points
description: Pivot Points depict classic support and resistance levels, based on prior calendar windows.  You can specify window size (e.g. month, week, day, etc) and any of the traditional Floor Trading, Camarilla, Demark, Fibonacci, and Woodie variants.
---

# Pivot Points

[Pivot Points](https://en.wikipedia.org/wiki/Pivot_point_(technical_analysis)) depict support and resistance levels, based on prior calendar windows.  You can specify window size (e.g. month, week, day, etc) and any of the traditional Floor Trading, Camarilla, Demark, Fibonacci, and Woodie variants. See [Rolling Pivot Points](/indicators/rolling-pivots) for lookback window variant.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/274 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="PivotPoints" />

<ChartKey indicator="PivotPoints" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<PivotPointsResult> results =
  bars.ToPivotPoints(windowSize, pointType);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
PivotPointsList pivotPointsList = new(windowSize, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => pivotPointsList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<PivotPointsResult> results = pivotPointsList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
PivotPointsHub observer = barHub.ToPivotPointsHub(windowSize, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<PivotPointsResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `windowSize`<br>_`BarInterval`_ | Size of the lookback window.  Default is `BarInterval.Month` |
| `pointType`<br>_`PivotPointType`_ | Type of Pivot Point.  Default is `PivotPointType.Standard` |

### Historical price bars requirements

You must have at least `2` windows of `bars` to cover the warmup periods.  For example, if you specify a `Week` window size, you need at least 14 calendar days of `bars`.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

### BarInterval options (for windowSize)

**`BarInterval.Month`** - Use the prior month's data to calculate current month's Pivot Points

**`BarInterval.Week`** - Use the prior week's data to calculate current week's Pivot Points

**`BarInterval.Day`** - Use the prior day's data to calculate current day's Pivot Points.  Commonly used for intraday data.

**`BarInterval.OneHour`** - Use the prior hour's data to calculate current hour's Pivot Points

### PivotPointType options

**`PivotPointType.Standard`** - Floor Trading (default)

**`PivotPointType.Camarilla`** - Camarilla

**`PivotPointType.Demark`** - Demark

**`PivotPointType.Fibonacci`** - Fibonacci

**`PivotPointType.Woodie`** - Woodie

## Response

::: series

```csharp
IReadOnlyList<PivotPointsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first window will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
PivotPointsList : IReadOnlyList<PivotPointsResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first window will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<PivotPointsResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first window will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 Incomplete first window
The second window may be inaccurate if the first window contains incomplete data.  For example, this can occur if you specify a `Month` window size and only provide 45 calendar days (1.5 months) of `bars`.
:::

::: warning 🖌️ Repaint warning
The last window is repainted when it does not contain a full window of data.
:::

### `PivotPointsResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `R4`<br>_`double?`_ | Resistance level 4 (Camarilla only) |
| `R3`<br>_`double?`_ | Resistance level 3 |
| `R2`<br>_`double?`_ | Resistance level 2 |
| `R1`<br>_`double?`_ | Resistance level 1 |
| `PP`<br>_`double?`_ | Pivot Point |
| `S1`<br>_`double?`_ | Support level 1 |
| `S2`<br>_`double?`_ | Support level 2 |
| `S3`<br>_`double?`_ | Support level 3 |
| `S4`<br>_`double?`_ | Support level 4 (Camarilla only) |

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Utilities

::: series

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on the list itself, as in `pivotPointsList.Find(lookupDate)`.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on `observer.Results`, as in `observer.Results.Find(lookupDate)`.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
