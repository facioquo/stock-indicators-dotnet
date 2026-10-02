---
title: Stochastic Oscillator
description: Created by George Lane, the Stochastic Oscillator, also known as KDJ Index, is a momentum oscillator that compares current financial market price with recent highs and lows and is presented on a scale of 0 to 100.  %J is also included for the KDJ Index extension.
---

# Stochastic Oscillator

Created by George Lane, the [Stochastic Oscillator](https://en.wikipedia.org/wiki/Stochastic_oscillator), also known as KDJ Index, is a momentum oscillator that compares current price with recent highs and lows and is presented on a scale of 0 to 100.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/237 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Stoch" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax (standard)
IReadOnlyList<StochResult> results =
  bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

// advanced customization
IReadOnlyList<StochResult> results =
  bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods,
                  kFactor, dFactor, movingAverageType);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Lookback period (`N`) for the oscillator (%K).  Must be greater than 0.  Default is 14. |
| `signalPeriods` | _`int`_ | Smoothing period for the signal (%D).  Must be greater than 0.  Default is 3. |
| `smoothPeriods` | _`int`_ | Smoothing period (`S`) for the Oscillator (%K).  "Slow" stochastic uses 3, "Fast" stochastic uses 1.  Must be greater than 0.  Default is 3. |
| `kFactor` | _`double`_ | Optional. Weight of %K in the %J calculation.  Must be greater than 0. Default is 3. |
| `dFactor` | _`double`_ | Optional. Weight of %D in the %J calculation.  Must be greater than 0. Default is 2. |
| `movingAverageType` | _`MaType`_ | Optional. Type of moving average (SMA or SMMA) used for smoothing.  See [`MaType` enum options](#matype-enum-options) below.  Default is `MaType.SMA`. |

### Historical price bars requirements

You must have at least `N+S` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

### `MaType` enum options

These are the supported moving average types:

| enum value    | moving average                                        |
| ------------- | ----------------------------------------------------- |
| `MaType.SMA`  | [Simple Moving Average](/indicators/sma) (default)    |
| `MaType.SMMA` | [Smoothed Moving Average](/indicators/smma)           |

## Response

```csharp
IReadOnlyList<StochResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N+S-2` periods will have `null` Oscillator values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods when using `MaType.SMMA`.  Standard use of `MaType.SMA` does not have convergence-related precision errors.
:::

### `StochResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Oscillator` or `K` | _`double`_ | %K Oscillator |
| `Signal` or `D` | _`double`_ | %D Simple moving average of Oscillator |
| `PercentJ` or `J` | _`double`_ | %J is the weighted divergence of %K and %D: `%J = kFactor × %K - dFactor × %D` |

Note: aliases of `K`, `D`, and `J` are also provided.  They can be used interchangeably with the standard outputs.

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
    .ToStoch(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
StochList stochList = new(lookbackPeriods, signalPeriods, smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => stochList.Add(bar);

// based on `ICollection<StochResult>`
IReadOnlyList<StochResult> results = stochList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
StochHub observer = barHub.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<StochResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
