---
title: Relative Strength Index (RSI)
description: Created by J. Welles Wilder, the Relative Strength Index is an oscillator that measures strength of the winning/losing price streak on a scale of 0 to 100, to depict overbought and oversold conditions.
---

# Relative Strength Index (RSI)

Created by J. Welles Wilder, the [Relative Strength Index](https://en.wikipedia.org/wiki/Relative_strength_index) is an oscillator that measures strength of the winning/losing streak over `N` lookback periods on a scale of 0 to 100, to depict overbought and oversold conditions.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/224 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Rsi" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<RsiResult> results =
  bars.ToRsi(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
RsiList rsiList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rsiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<RsiResult> results = rsiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
RsiHub observer = barHub.ToRsiHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<RsiResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `10×N` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<RsiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
RsiList : IReadOnlyList<RsiResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<RsiResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `10×N` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `RsiResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Rsi`<br>_`double?`_ | Relative Strength Index |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToRsi(..);
```

Results can be further processed on `Rsi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToRsi(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    rsiList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Rsi` further.

```csharp
// example
RsiList rsiList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    rsiList.Add(bar);
    slopeList.Add(rsiList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
RsiHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToRsiHub(..);
```

Subscribe other chain-enabled hubs to it to process `Rsi` further.

```csharp
// example
SlopeHub observer = barHub
    .ToRsiHub(..)
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
