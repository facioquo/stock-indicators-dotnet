---
title: Money Flow Index (MFI)
description: Created by Quong and Soudack, the Money Flow Index is a price-volume oscillator that shows buying and selling momentum.
---

# Money Flow Index (MFI)

Created by Quong and Soudack, the [Money Flow Index](https://en.wikipedia.org/wiki/Money_flow_index) is a price-volume oscillator that shows buying and selling momentum. Values outside of the 80 / 20 thresholds are considered overbought / oversold.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/247 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Mfi" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<MfiResult> results =
  bars.ToMfi(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
MfiList mfiList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => mfiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<MfiResult> results = mfiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
MfiHub observer = barHub.ToMfiHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MfiResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 1. Default is 14. |

### Historical price bars requirements

You must have at least `N+1` historical price bars to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<MfiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` MFI values since they cannot be calculated.
:::

::: buffer

```csharp
MfiList : IReadOnlyList<MfiResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` MFI values since they cannot be calculated.
:::

::: stream

```csharp
IReadOnlyList<MfiResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` MFI values since they cannot be calculated.
:::

### `MfiResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Mfi`<br>_`double?`_ | Money Flow Index |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Mfi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToMfi(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Mfi` further.

```csharp
// example
MfiList mfiList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    mfiList.Add(bar);
    rsiList.Add(mfiList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Mfi` further.

```csharp
// example
RsiHub observer = barHub
    .ToMfiHub(..)
    .ToRsiHub(..);
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

Call these on the list itself, as in `mfiList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `mfiList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
