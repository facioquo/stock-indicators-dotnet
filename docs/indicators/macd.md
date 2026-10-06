---
title: Moving Average Convergence / Divergence (MACD)
description: Created by Gerald Appel, MACD is a simple oscillator view of two converging / diverging exponential moving averages and their differences.
---

# Moving Average Convergence / Divergence (MACD)

Created by Gerald Appel, [MACD](https://en.wikipedia.org/wiki/MACD) is a simple oscillator view of two converging / diverging exponential moving averages and their differences.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/248 "Community discussion about this indicator")

<StockIndicatorChart indicator="Macd" withOverlay />

```csharp
// C# usage syntax (with Close price)
IReadOnlyList<MacdResult> results =
  bars.ToMacd(fastPeriods, slowPeriods, signalPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `fastPeriods` | _`int`_ | Number of periods (`F`) for the faster moving average. Must be greater than 0. Default is 12. |
| `slowPeriods` | _`int`_ | Number of periods (`S`) for the slower moving average. Must be greater than `fastPeriods`. Default is 26. |
| `signalPeriods` | _`int`_ | Number of periods (`P`) for the moving average of MACD. Must be greater than or equal to 0. Default is 9. |

### Historical price bars requirements

You must have at least `2×(S+P)` or `S+P+100` worth of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `S+P+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<MacdResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `S-1` slow periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `S+P+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `MacdResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Macd` | _`double`_ | The MACD line is the difference between slow and fast moving averages (`MACD = FastEma - SlowEma`) |
| `Signal` | _`double`_ | Moving average of the `MACD` line |
| `Histogram` | _`double`_ | Gap between the `MACD` and `Signal` line |
| `FastEma` | _`double`_ | Fast Exponential Moving Average |
| `SlowEma` | _`double`_ | Slow Exponential Moving Average |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToMacd(..);
```

Results can be further processed on `Macd` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToMacd(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
MacdList macdList = new(fastPeriods, slowPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => macdList.Add(bar);

// based on `ICollection<MacdResult>`; fills as the handler runs
IReadOnlyList<MacdResult> results = macdList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
MacdHub observer = barHub.ToMacdHub(fastPeriods, slowPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MacdResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
