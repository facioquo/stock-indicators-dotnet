---
title: Standard deviation (volatility)
description: Standard deviation represents the volatility of historical financial market prices.  It is also known as Historical Volatility (HV), includes Z-score.
---

# Standard deviation (σ, volatility)

[Standard deviation](https://en.wikipedia.org/wiki/Standard_deviation) of price over a rolling lookback window.  Also known as Historical Volatility (HV), includes Z-score.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/239 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="StdDev" withOverlay />
  <StockIndicatorChart indicator="StdDevZScore" />
</ClientOnly>

```csharp
// C# usage syntax (series)
IReadOnlyList<StdDevResult> results =
  bars.ToStdDev(lookbackPeriods);

// usage with streaming bars
BarHub barHub = new();
StdDevHub observer = barHub.ToStdDevHub(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 1 to calculate; however we suggest a larger period for statistically appropriate sample size. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<StdDevResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `StdDevResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `StdDev` | _`double`_ | Standard deviation of price |
| `Mean` | _`double`_ | Mean value of price |
| `ZScore` | _`double`_ | Z-score of current price (number of standard deviations from mean) |

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
    .ToStdDev(..);
```

Results can be further processed on `StdDev` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToStdDev(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
StdDevList stdDevList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => stdDevList.Add(bar);

// based on `ICollection<StdDevResult>`
IReadOnlyList<StdDevResult> results = stdDevList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
StdDevHub observer = barHub.ToStdDevHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<StdDevResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
