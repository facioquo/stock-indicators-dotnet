---
title: Kaufman's Adaptive Moving Average (KAMA)
description: Created by Perry Kaufman, KAMA is an volatility adaptive (adjusted) moving average of price over configurable lookback periods.
---

# Kaufman's Adaptive Moving Average (KAMA)

Created by Perry Kaufman, [KAMA](https://www.google.com/search?q=Kaufman+Adaptive+Moving+Average+(KAMA)) is an volatility adaptive moving average of price over configurable lookback periods.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/210 "Community discussion about this indicator")

<StockIndicatorChart indicator="Kama" />

```csharp
// C# usage syntax
IReadOnlyList<KamaResult> results =
  bars.ToKama(erPeriods, fastPeriods, slowPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `erPeriods` | _`int`_ | Number of Efficiency Ratio (volatility) periods (`E`).  Must be greater than 0.  Default is 10. |
| `fastPeriods` | _`int`_ | Number of Fast EMA periods.  Must be greater than 0.  Default is 2. |
| `slowPeriods` | _`int`_ | Number of Slow EMA periods.  Must be greater than `fastPeriods`.  Default is 30. |

### Historical price bars requirements

You must have at least `6×E` or `E+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `10×E` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<KamaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `E-1` periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `10×E` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `KamaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Er` | _`double`_ | Efficiency Ratio is the fractal efficiency of price changes |
| `Kama` | _`double`_ | Kaufman's adaptive moving average |

More about Efficiency Ratio: ER fluctuates between 0 and 1, but these extremes are the exception, not the norm. ER would be 1 if prices moved up or down consistently over the `erPeriods` window. ER would be zero if prices are unchanged over the `erPeriods` window.

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

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

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
KamaList kamaList = new(erPeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => kamaList.Add(bar);

// based on `ICollection<KamaResult>`; fills as the handler runs
IReadOnlyList<KamaResult> results = kamaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
KamaHub observer = barHub.ToKamaHub(erPeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<KamaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
