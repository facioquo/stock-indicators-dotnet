---
title: Ultimate Oscillator
description: Created by Larry Williams, the Ultimate Oscillator uses several moving averages to weigh buying power against true range price to produce an oversold / overbought oscillator.
---

# Ultimate Oscillator

Created by Larry Williams, the [Ultimate Oscillator](https://en.wikipedia.org/wiki/Ultimate_oscillator) uses several moving averages to weigh buying power against true range price to produce an oversold / overbought oscillator.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/231 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Ultimate" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<UltimateResult> results =
  bars.ToUltimate(shortPeriods, middlePeriods, longPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `shortPeriods` | _`int`_ | Number of periods (`S`) in the short lookback.  Must be greater than 0.  Default is 7. |
| `middlePeriods` | _`int`_ | Number of periods (`M`) in the middle lookback.  Must be greater than `S`.  Default is 14. |
| `longPeriods` | _`int`_ | Number of periods (`L`) in the long lookback.  Must be greater than `M`.  Default is 28. |

### Historical price bars requirements

You must have at least `L+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<UltimateResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `L-1` periods will have `null` Ultimate values since there's not enough data to calculate.

### `UltimateResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Ultimate` | _`double`_ | Ultimate Oscillator |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Ultimate` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToUltimate(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
UltimateList ultimateList = new(shortPeriods, middlePeriods, longPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => ultimateList.Add(bar);

// based on `ICollection<UltimateResult>`
IReadOnlyList<UltimateResult> results = ultimateList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
UltimateHub observer = barHub.ToUltimateHub(shortPeriods, middlePeriods, longPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<UltimateResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
