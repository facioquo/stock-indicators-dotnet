---
title: Average True Range (ATR) / True Range (TR)
description: Created by J. Welles Wilder, True Range and Average True Range is a measure of volatility that captures gaps and limits between periods.
---

# Average True Range (ATR) / True Range (TR)

Created by J. Welles Wilder, True Range and [Average True Range](https://en.wikipedia.org/wiki/Average_true_range) are measures of volatility that capture gaps and limits between periods. See [True Range (TR)](/indicators/tr) for dedicated TR documentation, including streaming support.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/269 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Atr" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<AtrResult> results =
  bars.ToAtr(lookbackPeriods);

// ATR with custom moving average
IReadOnlyList<SmmaResult> results =
  bars.ToTr().ToSmma(lookbackPeriods);

// raw True Range (TR) only
IReadOnlyList<TrResult> results =
  bar.ToTr();
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) to consider.  Must be greater than 1. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AtrResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for ATR since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `AtrResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Tr` | _`double`_ | True Range for current period |
| `Atr` | _`double`_ | Average True Range |
| `Atrp` | _`double`_ | Average True Range Percent is `(ATR/Price)*100`.  This normalizes so it can be compared to other stocks. |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Atrp` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAtr(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
AtrList atrList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => atrList.Add(bar);

// based on `ICollection<AtrResult>`
IReadOnlyList<AtrResult> results = atrList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AtrHub observer = barHub.ToAtrHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<AtrResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
