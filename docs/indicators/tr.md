---
title: True Range (TR)
description: Created by J. Welles Wilder, True Range is a measure of volatility that captures gaps and limits between periods.  It is the foundation for Average True Range (ATR).
---

# True Range (TR)

Created by J. Welles Wilder, [True Range](https://en.wikipedia.org/wiki/Average_true_range) is a measure of volatility that captures gaps and limits between periods.  It is the building block for [Average True Range](/indicators/atr).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/269 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Tr" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<TrResult> results =
  bars.ToTr();
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
TrList trList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => trList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<TrResult> results = trList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
TrHub observer = barHub.ToTrHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TrResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Historical price bars requirements

You must have at least 2 periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<TrResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period will have a `null` value since there is no prior period close.
:::

::: buffer

```csharp
TrList : IReadOnlyList<TrResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first period will have a `null` value since there is no prior period close.
:::

::: stream

```csharp
IReadOnlyList<TrResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first period will have a `null` value since there is no prior period close.
:::

### `TrResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Tr`<br>_`double?`_ | True Range |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Tr` with additional chain-enabled indicators.

```csharp
// example: ATR using a custom moving average
var results = bars
    .ToTr()
    .ToSmma(lookbackPeriods);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Tr` further.

```csharp
// example
TrList trList = new();
SmmaList smmaList = new(..);

void OnBarReceived(IBar bar)
{
    trList.Add(bar);
    smmaList.Add(trList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Tr` further.

```csharp
// example
SmmaHub observer = barHub
    .ToTrHub()
    .ToSmmaHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `trList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `trList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
