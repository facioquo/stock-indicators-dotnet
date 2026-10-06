---
title: Rate of Change (ROC)
description: Rate of Change, also known as Momentum Oscillator, is the percent change of price over a lookback window.  Momentum is the raw price change equivalent.
---

# Rate of Change (ROC)

[Rate of Change](https://en.wikipedia.org/wiki/Momentum_(technical_analysis)), also known as Momentum Oscillator, is the percent change of price over a lookback window.  Momentum is the raw price change equivalent.  A [Rate of Change with Bands](/indicators/roc-wb) variant, created by Vitali Apirine, is also available.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/242 "Community discussion about this indicator")

<StockIndicatorChart indicator="Roc" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<RocResult> results =
  bars.ToRoc(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) to go back.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<RocResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for ROC since there's not enough data to calculate.

### `RocResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Momentum` | _`double`_ | Raw change in price over `N` periods |
| `Roc` | _`double`_ | Percent change in price (%, not decimal) |

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
    .ToRoc(..);
```

Results can be further processed on `Roc` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToRoc(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
RocList rocList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rocList.Add(bar);

// based on `ICollection<RocResult>`; fills as the handler runs
IReadOnlyList<RocResult> results = rocList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
RocHub observer = barHub.ToRocHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<RocResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
