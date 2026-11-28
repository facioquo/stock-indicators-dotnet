---
title: Williams Alligator
description: Created by Bill Williams, Alligator is a depiction of three smoothed moving averages of median price, showing chart patterns that compared to an alligator's feeding habits when describing market movement. The moving averages are known as the Jaw, Teeth, and Lips, which are calculated using lookback and offset periods.  The related Gator Oscillator depicts periods of eating and resting.
---

# Williams Alligator

Created by Bill Williams, Alligator is a depiction of three smoothed moving averages of median price, showing chart patterns that compared to an alligator's feeding habits when describing market movement. The moving averages are known as the Jaw, Teeth, and Lips, which are calculated using lookback and offset periods.  See also the [Gator Oscillator](/indicators/gator).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/385 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Alligator" />

<ChartKey indicator="Alligator" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AlligatorResult> results =
  bars.ToAlligator(jawPeriods,jawOffset,teethPeriods,teethOffset,lipsPeriods,lipsOffset);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AlligatorList alligatorList = new(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => alligatorList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AlligatorResult> results = alligatorList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AlligatorHub observer = barHub.ToAlligatorHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AlligatorResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `jawPeriods`<br>_`int`_ | Number of periods (`JP`) for the Jaw moving average.  Must be greater than `teethPeriods`.  Default is 13. |
| `jawOffset`<br>_`int`_ | Number of periods (`JO`) for the Jaw offset.  Must be greater than 0.  Default is 8. |
| `teethPeriods`<br>_`int`_ | Number of periods (`TP`) for the Teeth moving average.  Must be greater than `lipsPeriods`.  Default is 8. |
| `teethOffset`<br>_`int`_ | Number of periods (`TO`) for the Teeth offset.  Must be greater than 0.  Default is 5. |
| `lipsPeriods`<br>_`int`_ | Number of periods (`LP`) for the Lips moving average.  Must be greater than 0.  Default is 5. |
| `lipsOffset`<br>_`int`_ | Number of periods (`LO`) for the Lips offset.  Must be greater than 0.  Default is 3. |

### Historical price bars requirements

You must have at least `JP+JO+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods. Since this uses a smoothing technique, we recommend you use at least `JP+JO+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<AlligatorResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `JP+JO` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
AlligatorList : IReadOnlyList<AlligatorResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `JP+JO` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<AlligatorResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `JP+JO` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `JP+JO+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `AlligatorResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Jaw`<br>_`double?`_ | Alligator's Jaw |
| `Teeth`<br>_`double?`_ | Alligator's Teeth |
| `Lips`<br>_`double?`_ | Alligator's Lips |

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
    .ToAlligator();
```

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
AlligatorList alligatorList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    alligatorList.Add(smaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
AlligatorHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToAlligatorHub();
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
