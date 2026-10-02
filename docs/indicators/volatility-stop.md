---
title: Volatility Stop
description: Created by J. Welles Wilder, Volatility Stop, also known his Volatility System, is an ATR based indicator used to determine trend direction, stops, and reversals.  It is similar to Wilder's Parabolic SAR, SuperTrend, and more contemporary ATR Trailing Stop.
---

# Volatility Stop

Created by J. Welles Wilder, [Volatility Stop](https://archive.org/details/newconceptsintec00wild), also known his Volatility System, is an [ATR](/indicators/atr) based indicator used to determine trend direction, stops, and reversals.  It is similar to Wilder's [Parabolic SAR](/indicators/parabolic-sar) and [SuperTrend](/indicators/super-trend).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/564 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="VolatilityStop" />
</ClientOnly>

```csharp
// C# usage syntax (Series)
IReadOnlyList<VolatilityStopResult> results =
  bars.ToVolatilityStop(lookbackPeriods, multiplier);

// Usage with BarHub (streaming)
BarHub barHub = new();
VolatilityStopHub observer = barHub.ToVolatilityStopHub(lookbackPeriods, multiplier);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) ATR lookback window.  Must be greater than 1.  Default is 7. |
| `multiplier` | _`double`_ | ATR multiplier for the offset.  Must be greater than 0.  Default is 3.0. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since the underlying ATR uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.  Initial values prior to the first reversal are not accurate and are excluded from the results.  Therefore, provide sufficient bars to capture prior trend reversals.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<VolatilityStopResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first trend will have `null` values since it is not accurate and based on an initial guess.

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `VolatilityStopResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sar` | _`double`_ | Stop and Reverse value contains both Upper and Lower segments |
| `IsStop` | _`bool`_ | Indicates a trend reversal |
| `UpperBand` | _`double`_ | Upper band only (bearish/red) |
| `LowerBand` | _`double`_ | Lower band only (bullish/green) |

`UpperBand` and `LowerBand` values are provided to differentiate bullish vs bearish trends and to clearly demark trend reversal.  `Sar` is the contiguous combination of both upper and lower line data.

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
    .ToVolatilityStop(..)
    .ToEma(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
VolatilityStopList volatilityStopList = new(lookbackPeriods, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => volatilityStopList.Add(bar);

// based on `ICollection<VolatilityStopResult>`
IReadOnlyList<VolatilityStopResult> results = volatilityStopList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
VolatilityStopHub observer = barHub.ToVolatilityStopHub(lookbackPeriods, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<VolatilityStopResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
