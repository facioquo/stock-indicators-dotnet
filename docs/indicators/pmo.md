---
title: Price Momentum Oscillator (PMO)
description: Created by Carl Swenlin, the DecisionPoint Price Momentum Oscillator is double-smoothed momentum indicator, based on Rate of Change (ROC).
---

# Price Momentum Oscillator (PMO)

Created by Carl Swenlin, the DecisionPoint [Price Momentum Oscillator](https://www.google.com/search?q=DecisionPoint+Price+Momentum+Oscillator+(PMO)) is double-smoothed momentum indicator based on Rate of Change (ROC).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/244 "Community discussion about this indicator")

<StockIndicatorChart indicator="Pmo" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<PmoResult> results =
  bars.ToPmo(timePeriods, smoothPeriods, signalPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `timePeriods` | _`int`_ | Number of periods (`T`) for first ROC smoothing.  Must be greater than 1.  Default is 35. |
| `smoothPeriods` | _`int`_ | Number of periods (`S`) for second PMO smoothing.  Must be greater than 0.  Default is 20. |
| `signalPeriods` | _`int`_ | Number of periods (`G`) for Signal line EMA.  Must be greater than 0.  Default is 10. |

### Historical price bars requirements

You must have at least `N` periods of `bars`, where `N` is the greater of `T+S`, `2×T`, or `T+100` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses multiple smoothing operations, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<PmoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `T+S-1` periods will have `null` values for PMO since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `T+S+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `PmoResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Pmo` | _`double`_ | Price Momentum Oscillator |
| `Signal` | _`double`_ | Signal line is EMA of PMO |

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
    .ToPmo(..);
```

Results can be further processed on `Pmo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToPmo(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
PmoList pmoList = new(timePeriods, smoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => pmoList.Add(bar);

// based on `ICollection<PmoResult>`; fills as the handler runs
IReadOnlyList<PmoResult> results = pmoList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
PmoHub observer = barHub.ToPmoHub(timePeriods, smoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<PmoResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
