---
title: Volume Weighted Average Price (VWAP)
description: The Volume Weighted Average Price is a volume weighted average of price, typically used on intraday data. Trading above or below the VWAP line can assist in finding favorable short-term trading windows.
---

# Volume Weighted Average Price (VWAP)

The [Volume Weighted Average Price](https://en.wikipedia.org/wiki/Volume-weighted_average_price) is a Volume weighted average of price, typically used on intraday data.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/310 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Vwap" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<VwapResult> results =
  bars.ToVwap();

// usage with optional anchored start date
IReadOnlyList<VwapResult> results =
  bars.ToVwap(startDate);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
VwapList vwapList = new(startDate);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => vwapList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<VwapResult> results = vwapList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
VwapHub observer = barHub.ToVwapHub(startDate);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<VwapResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `startDate`<br>_`DateTime`_ | Optional.  The anchor date used to start the VWAP accumulation.  The earliest date in `bars` is used when not provided. |

### Historical price bars requirements

You must have at least one historical bar to calculate; however, more is often needed to be useful.  Historical price bars are typically provided for a single day using minute-based intraday periods.  Since this is an accumulated weighted average price, different start dates will produce different results.  The accumulation starts at the first period in the provided `bars`, unless it is specified in the optional `startDate` parameter.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<VwapResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period or the `startDate` will have a `Vwap = Close` value since it is the initial starting point.
- `Vwap` values before `startDate`, if specified, will be `null`.
:::

::: buffer

```csharp
VwapList : IReadOnlyList<VwapResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first period or the `startDate` will have a `Vwap = Close` value since it is the initial starting point.
- `Vwap` values before `startDate`, if specified, will be `null`.
:::

::: stream

```csharp
IReadOnlyList<VwapResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first period or the `startDate` will have a `Vwap = Close` value since it is the initial starting point.
- `Vwap` values before `startDate`, if specified, will be `null`.
:::

### `VwapResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Vwap`<br>_`double?`_ | Volume Weighted Average Price |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Vwap` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToVwap(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Vwap` further.

```csharp
// example
VwapList vwapList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    vwapList.Add(bar);
    rsiList.Add(vwapList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Vwap` further.

```csharp
// example
RsiHub observer = barHub
    .ToVwapHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
