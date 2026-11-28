---
title: Arnaud Legoux Moving Average (ALMA)
description: Created by Arnaud Legoux and Dimitrios Kouzis-Loukas, ALMA is a normal Gaussian distribution weighted moving average of price.
---

# Arnaud Legoux Moving Average (ALMA)

Created by Arnaud Legoux and Dimitrios Kouzis-Loukas, [ALMA](https://github.com/facioquo/stock-indicators-dotnet/files/5654531/ALMA-Arnaud-Legoux-Moving-Average.pdf) is a normal Gaussian distribution weighted moving average of price.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/209 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Alma" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AlmaResult> results =
  bars.ToAlma(lookbackPeriods, offset, sigma);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AlmaList almaList = new(lookbackPeriods, offset, sigma);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => almaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AlmaResult> results = almaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AlmaHub observer = barHub.ToAlmaHub(lookbackPeriods, offset, sigma);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AlmaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1, but is typically in the 5-20 range.  Default is 9. |
| `offset`<br>_`double`_ | Adjusts smoothness versus responsiveness on a scale from 0 to 1; where 1 is max responsiveness.  Default is 0.85. |
| `sigma`<br>_`double`_ | Defines the width of the Gaussian [normal distribution](https://en.wikipedia.org/wiki/Normal_distribution).  Must be greater than 0.  Default is 6. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<AlmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
AlmaList : IReadOnlyList<AlmaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<AlmaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

### `AlmaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Alma`<br>_`double?`_ | Arnaud Legoux Moving Average |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToAlma(..);
```

Results can be further processed on `Alma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAlma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
AlmaList almaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    almaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Alma` further.

```csharp
// example
AlmaList almaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    almaList.Add(bar);
    rsiList.Add(almaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
AlmaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToAlmaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Alma` further.

```csharp
// example
RsiHub observer = barHub
    .ToAlmaHub(..)
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

Call these on the list itself, as in `almaList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `almaList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
