---
title: Detrended Price Oscillator (DPO)
description: Detrended Price Oscillator depicts the difference between price and an offset simple moving average.  It is used to identify trend cycles and duration.
---

# Detrended Price Oscillator (DPO)

[Detrended Price Oscillator](https://en.wikipedia.org/wiki/Detrended_price_oscillator) depicts the difference between price and an offset simple moving average.  It is used to identify trend cycles and duration.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/551 "Community discussion about this indicator")

<StockIndicatorChart indicator="Dpo" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<DpoResult> results =
  bars.ToDpo(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `N` historical price bars to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<DpoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N/2-2` and last `N/2+1` periods will be `null` since they cannot be calculated.

### `DpoResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sma` | _`double`_ | Simple moving average offset by `N/2+1` periods |
| `Dpo` | _`double`_ | Detrended Price Oscillator (DPO) |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToDpo(..);
```

Results can be further processed on `Dpo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToDpo(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
DpoList dpoList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => dpoList.Add(bar);

// based on `ICollection<DpoResult>`; fills as the handler runs
IReadOnlyList<DpoResult> results = dpoList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
DpoHub observer = barHub.ToDpoHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<DpoResult> results = observer.Results;
```

**Note:** DPO has a lookahead requirement (offset = N/2+1 periods), which means results are calculated when sufficient future data becomes available. This introduces a delay in real-time scenarios but maintains mathematical accuracy with the series implementation.

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
