---
title: Stochastic RSI
description: Created by Tushar Chande and Stanley Kroll, Stochastic RSI is a Stochastic Oscillator interpretation of the Relative Strength Index.  It is different from, and often confused with the more traditional Stochastic Oscillator.
---

# Stochastic RSI

Created by Tushar Chande and Stanley Kroll, [Stochastic RSI](https://chartschool.stockcharts.com/table-of-contents/technical-indicators-and-overlays/technical-indicators/stochrsi) is a Stochastic interpretation of the Relative Strength Index.  It is different from, and often confused with the more traditional [Stochastic Oscillator](/indicators/stoch). [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/236 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="StochRsi" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<StochRsiResult> results =
  bars.ToStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `rsiPeriods` | _`int`_ | Number of periods (`R`) in the lookback period.  Must be greater than 0.  Standard is 14. |
| `stochPeriods` | _`int`_ | Number of periods (`S`) in the lookback period.  Must be greater than 0.  Typically the same value as `rsiPeriods`. |
| `signalPeriods` | _`int`_ | Number of periods (`G`) in the signal line (SMA of the StochRSI).  Must be greater than 0.  Typically 3-5. |
| `smoothPeriods` | _`int`_ | Smoothing periods (`M`) for the Stochastic.  Must be greater than 0.  Default is 1 (Fast variant). |

The original Stochastic RSI formula uses a the Fast variant of the Stochastic calculation (`smoothPeriods=1`).  For a standard period of 14, the original formula would be `bars.ToStochRSI(14,14,3,1)`.  The "3" here is just for the Signal (%D), which is not present in the original formula, but useful for additional smoothing and analysis.

### Historical price bars requirements

You must have at least `N` periods of `bars`, where `N` is the greater of `R+S+M` and `R+100` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique in the underlying RSI value, we recommend you use at least `10×R` periods prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<StochRsiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `R+S+M` periods will have `null` values for `StochRsi` since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `10×R` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.  We recommend pruning at least `R+S+M+100` initial values.
:::

### `StochRsiResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `StochRsi` | _`double`_ | %K Oscillator = Stochastic RSI = Stoch(`S`,`G`,`M`) of RSI(`R`) of price |
| `Signal` | _`double`_ | %D Signal Line = Simple moving average of %K based on `G` periods |

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
    .ToStochRsi(..);
```

Results can be further processed on `StochRsi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToStochRsi(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
StochRsiList stochRsiList = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

foreach (IBar bar in bars)  // simulating stream
{
  stochRsiList.Add(bar);
}

// based on `ICollection<StochRsiResult>`
IReadOnlyList<StochRsiResult> results = stochRsiList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
StochRsiHub observer = barHub.ToStochRsiHub(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<StochRsiResult> results = observer.Results;
```

::: info Compound hub
The StochRSI hub is based on the RSI indicator. When the StochRSI hub is chained from an existing `RsiHub` instance it will reuse the existing RSI hub values rather than creating its own internal RSI calculations. **This is not a normal chaining model.**

```csharp
// creates a new internal RSI hub
var stochRsiHub = bars
  .ToStochRsiHub();
```

As an option, if you have an existing RSI hub you may reuse it:

```csharp

// existing hub
var rsiHub = bars
  .ToRsiHub();

// does not create a 2nd internal hub
var stochRsiHub = rsiHub
  .ToStochRsiHub();

// ❌ RSI → [ RSI ] → StochRSI
// ✅ RSI → StochRSI
```

:::

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
