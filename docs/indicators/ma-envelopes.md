---
title: Moving Average Envelopes
description:  Moving Average Envelopes is a price band channel overlay that is offset from the moving average of price.
---

# Moving Average Envelopes

[Moving Average Envelopes](https://en.wikipedia.org/wiki/Moving_average_envelope) is a price band channel overlay that is offset from the moving average of price.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/288 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="MaEnvelopes" />

<ChartKey indicator="MaEnvelopes" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<MaEnvelopeResult> results =
  bars.ToMaEnvelopes(lookbackPeriods, percentOffset, movingAverageType);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
MaEnvelopesList maEnvList = new(lookbackPeriods, percentOffset, movingAverageType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => maEnvList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<MaEnvelopeResult> results = maEnvList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
MaEnvelopesHub observer = barHub.ToMaEnvelopesHub(lookbackPeriods, percentOffset, movingAverageType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MaEnvelopeResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1. |
| `percentOffset`<br>_`double`_ | Percent offset for envelope width.  Example: 3.5% would be entered as 3.5 (not 0.035).  Must be greater than 0.  Typical values range from 2 to 10.  Default is 2.5. |
| `movingAverageType`<br>_`MaType`_ | Type of moving average (e.g. SMA, EMA, HMA).  See [`MaType` enum options](#matype-enum-options) below.  Default is `MaType.SMA`. |

### Historical price bars requirements

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

Detailed `bars` requirements for each of the [`MaType` enum options](#matype-enum-options) types can be found at the links in the following table.

### `MaType` enum options

These are the supported moving average types:

| enum value    | moving average                                        |
| ------------- | ----------------------------------------------------- |
| `MaType.ALMA` | [Arnaud Legoux Moving Average](/indicators/alma)      |
| `MaType.DEMA` | [Double Exponential Moving Average](/indicators/dema) |
| `MaType.EPMA` | [Endpoint Moving Average](/indicators/epma)           |
| `MaType.EMA`  | [Exponential Moving Average](/indicators/ema)         |
| `MaType.HMA`  | [Hull Moving Average](/indicators/hma)                |
| `MaType.SMA`  | [Simple Moving Average](/indicators/sma) (default)    |
| `MaType.SMMA` | [Smoothed Moving Average](/indicators/smma)           |
| `MaType.TEMA` | [Triple Exponential Moving Average](/indicators/tema) |
| `MaType.WMA`  | [Weighted Moving Average](/indicators/wma)            |

::: warning 🚩 ALMA parameters
For ALMA, default values are used for `offset` and `sigma`.
:::

## Response

::: series

```csharp
IReadOnlyList<MaEnvelopeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first periods will have `null` values since there's not enough data to calculate; the quantity will vary based on the `movingAverageType` specified.
:::

::: buffer

```csharp
MaEnvelopesList : IReadOnlyList<MaEnvelopeResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first periods will have `null` values since there's not enough data to calculate; the quantity will vary based on the `movingAverageType` specified.
:::

::: stream

```csharp
IReadOnlyList<MaEnvelopeResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first periods will have `null` values since there's not enough data to calculate; the quantity will vary based on the `movingAverageType` specified.
:::

::: warning 🚩 ⚞ Convergence warning
Some moving average variants have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.  See the [`MaType` enum options](#matype-enum-options) section above for more information.
:::

### `MaEnvelopeResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Centerline`<br>_`double?`_ | Moving average |
| `UpperEnvelope`<br>_`double?`_ | Upper envelope band |
| `LowerEnvelope`<br>_`double?`_ | Lower envelope band |

The moving average `Centerline` is based on the `movingAverageType` type specified.

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results **cannot** be further chained with additional transforms.

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HLC3)
    .ToMaEnvelopes(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
MaEnvelopesList maEnvList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    maEnvList.Add(smaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
MaEnvelopesHub observer = barHub
    .ToBarPartHub(CandlePart.HLC3)
    .ToMaEnvelopesHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
