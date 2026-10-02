---
title: Parabolic SAR
description: Created by J. Welles Wilder, Parabolic SAR (stop and reverse) is a price-time based indicator used to determine trend direction and reversals.  It can be used to identify trend direction, reversals, and stop-loss signals.
---

# Parabolic SAR

Created by J. Welles Wilder, [Parabolic SAR](https://en.wikipedia.org/wiki/Parabolic_SAR) (stop and reverse) is a price-time based indicator used to determine trend direction and reversals.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/245 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="ParabolicSar" />
</ClientOnly>

```csharp
// C# usage syntax (standard)
IReadOnlyList<ParabolicSarResult> results =
  bars.ToParabolicSar(accelerationStep, maxAccelerationFactor);

// alternate usage with custom initial Factor
IReadOnlyList<ParabolicSarResult> results =
  bars.ToParabolicSar(accelerationStep, maxAccelerationFactor, initialFactor);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `accelerationStep` | _`double`_ | Incremental step size for the Acceleration Factor.  Must be greater than 0.  Default is 0.02 |
| `maxAccelerationFactor` | _`double`_ | Maximum factor limit.  Must be greater than `accelerationStep`.  Default is 0.2 |
| `initialFactor` | _`double`_ | Optional.  Initial Acceleration Factor.  Must be greater than 0 and not larger than `maxAccelerationFactor`.  Default is `accelerationStep`. |

### Historical price bars requirements

You must have at least two historical price bars to cover the warmup periods; however, we recommend at least 100 data points.  Initial Parabolic SAR values prior to the first reversal are not accurate and are excluded from the results.  Therefore, provide sufficient bars to capture prior trend reversals, before your intended usage period.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<ParabolicSarResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first trend will have `null` values since it is not accurate and based on an initial guess.

### `ParabolicSarResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sar` | _`double`_ | Stop and Reverse value |
| `IsReversal` | _`bool`_ | Indicates a trend reversal |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Sar` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToParabolicSar(..)
    .ToEma(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ParabolicSarList psarList = new(accelerationStep, maxAccelerationFactor);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => psarList.Add(bar);

// based on `ICollection<ParabolicSarResult>`
IReadOnlyList<ParabolicSarResult> results = psarList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ParabolicSarHub observer = barHub.ToParabolicSarHub(accelerationStep, maxAccelerationFactor);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<ParabolicSarResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
