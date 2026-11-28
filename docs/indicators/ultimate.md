---
title: Ultimate Oscillator
description: Created by Larry Williams, the Ultimate Oscillator uses several moving averages to weigh buying power against true range price to produce an oversold / overbought oscillator.
---

# Ultimate Oscillator

Created by Larry Williams, the [Ultimate Oscillator](https://en.wikipedia.org/wiki/Ultimate_oscillator) uses several moving averages to weigh buying power against true range price to produce an oversold / overbought oscillator.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/231 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Ultimate" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<UltimateResult> results =
  bars.ToUltimate(shortPeriods, middlePeriods, longPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
UltimateList ultimateList = new(shortPeriods, middlePeriods, longPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => ultimateList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<UltimateResult> results = ultimateList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
UltimateHub observer = barHub.ToUltimateHub(shortPeriods, middlePeriods, longPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<UltimateResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `shortPeriods`<br>_`int`_ | Number of periods (`S`) in the short lookback.  Must be greater than 0.  Default is 7. |
| `middlePeriods`<br>_`int`_ | Number of periods (`M`) in the middle lookback.  Must be greater than `S`.  Default is 14. |
| `longPeriods`<br>_`int`_ | Number of periods (`L`) in the long lookback.  Must be greater than `M`.  Default is 28. |

### Historical price bars requirements

You must have at least `L+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<UltimateResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `L-1` periods will have `null` Ultimate values since there's not enough data to calculate.
:::

::: buffer

```csharp
UltimateList : IReadOnlyList<UltimateResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `L-1` periods will have `null` Ultimate values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<UltimateResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `L-1` periods will have `null` Ultimate values since there's not enough data to calculate.
:::

### `UltimateResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Ultimate`<br>_`double?`_ | Ultimate Oscillator |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Ultimate` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToUltimate(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Ultimate` further.

```csharp
// example
UltimateList ultimateList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    ultimateList.Add(bar);
    slopeList.Add(ultimateList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Ultimate` further.

```csharp
// example
SlopeHub observer = barHub
    .ToUltimateHub(..)
    .ToSlopeHub(..);
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

Call these on the list itself, as in `ultimateList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `ultimateList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
