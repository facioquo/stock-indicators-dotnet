---
title: Volume Weighted Moving Average (VWMA)
description: Volume Weighted Moving Average is the volume adjusted average price over a lookback window.
---

# Volume Weighted Moving Average (VWMA)

Volume Weighted Moving Average is the volume adjusted average price over a lookback window. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/657 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Vwma" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<VwmaResult> results =
  bars.ToVwma(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<VwmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values for `Vwma` since there's not enough data to calculate.

### `VwmaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Vwma` | _`double`_ | Volume Weighted Moving Average |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Vwma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToVwma(..)
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
VwmaList vwmaList = new(lookbackPeriods);

foreach (IBar bar in bars)  // simulating stream
{
  vwmaList.Add(bar);
}

// based on `ICollection<VwmaResult>`
IReadOnlyList<VwmaResult> results = vwmaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<VwmaResult> results = observer.Results;
```

### Additional buffering methods

For volume-weighted calculations, VWMA also supports direct price and volume input:

```csharp
VwmaList vwmaList = new(lookbackPeriods);

// Add individual price and volume data
vwmaList.Add(DateTime.Now, price: 100.50, volume: 1000);
```

**Note:** VWMA requires both price and volume data, so it only supports methods that accept `IBar` or direct price/volume parameters.

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
