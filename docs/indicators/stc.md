---
title: Schaff Trend Cycle
description: Created by Doug Schaff, the Schaff Trend Cycle is a stochastic oscillator view of two converging/diverging exponential moving averages.  In other words, it's a Stochastic Oscillator of Moving Average Convergence / Divergence (MACD).
---

# Schaff Trend Cycle

Created by Doug Schaff, the [Schaff Trend Cycle](https://www.investopedia.com/articles/forex/10/schaff-trend-cycle-indicator.asp) is a stochastic oscillator view of two converging/diverging exponential moving averages.  In other words, it's a Stochastic Oscillator of Moving Average Convergence / Divergence (MACD).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/570 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Stc" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<StcResult> results =
  bars.ToStc(cyclePeriods, fastPeriods, slowPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
StcList stcList = new(cyclePeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => stcList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<StcResult> results = stcList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
StcHub observer = barHub.ToStcHub(cyclePeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<StcResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `cyclePeriods`<br>_`int`_ | Number of periods (`C`) for the Trend Cycle.  Must be greater than 0.  Default is 10. |
| `fastPeriods`<br>_`int`_ | Number of periods (`F`) for the faster moving average.  Must be greater than 0.  Default is 23. |
| `slowPeriods`<br>_`int`_ | Number of periods (`S`) for the slower moving average.  Must be greater than `fastPeriods`.  Default is 50. |

### Historical price bars requirements

You must have at least `2×(S+C)` or `S+C+100` worth of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `S+C+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<StcResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `S+C` slow periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
StcList : IReadOnlyList<StcResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `S+C` slow periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<StcResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `S+C` slow periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `S+C+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `StcResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Stc`<br>_`double?`_ | Schaff Trend Cycle |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToStc(..);
```

Results can be further processed on `Stc` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToStc(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
StcList stcList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    stcList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Stc` further.

```csharp
// example
StcList stcList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    stcList.Add(bar);
    rsiList.Add(stcList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
StcHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToStcHub(..);
```

Subscribe other chain-enabled hubs to it to process `Stc` further.

```csharp
// example
RsiHub observer = barHub
    .ToStcHub(..)
    .ToRsiHub(..);
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

Call these on the list itself, as in `stcList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `stcList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
