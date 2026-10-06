---
title: Hilbert Transform Instantaneous Trendline
description: Created by John Ehlers, the Hilbert Transform Instantaneous Trendline is a 5-period trendline of high/low price that that uses classic electrical radio-frequency signal processing algorithms reduce noise.
---

# Hilbert Transform Instantaneous Trendline

Created by John Ehlers, the Hilbert Transform Instantaneous Trendline is a 5-period trendline of high/low price that that uses classic electrical radio-frequency signal processing algorithms reduce noise.  Dominant Cycle Periods information is also provided.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/363 "Community discussion about this indicator")

<StockIndicatorChart indicator="HtTrendline" with="DcPeriods" />

```csharp
// C# usage syntax
IReadOnlyList<HtlResult> results =
  bars.ToHtTrendline();
```

## Historical price bars requirements

You must have at least `100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<HtlResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `6` periods will have `null` values for `SmoothPrice` since there's not enough data to calculate.
- The first `7` periods will have `null` values for `DcPeriods` since there is not enough data to calculate; and are generally unreliable for the first ~25 periods.

::: warning 🚩 ⚞ Convergence warning
The first `100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `HtlResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `DcPeriods` | _`int`_ | Dominant cycle periods (smoothed) |
| `Trendline` | _`double`_ | HT Trendline |
| `SmoothPrice` | _`double`_ | Weighted moving average of `(H+L)/2` price |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Streaming

### Real-time streaming

Use the streaming hub for real-time incremental calculations:

```csharp
BarHub barHub = new();
HtTrendlineHub observer = barHub.ToHtTrendlineHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<HtlResult> results = observer.Results;
```

### Buffer-style streaming

Use the buffer-style `List<T>` when you need incremental calculations:

```csharp
HtTrendlineList htlList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => htlList.Add(bar);

// based on `ICollection<HtlResult>`; fills as the handler runs
IReadOnlyList<HtlResult> results = htlList;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HLC3)
    .ToHtTrendline(..);
```

Results can be further processed on `Trendline` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToHtTrendline(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
