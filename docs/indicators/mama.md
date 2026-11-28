---
title: MESA Adaptive Moving Average (MAMA)
description: Created by John Ehlers, the MAMA indicator is a 5-period adaptive moving average of high/low price that uses classic electrical radio-frequency signal processing algorithms to reduce noise.
---

# MESA Adaptive Moving Average (MAMA)

Created by John Ehlers, the [MAMA](https://mesasoftware.com/papers/MAMA.pdf) indicator is a 5-period adaptive moving average of high/low price that uses classic electrical radio-frequency signal processing algorithms to reduce noise.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/211 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Mama" />

<ChartKey indicator="Mama" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<MamaResult> results =
  bars.ToMama(fastLimit, slowLimit);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
MamaList mamaList = new(fastLimit, slowLimit);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => mamaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<MamaResult> results = mamaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
MamaHub observer = barHub.ToMamaHub(fastLimit, slowLimit);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MamaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `fastLimit`<br>_`double`_ | Fast limit threshold.  Must be greater than `slowLimit` and less than 1.  Default is 0.5. |
| `slowLimit`<br>_`double`_ | Slow limit threshold.  Must be greater than 0.  Default is 0.05. |

### Historical price bars requirements

You must have at least `50` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<MamaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `5` periods will have `null` values for `Mama` since there's not enough data to calculate.
:::

::: buffer

```csharp
MamaList : IReadOnlyList<MamaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `5` periods will have `null` values for `Mama` since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<MamaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `5` periods will have `null` values for `Mama` since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `50` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `MamaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Mama`<br>_`double?`_ | MESA adaptive moving average (MAMA) |
| `Fama`<br>_`double?`_ | Following adaptive moving average (FAMA) |

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
    .ToMama(..);
```

Results can be further processed on `Mama` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToMama(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
MamaList mamaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    mamaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Mama` further.

```csharp
// example
MamaList mamaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    mamaList.Add(bar);
    rsiList.Add(mamaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
MamaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToMamaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Mama` further.

```csharp
// example
RsiHub observer = barHub
    .ToMamaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
