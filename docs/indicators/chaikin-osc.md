---
title: Chaikin Oscillator
description: Created by Marc Chaikin, the Chaikin Oscillator is the difference between fast and slow Exponential Moving Averages (EMA) of an Accumulation / Distribution Line (ADL).
---

# Chaikin Oscillator

Created by Marc Chaikin, the [Chaikin Oscillator](https://en.wikipedia.org/wiki/Chaikin_Analytics#Chaikin_Oscillator) is the difference between fast and slow Exponential Moving Averages (EMA) of the [Accumulation/Distribution Line](/indicators/adl) (ADL).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/264 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="ChaikinOsc" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<ChaikinOscResult> results =
  bars.ToChaikinOsc(fastPeriods, slowPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
ChaikinOscList chaikinOscList = new(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => chaikinOscList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<ChaikinOscResult> results = chaikinOscList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
ChaikinOscHub observer = barHub.ToChaikinOscHub(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ChaikinOscResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `fastPeriods`<br>_`int`_ | Number of periods (`F`) in the ADL fast EMA.  Must be greater than 0 and smaller than `S`.  Default is 3. |
| `slowPeriods`<br>_`int`_ | Number of periods (`S`) in the ADL slow EMA.  Must be greater than `F`.  Default is 10. |

### Historical price bars requirements

You must have at least `2×S` or `S+100` periods of `bars`, whichever is more,  to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `S+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<ChaikinOscResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `S-1` periods will have `null` values for `Oscillator` since there's not enough data to calculate.
:::

::: buffer

```csharp
ChaikinOscList : IReadOnlyList<ChaikinOscResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `S-1` periods will have `null` values for `Oscillator` since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<ChaikinOscResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `S-1` periods will have `null` values for `Oscillator` since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `S+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ChaikinOscResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `MoneyFlowMultiplier`<br>_`double?`_ | Money Flow Multiplier |
| `MoneyFlowVolume`<br>_`double?`_ | Money Flow Volume |
| `Adl`<br>_`double?`_ | Accumulation Distribution Line (ADL) |
| `Oscillator`<br>_`double?`_ | Chaikin Oscillator |
| `FastEma`<br>_`double?`_ | Fast EMA of the ADL |
| `SlowEma`<br>_`double?`_ | Slow EMA of the ADL |

::: warning 🚩 Relative values only
Absolute values in MFV, ADL, FastEma, SlowEma, and Oscillator are somewhat meaningless.  Use with caution.
:::

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Oscillator` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToChaikinOsc(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Oscillator` further.

```csharp
// example
ChaikinOscList chaikinOscList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    chaikinOscList.Add(bar);
    slopeList.Add(chaikinOscList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Oscillator` further.

```csharp
// example
SlopeHub observer = barHub
    .ToChaikinOscHub(..)
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
