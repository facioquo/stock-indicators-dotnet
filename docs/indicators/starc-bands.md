---
title: STARC Bands
description: Created by Manning Stoller, the Stoller Average Range Channel (STARC) Bands are financial market price ranges based on an simple moving average centerline and Average True Range (ATR) band widths.  Keltner Channels are the EMA centerline equivalent.
---

# STARC Bands

Created by Manning Stoller, the [Stoller Average Range Channel (STARC) Bands](https://www.investopedia.com/terms/s/starc.asp), are price ranges based on an SMA centerline and ATR band widths.  See also [Keltner Channels](/indicators/keltner) for an EMA centerline equivalent.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/292 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="StarcBands" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<StarcBandsResult> results =
  bars.ToStarcBands(smaPeriods, multiplier, atrPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `smaPeriods` | _`int`_ | Number of lookback periods (`S`) for the center line moving average.  Must be greater than 1 to calculate and is typically between 5 and 10.  Default is 5. |
| `multiplier` | _`double`_ | ATR Multiplier. Must be greater than 0.  Default is 2. |
| `atrPeriods` | _`int`_ | Number of lookback periods (`A`) for the Average True Range.  Must be greater than 1 to calculate and is typically the same value as `smaPeriods`.  Default is 10. |

### Historical price bars requirements

You must have at least `S` or `A+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `A+150` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<StarcBandsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate, where `N` is the greater of `S` or `A`.

::: warning 🚩 ⚞ Convergence warning
The first `A+150` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `StarcBandsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `UpperBand` | _`double`_ | Upper STARC band |
| `Centerline` | _`double`_ | SMA of price |
| `LowerBand` | _`double`_ | Lower STARC band |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
StarcBandsList starcBandsList = new(smaPeriods, multiplier, atrPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => starcBandsList.Add(bar);

// based on `ICollection<StarcBandsResult>`; fills as the handler runs
IReadOnlyList<StarcBandsResult> results = starcBandsList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
StarcBandsHub observer = barHub.ToStarcBandsHub(smaPeriods, multiplier, atrPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<StarcBandsResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
