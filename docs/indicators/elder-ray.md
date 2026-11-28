---
title: Elder-ray Index
description: Created by Alexander Elder, the Elder-ray Index, also known as Bull and Bear Power, is an oscillator that depicts buying and selling pressure.  It compares current high/low prices against an Exponential Moving Average.
---

# Elder-ray Index

Created by Alexander Elder, the [Elder-ray Index](https://www.investopedia.com/terms/e/elderray.asp), also known as Bull and Bear Power, is an oscillator that depicts buying and selling pressure.  It compares current high/low prices against an Exponential Moving Average.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/378 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="ElderRay" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<ElderRayResult> results =
  bars.ToElderRay(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
ElderRayList elderRayList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => elderRayList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<ElderRayResult> results = elderRayList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
ElderRayHub observer = barHub.ToElderRayHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ElderRayResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the underlying EMA evaluation.  Must be greater than 0.  Default is 13. |

### Historical price bars requirements

You must have at least `2×N` or `N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<ElderRayResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` indicator values since there's not enough data to calculate.
:::

::: buffer

```csharp
ElderRayList : IReadOnlyList<ElderRayResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` indicator values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<ElderRayResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` indicator values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ElderRayResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Ema`<br>_`double?`_ | Exponential moving average |
| `BullPower`<br>_`double?`_ | Bull Power |
| `BearPower`<br>_`double?`_ | Bear Power |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `(BullPower+BearPower)` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToElderRay(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::
