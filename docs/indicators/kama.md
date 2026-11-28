---
title: Kaufman's Adaptive Moving Average (KAMA)
description: Created by Perry Kaufman, KAMA is an volatility adaptive (adjusted) moving average of price over configurable lookback periods.
---

# Kaufman's Adaptive Moving Average (KAMA)

Created by Perry Kaufman, [KAMA](https://www.google.com/search?q=Kaufman+Adaptive+Moving+Average+(KAMA)) is an volatility adaptive moving average of price over configurable lookback periods.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/210 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Kama" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<KamaResult> results =
  bars.ToKama(erPeriods, fastPeriods, slowPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
KamaList kamaList = new(erPeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => kamaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<KamaResult> results = kamaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
KamaHub observer = barHub.ToKamaHub(erPeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<KamaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `erPeriods`<br>_`int`_ | Number of Efficiency Ratio (volatility) periods (`E`).  Must be greater than 0.  Default is 10. |
| `fastPeriods`<br>_`int`_ | Number of Fast EMA periods.  Must be greater than 0.  Default is 2. |
| `slowPeriods`<br>_`int`_ | Number of Slow EMA periods.  Must be greater than `fastPeriods`.  Default is 30. |

### Historical price bars requirements

You must have at least `6×E` or `E+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `10×E` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<KamaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `E-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
KamaList : IReadOnlyList<KamaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `E-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<KamaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `E-1` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `10×E` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `KamaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Er`<br>_`double?`_ | Efficiency Ratio is the fractal efficiency of price changes |
| `Kama`<br>_`double?`_ | Kaufman's adaptive moving average |

More about Efficiency Ratio: ER fluctuates between 0 and 1, but these extremes are the exception, not the norm. ER would be 1 if prices moved up or down consistently over the `erPeriods` window. ER would be zero if prices are unchanged over the `erPeriods` window.

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToKama(..);
```

Results can be further processed on `Kama` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToKama(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
KamaList kamaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    kamaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Kama` further.

```csharp
// example
KamaList kamaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    kamaList.Add(bar);
    rsiList.Add(kamaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
KamaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToKamaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Kama` further.

```csharp
// example
RsiHub observer = barHub
    .ToKamaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
