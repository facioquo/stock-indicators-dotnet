---
title: Ulcer Index (UI)
description: Created by Peter Martin, the Ulcer Index is a measure of downside price volatility.  Often called the "heart attack" score, it measures the amount of pain seen from drawdowns in financial market prices and portfolio value.
---

# Ulcer Index (UI)

Created by Peter Martin, the [Ulcer Index](https://en.wikipedia.org/wiki/Ulcer_index) is a measure of downside price volatility over a lookback window.  Often called the "heart attack" score, it measures the amount of pain seen from drawdowns in financial market prices and portfolio value.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/232 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="UlcerIndex" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<UlcerIndexResult> results =
  bars.ToUlcerIndex(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
UlcerIndexList ulcerIndexList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => ulcerIndexList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<UlcerIndexResult> results = ulcerIndexList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
UlcerIndexHub observer = barHub.ToUlcerIndexHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<UlcerIndexResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for review.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<UlcerIndexResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
UlcerIndexList : IReadOnlyList<UlcerIndexResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<UlcerIndexResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

### `UlcerIndexResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `UlcerIndex`<br>_`double?`_ | Ulcer Index |

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
    .ToUlcerIndex(..);
```

Results can be further processed on `UlcerIndex` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToUlcerIndex(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
UlcerIndexList ulcerIndexList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    ulcerIndexList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `UlcerIndex` further.

```csharp
// example
UlcerIndexList ulcerIndexList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    ulcerIndexList.Add(bar);
    rsiList.Add(ulcerIndexList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
UlcerIndexHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToUlcerIndexHub(..);
```

Subscribe other chain-enabled hubs to it to process `UlcerIndex` further.

```csharp
// example
RsiHub observer = barHub
    .ToUlcerIndexHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
