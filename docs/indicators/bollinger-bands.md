---
title: Bollinger Bands®
description: Created by John Bollinger, the Bollinger Bands price channels depict volatility as standard deviation boundary line range from a moving average of price.  Bollinger Bands&#174; is a registered trademark of John A. Bollinger.
---

# Bollinger Bands®

Created by John Bollinger, [Bollinger Bands](https://en.wikipedia.org/wiki/Bollinger_Bands) price channels depict volatility as standard deviation boundary line range from a moving average of price.  Bollinger Bands® is a registered trademark of John A. Bollinger.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/267 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="BollingerBands" with="BollingerBandsPctB" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<BollingerBandsResult> results =
  bars.ToBollingerBands(lookbackPeriods, standardDeviations);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) for the center line moving average.  Must be greater than 1 to calculate; however we suggest a larger period for statistically appropriate sample size.  Default is 20. |
| `standardDeviations` | _`double`_ | Width of bands.  Standard deviations (`D`) from the moving average.  Must be greater than 0.  Default is 2. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<BollingerBandsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `BollingerBandsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sma` | _`double`_ | Simple moving average (SMA) of price (center line) |
| `UpperBand` | _`double`_ | Upper line is `D` standard deviations above the SMA |
| `LowerBand` | _`double`_ | Lower line is `D` standard deviations below the SMA |
| `PercentB` | _`double`_ | `%B` is the location within the bands.  `(Price-LowerBand)/(UpperBand-LowerBand)` |
| `ZScore` | _`double`_ | Z-score of current price (number of standard deviations from mean) |
| `Width` | _`double`_ | Width as percent of SMA price.  `(UpperBand-LowerBand)/Sma` |

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

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
BollingerBandsList bbList = new(lookbackPeriods, standardDeviations);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => bbList.Add(bar);

// based on `ICollection<BollingerBandsResult>`; fills as the handler runs
IReadOnlyList<BollingerBandsResult> results = bbList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
BollingerBandsHub observer = barHub.ToBollingerBandsHub(lookbackPeriods, standardDeviations);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<BollingerBandsResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
