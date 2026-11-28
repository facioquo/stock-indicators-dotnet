---
title: Average True Range (ATR) / True Range (TR)
description: Created by J. Welles Wilder, True Range and Average True Range is a measure of volatility that captures gaps and limits between periods.
---

# Average True Range (ATR) / True Range (TR)

Created by J. Welles Wilder, True Range and [Average True Range](https://en.wikipedia.org/wiki/Average_true_range) are measures of volatility that capture gaps and limits between periods. See [True Range (TR)](/indicators/tr) for dedicated TR documentation, including streaming support.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/269 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Atr" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AtrResult> results =
  bars.ToAtr(lookbackPeriods);

// ATR with custom moving average
IReadOnlyList<SmmaResult> results =
  bars.ToTr().ToSmma(lookbackPeriods);

// raw True Range (TR) only
IReadOnlyList<TrResult> results =
  bar.ToTr();
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AtrList atrList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => atrList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AtrResult> results = atrList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AtrHub observer = barHub.ToAtrHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AtrResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) to consider.  Must be greater than 1.  Default is 14. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<AtrResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for ATR since there's not enough data to calculate.
:::

::: buffer

```csharp
AtrList : IReadOnlyList<AtrResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` values for ATR since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<AtrResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` values for ATR since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `AtrResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Tr`<br>_`double?`_ | True Range for current period |
| `Atr`<br>_`double?`_ | Average True Range |
| `Atrp`<br>_`double?`_ | Average True Range Percent is `(ATR/Price)*100`.  This normalizes so it can be compared to other stocks. |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Atrp` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAtr(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Atrp` further.

```csharp
// example
AtrList atrList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    atrList.Add(bar);
    slopeList.Add(atrList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Atrp` further.

```csharp
// example
SlopeHub observer = barHub
    .ToAtrHub(..)
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `atrList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `atrList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
