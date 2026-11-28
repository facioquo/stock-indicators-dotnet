---
title: Rolling Pivot Points
description: Rolling Pivot Points is a modern update to traditional fixed calendar window Pivot Points.  It depicts support and resistance levels, based on a defined rolling window and offset.
---

# Rolling Pivot Points

Created by Dave Skender, Rolling Pivot Points is a modern update to traditional fixed calendar window <a href="/indicators/pivot-points/">Pivot Points</a>.  It depicts support and resistance levels, based on a defined _rolling_ window and offset.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/274 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="RollingPivots" />

<ChartKey indicator="RollingPivots" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<RollingPivotsResult> results =
  bars.ToRollingPivots(windowPeriods, offsetPeriods, pointType);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
RollingPivotsList rollingPivotsList = new(windowPeriods, offsetPeriods, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rollingPivotsList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<RollingPivotsResult> results = rollingPivotsList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
RollingPivotsHub observer = barHub.ToRollingPivotsHub(windowPeriods, offsetPeriods, pointType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<RollingPivotsResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `windowPeriods`<br>_`int`_ | Number of periods (`W`) in the evaluation window.  Must be greater than 0 to calculate; but is typically specified in the 5-20 range.  Default is 20. |
| `offsetPeriods`<br>_`int`_ | Number of periods (`F`) to offset the window from the current period.  Must be greater than or equal to 0 and is typically less than or equal to `W`.  Default is 0. |
| `pointType`<br>_`PivotPointType`_ | Type of Pivot Point.  Default is `PivotPointType.Standard` |

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

::: series

```csharp
IReadOnlyList<RollingPivotsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `W+F-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
RollingPivotsList : IReadOnlyList<RollingPivotsResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `W+F-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<RollingPivotsResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `W+F-1` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🖌️ Repaint warning
Historical results are a function of the rolling window position and will shift as new bars are added.  Each new period causes the window to move forward, recalculating pivot points based on the new window data.
:::

### `RollingPivotsResult`

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

Call it on the list itself, as in `rollingPivotsList.Find(lookupDate)`.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on `observer.Results`, as in `observer.Results.Find(lookupDate)`.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
