---
title: Pivots
description: Pivots is an extended customizable version of Williams Fractal that includes identification of Higher High, Lower Low, Higher Low, and Lower Low trends between pivots in a lookback window.
---

# Pivots {#pivots-indicator}

Pivots is an extended customizable version of [Williams Fractal](/indicators/fractal) that includes identification of Higher High, Lower Low, Higher Low, and Lower Low trends between pivots in a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/436 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Pivots" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<PivotsResult> results =
  bars.ToPivots(leftSpan, rightSpan, maxTrendPeriods, endType);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `leftSpan` | _`int`_ | Left evaluation window span width (`L`).  Must be at least 2.  Default is 2. |
| `rightSpan` | _`int`_ | Right evaluation window span width (`R`).  Must be at least 2.  Default is 2. |
| `maxTrendPeriods` | _`int`_ | Maximum lookback periods (`N`) for drawing trend lines between pivot points.  When pivot points are further apart than this value, the trend line tracking resets.  Must be greater than `leftSpan`.  Default is 20. |
| `endType` | _`EndType`_ | Determines whether `Close` or `High/Low` are used to find end points.  Default is `EndType.HighLow`. |

The total evaluation window size is `L+R+1`.

::: note
The `maxTrendPeriods` parameter controls the lookback window for trend line calculations, not the number of results returned.
:::

### Historical price bars requirements

You must have at least `L+R+1` periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

```csharp
IReadOnlyList<PivotsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `L` and last `R` periods in `bars` are unable to be calculated since there's not enough prior/following data.

::: warning 🖌️ Repaint warning
This price pattern looks forward and backward in the historical price bars so it will never identify a pivot in the last `R` periods of `bars`.  Pivots are retroactively identified.
:::

### `PivotsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `HighPoint` | _`decimal`_ | Value indicates a **high** point; otherwise `null` is returned. |
| `LowPoint` | _`decimal`_ | Value indicates a **low** point; otherwise `null` is returned. |
| `HighLine` | _`decimal`_ | Drawn line between two high points in the `maxTrendPeriods` |
| `LowLine` | _`decimal`_ | Drawn line between two low points in the `maxTrendPeriods` |
| `HighTrend` | _`PivotTrend`_ | Enum that represents higher high or lower high.  See [PivotTrend values](#pivottrend-values) below. |
| `LowTrend` | _`PivotTrend`_ | Enum that represents higher low or lower low.  See [PivotTrend values](#pivottrend-values) below. |

#### PivotTrend values

**`PivotTrend.Hh`** - Higher high

**`PivotTrend.Lh`** - Lower high

**`PivotTrend.Hl`** - Higher low

**`PivotTrend.Ll`** - Lower low

#### Filtering results

Since this method returns one result per input bar (with `null` values where no pivot exists), you'll often want to filter results for specific use cases:

```csharp
// get only records with pivot points
var pivotsOnly = results.Condense();

// get only records with trend lines
var trendsOnly = results
    .Where(x => x.HighTrend != null || x.LowTrend != null);

// get only recent N periods
var recentPivots = results.TakeLast(period);

// get only high pivot points with Higher High trend
var higherHighs = results
    .Where(x => x.HighPoint != null && x.HighTrend == PivotTrend.Hh);

// combine filters: recent periods with trends
var recentTrends = results
    .Where(x => x.HighTrend != null || x.LowTrend != null)
    .TakeLast(period);
```

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
PivotsList pivotsList = new(leftSpan, rightSpan, maxTrendPeriods, endType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => pivotsList.Add(bar);

// based on `ICollection<PivotsResult>`
IReadOnlyList<PivotsResult> results = pivotsList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
PivotsHub observer = barHub.ToPivotsHub(leftSpan, rightSpan, maxTrendPeriods, endType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<PivotsResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
