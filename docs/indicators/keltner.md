---
title: Keltner Channels
description: Created by Chester W. Keltner, the Keltner Channels price range overlay is based on an EMA centerline and Average True Range (ATR) band widths.  STARC Bands are the SMA centerline equivalent.
---

# Keltner Channels

Created by Chester W. Keltner, [Keltner Channels](https://en.wikipedia.org/wiki/Keltner_channel) are based on an EMA centerline and ATR band widths.  See also [STARC Bands](/indicators/starc-bands) for an SMA centerline equivalent.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/249 "Community discussion about this indicator")

<StockIndicatorChart indicator="Keltner" />

```csharp
// C# usage syntax
IReadOnlyList<KeltnerResult> results =
  bars.ToKeltner(emaPeriods, multiplier, atrPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `emaPeriods` | _`int`_ | Number of lookback periods (`E`) for the center line moving average.  Must be greater than 1 to calculate.  Default is 20. |
| `multiplier` | _`double`_ | ATR Multiplier. Must be greater than 0.  Default is 2. |
| `atrPeriods` | _`int`_ | Number of lookback periods (`A`) for the Average True Range.  Must be greater than 1 to calculate.  Default is 10. |

### Historical price bars requirements

You must have at least `2×N` or `N+100` periods of `bars`, whichever is more, where `N` is the greater of `E` or `A` periods, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<KeltnerResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `N+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `KeltnerResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `UpperBand` | _`double`_ | Upper band of Keltner Channel |
| `Centerline` | _`double`_ | EMA of price |
| `LowerBand` | _`double`_ | Lower band of Keltner Channel |
| `Width` | _`double`_ | Width as percent of Centerline price.  `(UpperBand-LowerBand)/Centerline` |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Centerline` with other [chained indicators](/guide/chaining).

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

```csharp
// example
var results = bars
    .ToKeltner(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
KeltnerList keltnerList = new(emaPeriods, multiplier, atrPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => keltnerList.Add(bar);

// based on `ICollection<KeltnerResult>`; fills as the handler runs
IReadOnlyList<KeltnerResult> results = keltnerList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
KeltnerHub observer = barHub.ToKeltnerHub(emaPeriods, multiplier, atrPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<KeltnerResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
