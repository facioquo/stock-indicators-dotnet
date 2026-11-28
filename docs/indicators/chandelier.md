---
title: Chandelier Exit
description: Created by Charles Le Beau, the Chandelier Exit is an adjusted Average True Range (ATR) offset from price that is typically used for stop-loss and can be computed for both long or short types.
---

# Chandelier Exit

Created by Charles Le Beau, the [Chandelier Exit](https://www.google.com/search?q=Chandelier+Exit+indicator) is an adjusted Average True Range (ATR) offset from price that is typically used for stop-loss and can be computed for both long or short types.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/263 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Chandelier" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<ChandelierResult> results =
  bars.ToChandelier(lookbackPeriods, multiplier, type);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
ChandelierList chandelierList = new(lookbackPeriods, multiplier, type);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => chandelierList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<ChandelierResult> results = chandelierList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
ChandelierHub observer = barHub.ToChandelierHub(lookbackPeriods, multiplier, type);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ChandelierResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the lookback evaluation.  Must be greater than 1.  Default is 22. |
| `multiplier`<br>_`double`_ | Multiplier number must be a positive value.  Default is 3. |
| `type`<br>_`Direction`_ | Direction of exit. Default is `Direction.Long`. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since the underlying ATR uses a smoothing technique, we recommend you use at least `N+250` periods prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-direction.md-->

## Response

::: series

```csharp
IReadOnlyList<ChandelierResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` Chandelier values since there's not enough data to calculate.
:::

::: buffer

```csharp
ChandelierList : IReadOnlyList<ChandelierResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` Chandelier values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<ChandelierResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` Chandelier values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors in the underlying ATR values.
:::

### `ChandelierResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `ChandelierExit`<br>_`double?`_ | Exit line |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `ChandelierExit` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToChandelier(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `ChandelierExit` further.

```csharp
// example
ChandelierList chandelierList = new(..);
EmaList emaList = new(..);

void OnBarReceived(IBar bar)
{
    chandelierList.Add(bar);
    emaList.Add(chandelierList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `ChandelierExit` further.

```csharp
// example
EmaHub observer = barHub
    .ToChandelierHub(..)
    .ToEmaHub(..);
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

Call these on the list itself, as in `chandelierList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `chandelierList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
