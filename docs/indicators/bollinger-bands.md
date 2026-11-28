---
title: Bollinger Bands®
description: Created by John Bollinger, the Bollinger Bands price channels depict volatility as upper and lower lines set a number of standard deviations from a moving average of price.  Bollinger Bands&#174; is a registered trademark of John A. Bollinger.
---

# Bollinger Bands®

Created by John Bollinger, [Bollinger Bands](https://en.wikipedia.org/wiki/Bollinger_Bands) price channels depict volatility as upper and lower lines set a number of standard deviations from a moving average of price.  Bollinger Bands® is a registered trademark of John A. Bollinger.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/267 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="BollingerBands" with="BollingerBandsPctB" />

<ChartKey indicator="BollingerBands" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<BollingerBandsResult> results =
  bars.ToBollingerBands(lookbackPeriods, standardDeviations);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
BollingerBandsList bbList = new(lookbackPeriods, standardDeviations);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => bbList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<BollingerBandsResult> results = bbList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
BollingerBandsHub observer = barHub.ToBollingerBandsHub(lookbackPeriods, standardDeviations);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<BollingerBandsResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the center line moving average.  Must be greater than 1 to calculate; however we suggest a larger period for statistically appropriate sample size.  Default is 20. |
| `standardDeviations`<br>_`double`_ | Width of bands.  Standard deviations (`D`) from the moving average.  Must be greater than 0.  Default is 2. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<BollingerBandsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
BollingerBandsList : IReadOnlyList<BollingerBandsResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<BollingerBandsResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

### `BollingerBandsResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Sma`<br>_`double?`_ | Simple moving average (SMA) of price (center line) |
| `UpperBand`<br>_`double?`_ | Upper line is `D` standard deviations above the SMA |
| `LowerBand`<br>_`double?`_ | Lower line is `D` standard deviations below the SMA |
| `PercentB`<br>_`double?`_ | `%B` is the location within the bands.  `(Price-LowerBand)/(UpperBand-LowerBand)` |
| `ZScore`<br>_`double?`_ | Z-score of current price (number of standard deviations from mean) |
| `Width`<br>_`double?`_ | Width as percent of SMA price.  `(UpperBand-LowerBand)/Sma` |

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
    .ToBollingerBands(..);
```

Results can be further processed on `PercentB` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToBollingerBands(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
BollingerBandsList bbList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    bbList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `PercentB` further.

```csharp
// example
BollingerBandsList bbList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    bbList.Add(bar);
    rsiList.Add(bbList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
BollingerBandsHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToBollingerBandsHub(..);
```

Subscribe other chain-enabled hubs to it to process `PercentB` further.

```csharp
// example
RsiHub observer = barHub
    .ToBollingerBandsHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
