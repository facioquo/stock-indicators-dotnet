---
title: Detrended Price Oscillator (DPO)
description: Detrended Price Oscillator depicts the difference between price and an offset simple moving average.  It is used to identify trend cycles and duration.
---

# Detrended Price Oscillator (DPO)

[Detrended Price Oscillator](https://en.wikipedia.org/wiki/Detrended_price_oscillator) depicts the difference between price and an offset simple moving average.  It is used to identify trend cycles and duration.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/551 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Dpo" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<DpoResult> results =
  bars.ToDpo(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
DpoList dpoList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => dpoList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<DpoResult> results = dpoList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
DpoHub observer = barHub.ToDpoHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<DpoResult> results = observer.Results;
```

**Note:** DPO has a lookahead requirement (offset = N/2+1 periods), which means results are calculated when sufficient future data becomes available. This introduces a delay in real-time scenarios but maintains mathematical accuracy with the series implementation.

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N` historical price bars to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<DpoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N/2-2` and last `N/2+1` periods will be `null` since they cannot be calculated.
:::

::: buffer

```csharp
DpoList : IReadOnlyList<DpoResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N/2-2` and last `N/2+1` periods will be `null` since they cannot be calculated.
:::

::: stream

```csharp
IReadOnlyList<DpoResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N/2-2` and last `N/2+1` periods will be `null` since they cannot be calculated.
:::

### `DpoResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Dpo`<br>_`double?`_ | Detrended Price Oscillator (DPO) |
| `Sma`<br>_`double?`_ | Simple moving average offset by `N/2+1` periods |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToDpo(..);
```

Results can be further processed on `Dpo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToDpo(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
DpoList dpoList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    dpoList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Dpo` further.

```csharp
// example
DpoList dpoList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    dpoList.Add(bar);
    rsiList.Add(dpoList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
DpoHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToDpoHub(..);
```

Subscribe other chain-enabled hubs to it to process `Dpo` further.

```csharp
// example
RsiHub observer = barHub
    .ToDpoHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
