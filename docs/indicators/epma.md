---
title: Endpoint Moving Average (EPMA)
description: Endpoint Moving Average (EPMA), also known as Least Squares Moving Average (LSMA), plots the projected last point of a defined retrospective linear regression.
---

# Endpoint Moving Average (EPMA)

Endpoint Moving Average (EPMA), also known as Least Squares Moving Average (LSMA), plots the projected last point of a defined retrospective linear regression.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/371 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Epma" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<EpmaResult> results =
  bars.ToEpma(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
EpmaList epmaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => epmaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<EpmaResult> results = epmaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
EpmaHub observer = barHub.ToEpmaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<EpmaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1.  Default is 10. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<EpmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
EpmaList : IReadOnlyList<EpmaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<EpmaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

### `EpmaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Epma`<br>_`double?`_ | Endpoint moving average |

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
    .ToEpma(..);
```

Results can be further processed on `Epma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToEpma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
EpmaList epmaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    epmaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Epma` further.

```csharp
// example
EpmaList epmaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    epmaList.Add(bar);
    rsiList.Add(epmaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
EpmaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToEpmaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Epma` further.

```csharp
// example
RsiHub observer = barHub
    .ToEpmaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
