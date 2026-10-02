---
title: Balance of Power (BOP)
description: Created by Igor Levshin, the Balance of Power (aka Balance of Market Power) is a momentum oscillator that depicts the strength of buying and selling pressure.
---

# Balance of Power (BOP)

Created by Igor Levshin, the [Balance of Power](https://www.google.com/search?q=Balance+of+Power+(BOP)+indicator) (aka Balance of Market Power) is a momentum oscillator that depicts the strength of buying and selling pressure.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/302 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Bop" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<BopResult> results =
  bars.ToBop(smoothPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `smoothPeriods` | _`int`_ | Number of periods (`N`) for smoothing.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<BopResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `BopResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Bop` | _`double`_ | Balance of Power |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Bop` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToBop(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
BopList bopList = new(smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => bopList.Add(bar);

// based on `ICollection<BopResult>`
IReadOnlyList<BopResult> results = bopList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
BopHub observer = barHub.ToBopHub(smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<BopResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
