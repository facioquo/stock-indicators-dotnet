---
title: Arnaud Legoux Moving Average (ALMA)
description: Created by Arnaud Legoux and Dimitrios Kouzis-Loukas, ALMA is a normal Gaussian distribution weighted moving average of price.
---

# Arnaud Legoux Moving Average (ALMA)

Created by Arnaud Legoux and Dimitrios Kouzis-Loukas, [ALMA](https://github.com/facioquo/stock-indicators-dotnet/files/5654531/ALMA-Arnaud-Legoux-Moving-Average.pdf) is a normal Gaussian distribution weighted moving average of price.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/209 "Community discussion about this indicator")

<StockIndicatorChart indicator="Alma" />

```csharp
// C# usage syntax
IReadOnlyList<AlmaResult> results =
  bars.ToAlma(lookbackPeriods, offset, sigma);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1, but is typically in the 5-20 range.  Default is 9. |
| `offset` | _`double`_ | Adjusts smoothness versus responsiveness on a scale from 0 to 1; where 1 is max responsiveness.  Default is 0.85. |
| `sigma` | _`double`_ | Defines the width of the Gaussian [normal distribution](https://en.wikipedia.org/wiki/Normal_distribution).  Must be greater than 0.  Default is 6. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AlmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `AlmaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Alma` | _`double`_ | Arnaud Legoux Moving Average |

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
    .ToAlma(..);
```

Results can be further processed on `Alma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAlma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
AlmaList almaList = new(lookbackPeriods, offset, sigma);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => almaList.Add(bar);

// based on `ICollection<AlmaResult>`; fills as the handler runs
IReadOnlyList<AlmaResult> results = almaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AlmaHub observer = barHub.ToAlmaHub(lookbackPeriods, offset, sigma);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AlmaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
