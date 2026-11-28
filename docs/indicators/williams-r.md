---
title: Williams %R
description: Created by Larry Williams, the Williams %R momentum oscillator compares current price with recent highs and lows and is presented on scale of -100 to 0.  It is exactly the same as the fast variant of Stochastic Oscillator, but with a different scaling.
---

# Williams %R

Created by Larry Williams, the [Williams %R](https://en.wikipedia.org/wiki/Williams_%25R) momentum oscillator compares current price with recent highs and lows and is presented on scale of -100 to 0.  It is exactly the same as the fast variant of [Stochastic Oscillator](/indicators/stoch), but with a different scaling.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/229 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="WilliamsR" withOverlay     showRightAxisLabels=false />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<WilliamsResult> results =
  bars.ToWilliamsR(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
WilliamsRList williamsRList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => williamsRList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<WilliamsResult> results = williamsRList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
WilliamsRHub observer = barHub.ToWilliamsRHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<WilliamsResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<WilliamsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` Oscillator values since there's not enough data to calculate.
:::

::: buffer

```csharp
WilliamsRList : IReadOnlyList<WilliamsResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` Oscillator values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<WilliamsResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` Oscillator values since there's not enough data to calculate.
:::

### `WilliamsResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `WilliamsR`<br>_`double?`_ | Oscillator over prior `N` lookback periods |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `WilliamsR` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToWilliamsR(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `WilliamsR` further.

```csharp
// example
WilliamsRList williamsRList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    williamsRList.Add(bar);
    slopeList.Add(williamsRList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Stream hub results **cannot** be further chained with additional hubs.
:::
