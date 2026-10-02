---
title: Chaikin Oscillator
description: Created by Marc Chaikin, the Chaikin Oscillator is the difference between fast and slow Exponential Moving Averages (EMA) of an Accumulation / Distribution Line (ADL).
---

# Chaikin Oscillator

Created by Marc Chaikin, the [Chaikin Oscillator](https://en.wikipedia.org/wiki/Chaikin_Analytics#Chaikin_Oscillator) is the difference between fast and slow Exponential Moving Averages (EMA) of the [Accumulation/Distribution Line](/indicators/adl) (ADL).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/264 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="ChaikinOsc" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<ChaikinOscResult> results =
  bars.ToChaikinOsc(fastPeriods, slowPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `fastPeriods` | _`int`_ | Number of periods (`F`) in the ADL fast EMA.  Must be greater than 0 and smaller than `S`.  Default is 3. |
| `slowPeriods` | _`int`_ | Number of periods (`S`) in the ADL slow EMA.  Must be greater than `F`.  Default is 10. |

### Historical price bars requirements

You must have at least `2×S` or `S+100` periods of `bars`, whichever is more,  to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `S+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<ChaikinOscResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `S-1` periods will have `null` values for `Oscillator` since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `S+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ChaikinOscResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `MoneyFlowMultiplier` | _`double`_ | Money Flow Multiplier |
| `MoneyFlowVolume` | _`double`_ | Money Flow Volume |
| `Adl` | _`double`_ | Accumulation Distribution Line (ADL) |
| `Oscillator` | _`double`_ | Chaikin Oscillator |

::: warning 🚩
absolute values in MFV, ADL, and Oscillator are somewhat meaningless.  Use with caution.
:::

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
    .ToChaikinOsc(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ChaikinOscList chaikinOscList = new(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => chaikinOscList.Add(bar);

// based on `ICollection<ChaikinOscResult>`
IReadOnlyList<ChaikinOscResult> results = chaikinOscList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ChaikinOscHub observer = barHub.ToChaikinOscHub(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<ChaikinOscResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
