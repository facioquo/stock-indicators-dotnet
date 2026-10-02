---
title: True Range (TR)
description: Created by J. Welles Wilder, True Range is a measure of volatility that captures gaps and limits between periods.  It is the foundation for Average True Range (ATR).
---

# True Range (TR)

Created by J. Welles Wilder, [True Range](https://en.wikipedia.org/wiki/Average_true_range) is a measure of volatility that captures gaps and limits between periods.  It is the building block for [Average True Range](/indicators/atr).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/269 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Tr" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<TrResult> results =
  bars.ToTr();
```

## Historical price bars requirements

You must have at least 2 periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<TrResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period will have a `null` value since there is no prior period close.

### `TrResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Tr` | _`double`_ | True Range |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Tr` with additional chain-enabled indicators.

```csharp
// example: ATR using a custom moving average
var results = bars
    .ToTr()
    .ToSmma(lookbackPeriods);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
TrList trList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => trList.Add(bar);

// based on `ICollection<TrResult>`; fills as the handler runs
IReadOnlyList<TrResult> results = trList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
TrHub observer = barHub.ToTrHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TrResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
