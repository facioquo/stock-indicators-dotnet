---
title: Keltner Channels
description: Created by Chester W. Keltner, the Keltner Channels price range overlay is based on an EMA centerline and Average True Range (ATR) band widths.  STARC Bands are the SMA centerline equivalent.
---

# Keltner Channels

Created by Chester W. Keltner, [Keltner Channels](https://en.wikipedia.org/wiki/Keltner_channel) are based on an EMA centerline and ATR band widths.  See also [STARC Bands](/indicators/starc-bands) for an SMA centerline equivalent.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/249 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Keltner" />

<ChartKey indicator="Keltner" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<KeltnerResult> results =
  bars.ToKeltner(emaPeriods, multiplier, atrPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
KeltnerList keltnerList = new(emaPeriods, multiplier, atrPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => keltnerList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<KeltnerResult> results = keltnerList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
KeltnerHub observer = barHub.ToKeltnerHub(emaPeriods, multiplier, atrPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<KeltnerResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `emaPeriods`<br>_`int`_ | Number of lookback periods (`E`) for the center line moving average.  Must be greater than 1 to calculate.  Default is 20. |
| `multiplier`<br>_`double`_ | ATR Multiplier. Must be greater than 0.  Default is 2. |
| `atrPeriods`<br>_`int`_ | Number of lookback periods (`A`) for the Average True Range.  Must be greater than 1 to calculate.  Default is 10. |

### Historical price bars requirements

You must have at least `2×N` or `N+100` periods of `bars`, whichever is more, where `N` is the greater of `E` or `A` periods, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<KeltnerResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
KeltnerList : IReadOnlyList<KeltnerResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<KeltnerResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `KeltnerResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `UpperBand`<br>_`double?`_ | Upper band of Keltner Channel |
| `Centerline`<br>_`double?`_ | EMA of price |
| `LowerBand`<br>_`double?`_ | Lower band of Keltner Channel |
| `Width`<br>_`double?`_ | Width as percent of Centerline price.  `(UpperBand-LowerBand)/Centerline` |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.
