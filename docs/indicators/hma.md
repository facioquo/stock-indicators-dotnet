---
title: Hull Moving Average (HMA)
description: Created by Alan Hull, the Hull Moving Average is a modified weighted average of price that reduces lag.
---

# Hull Moving Average (HMA)

Created by Alan Hull, the [Hull Moving Average](https://alanhull.com/hull-moving-average) is a modified weighted average of price that reduces lag.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/252 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Hma" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<HmaResult> results =
  bars.ToHma(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
HmaList hmaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => hmaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<HmaResult> results = hmaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
HmaHub observer = barHub.ToHmaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<HmaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1.  Default is 14. |

### Historical price bars requirements

You must have at least `N+(integer of SQRT(N))-1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<HmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N+(integer of SQRT(N))-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
HmaList : IReadOnlyList<HmaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N+(integer of SQRT(N))-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<HmaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N+(integer of SQRT(N))-1` periods will have `null` values since there's not enough data to calculate.
:::

### `HmaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Hma`<br>_`double?`_ | Hull moving average |

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
    .ToHma(..);
```

Results can be further processed on `Hma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToHma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
HmaList hmaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    hmaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Hma` further.

```csharp
// example
HmaList hmaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    hmaList.Add(bar);
    rsiList.Add(hmaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
HmaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToHmaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Hma` further.

```csharp
// example
RsiHub observer = barHub
    .ToHmaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
