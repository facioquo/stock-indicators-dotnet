---
title: Moving Average Envelopes
description:  Moving Average Envelopes is a price band channel overlay that is offset from the moving average of price.
---

# Moving Average Envelopes

[Moving Average Envelopes](https://en.wikipedia.org/wiki/Moving_average_envelope) is a price band channel overlay that is offset from the moving average of price.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/288 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="MaEnvelopes" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<MaEnvelopeResult> results =
  bars.ToMaEnvelopes(lookbackPeriods, percentOffset, movingAverageType);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 1. |
| `percentOffset` | _`double`_ | Percent offset for envelope width.  Example: 3.5% would be entered as 3.5 (not 0.035).  Must be greater than 0.  Typical values range from 2 to 10.  Default is 2.5. |
| `movingAverageType` | _`MaType`_ | Type of moving average (e.g. SMA, EMA, HMA).  See [`MaType` enum options](#matype-enum-options) below.  Default is `MaType.SMA`. |

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

::: warning 🚩
For ALMA, default values are used for `offset` and `sigma`.
:::

## Response

```csharp
IReadOnlyList<MaEnvelopeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first periods will have `null` values since there's not enough data to calculate; the quantity will vary based on the `movingAverageType` specified.

::: warning 🚩 ⚞ Convergence warning
Some moving average variants have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.  See the [`MaType` enum options](#matype-enum-options) section above for more information.
:::

### `MaEnvelopeResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Centerline` | _`double`_ | Moving average |
| `UpperEnvelope` | _`double`_ | Upper envelope band |
| `LowerEnvelope` | _`double`_ | Lower envelope band |

The moving average `Centerline` is based on the `movingAverageType` type specified.

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HLC3)
    .ToMaEnvelopes(..);
```

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
MaEnvelopesList maEnvList = new(lookbackPeriods, percentOffset, movingAverageType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => maEnvList.Add(bar);

// based on `ICollection<MaEnvelopeResult>`; fills as the handler runs
IReadOnlyList<MaEnvelopeResult> results = maEnvList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
MaEnvelopesHub observer = barHub.ToMaEnvelopesHub(lookbackPeriods, percentOffset, movingAverageType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MaEnvelopeResult> results = observer.Results;
```

::: note
In streaming mode, only certain moving average types are supported. ALMA, EPMA, and HMA are not yet supported in streaming mode and will throw a `NotImplementedException`.
:::

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
