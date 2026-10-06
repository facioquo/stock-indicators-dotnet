---
title: Aroon
description: Created by Tushar Chande, Aroon is a oscillator view of how long ago the new high or low price occurred.
---

# Aroon {#aroon-indicator}

Created by Tushar Chande, [Aroon (Up/Down)](https://www.google.com/search?q=Aroon+Up/Down+Indicator) is a oscillator view that tracks how recently each lookback window saw a new high (Up) and low (Down). [Aroon Oscillator](https://www.google.com/search?q=Aroon+Up/Down+Indicator) is the difference `Up − Down`, presented as a single oscillator that crosses zero when the dominant trend flips.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/266 "Community discussion about this indicator")

<StockIndicatorChart indicator="Aroon" withOverlay />

<StockIndicatorChart indicator="AroonOsc" />

```csharp
// C# usage syntax
IReadOnlyList<AroonResult> results =
  bars.ToAroon(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) for the lookback evaluation.  Must be greater than 0.  Default is 25. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AroonResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values for `Aroon` since there's not enough data to calculate.

### `AroonResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `AroonUp` | _`double`_ | Based on last High price |
| `AroonDown` | _`double`_ | Based on last Low price |
| `Oscillator` | _`double`_ | AroonUp - AroonDown |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Oscillator` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAroon(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
AroonList aroonList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => aroonList.Add(bar);

// based on `ICollection<AroonResult>`; fills as the handler runs
IReadOnlyList<AroonResult> results = aroonList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AroonHub observer = barHub.ToAroonHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AroonResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
