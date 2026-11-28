---
title: Aroon
description: Created by Tushar Chande, Aroon is a oscillator view of how long ago the new high or low price occurred.
---

# Aroon {#aroon-indicator}

Created by Tushar Chande, [Aroon (Up/Down)](https://www.google.com/search?q=Aroon+Up/Down+Indicator) is a oscillator view that tracks how recently each lookback window saw a new high (Up) and low (Down). [Aroon Oscillator](https://www.google.com/search?q=Aroon+Up/Down+Indicator) is the difference `Up − Down`, presented as a single oscillator that crosses zero when the dominant trend flips.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/266 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Aroon" withOverlay />

<StockIndicatorChart indicator="AroonOsc" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AroonResult> results =
  bars.ToAroon(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AroonList aroonList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => aroonList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AroonResult> results = aroonList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AroonHub observer = barHub.ToAroonHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AroonResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the lookback evaluation.  Must be greater than 0.  Default is 25. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<AroonResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values for `Aroon` since there's not enough data to calculate.
:::

::: buffer

```csharp
AroonList : IReadOnlyList<AroonResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values for `Aroon` since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<AroonResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values for `Aroon` since there's not enough data to calculate.
:::

### `AroonResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `AroonUp`<br>_`double?`_ | Based on last High price |
| `AroonDown`<br>_`double?`_ | Based on last Low price |
| `Oscillator`<br>_`double?`_ | AroonUp - AroonDown |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Oscillator` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAroon(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Oscillator` further.

```csharp
// example
AroonList aroonList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    aroonList.Add(bar);
    slopeList.Add(aroonList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Oscillator` further.

```csharp
// example
SlopeHub observer = barHub
    .ToAroonHub(..)
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
