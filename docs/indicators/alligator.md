---
title: Williams Alligator
description: Created by Bill Williams, Alligator is a depiction of three smoothed moving averages of median price, showing chart patterns that compared to an alligator's feeding habits when describing market movement. The moving averages are known as the Jaw, Teeth, and Lips, which are calculated using lookback and offset periods.  The related Gator Oscillator depicts periods of eating and resting.
---

# Williams Alligator

Created by Bill Williams, Alligator is a depiction of three smoothed moving averages of median price, showing chart patterns that compared to an alligator's feeding habits when describing market movement. The moving averages are known as the Jaw, Teeth, and Lips, which are calculated using lookback and offset periods.  See also the [Gator Oscillator](/indicators/gator).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/385 "Community discussion about this indicator")

<StockIndicatorChart indicator="Alligator" />

```csharp
// C# usage syntax
IReadOnlyList<AlligatorResult> results =
  bars.ToAlligator(jawPeriods,jawOffset,teethPeriods,teethOffset,lipsPeriods,lipsOffset);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `jawPeriods` | _`int`_ | Number of periods (`JP`) for the Jaw moving average.  Must be greater than `teethPeriods`.  Default is 13. |
| `jawOffset` | _`int`_ | Number of periods (`JO`) for the Jaw offset.  Must be greater than 0.  Default is 8. |
| `teethPeriods` | _`int`_ | Number of periods (`TP`) for the Teeth moving average.  Must be greater than `lipsPeriods`.  Default is 8. |
| `teethOffset` | _`int`_ | Number of periods (`TO`) for the Teeth offset.  Must be greater than 0.  Default is 5. |
| `lipsPeriods` | _`int`_ | Number of periods (`LP`) for the Lips moving average.  Must be greater than 0.  Default is 5. |
| `lipsOffset` | _`int`_ | Number of periods (`LO`) for the Lips offset.  Must be greater than 0.  Default is 3. |

### Historical price bars requirements

You must have at least `JP+JO+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods. Since this uses a smoothing technique, we recommend you use at least `JP+JO+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AlligatorResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `JP+JO` periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `JP+JO+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `AlligatorResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Jaw` | _`double`_ | Alligator's Jaw |
| `Teeth` | _`double`_ | Alligator's Teeth |
| `Lips` | _`double`_ | Alligator's Lips |

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
    .ToAlligator();
```

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
AlligatorList alligatorList = new(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => alligatorList.Add(bar);

// based on `ICollection<AlligatorResult>`; fills as the handler runs
IReadOnlyList<AlligatorResult> results = alligatorList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AlligatorHub observer = barHub.ToAlligatorHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AlligatorResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
