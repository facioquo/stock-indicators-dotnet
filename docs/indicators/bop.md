---
title: Balance of Power (BOP)
description: Created by Igor Levshin, the Balance of Power (aka Balance of Market Power) is a momentum oscillator that depicts the strength of buying and selling pressure.
---

# Balance of Power (BOP)

Created by Igor Levshin, the [Balance of Power](https://www.google.com/search?q=Balance+of+Power+(BOP)+indicator) (aka Balance of Market Power) is a momentum oscillator that depicts the strength of buying and selling pressure.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/302 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Bop" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<BopResult> results =
  bars.ToBop(smoothPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
BopList bopList = new(smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => bopList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<BopResult> results = bopList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
BopHub observer = barHub.ToBopHub(smoothPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<BopResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `smoothPeriods`<br>_`int`_ | Number of periods (`N`) for smoothing.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<BopResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
BopList : IReadOnlyList<BopResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<BopResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

### `BopResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Bop`<br>_`double?`_ | Balance of Power |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Bop` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToBop(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Bop` further.

```csharp
// example
BopList bopList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    bopList.Add(bar);
    slopeList.Add(bopList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Bop` further.

```csharp
// example
SlopeHub observer = barHub
    .ToBopHub(..)
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
