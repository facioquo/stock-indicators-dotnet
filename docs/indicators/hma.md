---
title: Hull Moving Average (HMA)
description: Created by Alan Hull, the Hull Moving Average is a modified weighted average of price that reduces lag.
---

# Hull Moving Average (HMA)

Created by Alan Hull, the [Hull Moving Average](https://alanhull.com/hull-moving-average) is a modified weighted average of price that reduces lag.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/252 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Hma" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<HmaResult> results =
  bars.ToHma(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1. |

### Historical price bars requirements

You must have at least `N+(integer of SQRT(N))-1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<HmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N+(integer of SQRT(N))-1` periods will have `null` values since there's not enough data to calculate.

### `HmaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Hma` | _`double`_ | Hull moving average |

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
    .ToHma(..);
```

Results can be further processed on `Hma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToHma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
HmaList hmaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => hmaList.Add(bar);

// based on `ICollection<HmaResult>`
IReadOnlyList<HmaResult> results = hmaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
HmaHub observer = barHub.ToHmaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<HmaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
