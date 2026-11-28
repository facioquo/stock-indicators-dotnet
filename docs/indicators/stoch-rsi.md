---
title: Stochastic RSI
description: Created by Tushar Chande and Stanley Kroll, Stochastic RSI is a Stochastic Oscillator interpretation of the Relative Strength Index.  It is different from, and often confused with the more traditional Stochastic Oscillator.
---

# Stochastic RSI

Created by Tushar Chande and Stanley Kroll, [Stochastic RSI](https://chartschool.stockcharts.com/table-of-contents/technical-indicators-and-overlays/technical-indicators/stochrsi) is a Stochastic interpretation of the Relative Strength Index.  It is different from, and often confused with the more traditional [Stochastic Oscillator](/indicators/stoch). [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/236 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="StochRsi" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<StochRsiResult> results =
  bars.ToStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
StochRsiList stochRsiList = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => stochRsiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<StochRsiResult> results = stochRsiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
StochRsiHub observer = barHub.ToStochRsiHub(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
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

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `rsiPeriods`<br>_`int`_ | Number of periods (`R`) in the lookback period.  Must be greater than 0.  Default is 14. |
| `stochPeriods`<br>_`int`_ | Number of periods (`S`) in the lookback period.  Must be greater than 0.  Typically the same value as `rsiPeriods`.  Default is 14. |
| `signalPeriods`<br>_`int`_ | Number of periods (`G`) in the signal line (SMA of the StochRSI).  Must be greater than 0.  Typically 3-5.  Default is 3. |
| `smoothPeriods`<br>_`int`_ | Smoothing periods (`M`) for the Stochastic.  Must be greater than 0.  Default is 1. |

The original Stochastic RSI formula uses a the Fast variant of the Stochastic calculation (`smoothPeriods=1`).  For a standard period of 14, the original formula would be `bars.ToStochRSI(14,14,3,1)`.  The "3" here is just for the Signal (%D), which is not present in the original formula, but useful for additional smoothing and analysis.

### Historical price bars requirements

You must have at least `N` periods of `bars`, where `N` is the greater of `R+S+M` and `R+100` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique in the underlying RSI value, we recommend you use at least `10×R` periods prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<StochRsiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `R+S+M-2` periods will have `null` values for `StochRsi`, and the first `R+S+M+G-3` periods for `Signal`, since there's not enough data to calculate.
:::

::: buffer

```csharp
StochRsiList : IReadOnlyList<StochRsiResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `R+S+M-2` periods will have `null` values for `StochRsi`, and the first `R+S+M+G-3` periods for `Signal`, since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<StochRsiResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `R+S+M-2` periods will have `null` values for `StochRsi`, and the first `R+S+M+G-3` periods for `Signal`, since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `10×R` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.  We recommend pruning at least `R+S+M+100` initial values.
:::

### `StochRsiResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `StochRsi`<br>_`double?`_ | %K Oscillator = Stochastic RSI = Stoch(`S`,`G`,`M`) of RSI(`R`) of price |
| `Signal`<br>_`double?`_ | %D Signal Line = Simple moving average of %K based on `G` periods |

## Chaining

::: series
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
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
StochRsiList stochRsiList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    stochRsiList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `StochRsi` further.

```csharp
// example
StochRsiList stochRsiList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    stochRsiList.Add(bar);
    slopeList.Add(stochRsiList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
StochRsiHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToStochRsiHub();
```

Subscribe other chain-enabled hubs to it to process `StochRsi` further.

```csharp
// example
SlopeHub observer = barHub
    .ToStochRsiHub()
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `stochRsiList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `stochRsiList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
