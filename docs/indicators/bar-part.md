---
title: Basic price bar transforms
description: Basic price bar transforms (e.g. HL2, OHL3, etc.) and isolation of individual price bar candle parts from a full OHLCV bar.
---

# Basic price bar transforms

Returns a reusable (chainable) basic bar transform (e.g. HL2, OHL3, etc.) by isolating a single component part value or calculated value from the full OHLCV bar candle parts.

<StyleSwitch />

<StockIndicatorChart indicator="HL2" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<TimeValue> results =
  bars.Use(candlePart);

// alternate syntax
IReadOnlyList<TimeValue> results =
  bars.ToBarPart(candlePart);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
BarPartList barPartList = new(CandlePart.HL2);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barPartList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<TimeValue> results = barPartList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
BarPartHub observer = barHub.ToBarPartHub(CandlePart.HL2);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TimeValue> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `candlePart`<br>_`CandlePart`_ | The [OHLCV](/guide/getting-started#historical-bars) element or simple price transform |

### Historical price bars requirements

You must have at least 1 period of `bars`.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc). See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-candlepart.md-->

## Response

::: series

```csharp
IReadOnlyList<TimeValue>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
:::

::: buffer

```csharp
BarPartList : IReadOnlyList<TimeValue>
```

- The list itself holds the results, one for each bar added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
:::

::: stream

```csharp
IReadOnlyList<TimeValue> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
:::

### `TimeValue` type

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Value`<br>_`double`_ | Price of `CandlePart` option |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Value` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .Use(CandlePart.OHLC4)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Value` further.

```csharp
// example
BarPartList barPartList = new(CandlePart.OHLC4);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    barPartList.Add(bar);
    rsiList.Add(barPartList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Value` further.

```csharp
// example
RsiHub observer = barHub
    .ToBarPartHub(CandlePart.OHLC4)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Find(lookupDate)](/utilities/results#find-by-date)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on the list itself, as in `barPartList.Find(lookupDate)`.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on `observer.Results`, as in `observer.Results.Find(lookupDate)`.
:::

See [Utilities and helpers](/utilities/) for more information.
