---
title: ConnorsRSI
description: Created by Laurence Connors, the ConnorsRSI is a composite oscillator that incorporates RSI, winning/losing streaks, and percentile gain metrics on scale of 0 to 100.
---

# ConnorsRSI {#connorsrsi-indicator}

Created by Laurence Connors, the [ConnorsRSI](https://alvarezquanttrading.com/wp-content/uploads/2016/05/ConnorsRSIGuidebook.pdf) is a composite oscillator that incorporates RSI, winning/losing streaks, and percentile gain metrics on scale of 0 to 100.  See [analysis](https://alvarezquanttrading.com/blog/connorsrsi-analysis).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/260 "Community discussion about this indicator")

<StockIndicatorChart indicator="ConnorsRsi" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<ConnorsRsiResult> results =
  bars.ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `rsiPeriods` | _`int`_ | Lookback period (`R`) for the price RSI.  Must be greater than 1.  Default is 3. |
| `streakPeriods` | _`int`_ | Lookback period (`S`) for the streak RSI.  Must be greater than 1.  Default is 2. |
| `rankPeriods` | _`int`_ | Lookback period (`P`) for the Percentile Rank.  Must be greater than 1.  Default is 100. |

### Historical price bars requirements

`N` is the greater of `R+100`, `S`, and `P+2`.  You must have at least `N` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+150` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<ConnorsRsiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `MAX(R,S,P)+1` periods will have `null` `ConnorsRsi` values since there's not enough data to calculate all three component scores (RSI of close, RSI of streak, percent rank) and combine them.

::: warning 🚩 ⚞ Convergence warning
The first `N` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ConnorsRsiResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Rsi` | _`double`_ | `RSI(R)` of the price. |
| `RsiStreak` | _`double`_ | `RSI(S)` of the Streak. |
| `PercentRank` | _`double`_ | Percentile rank of the period gain value. |
| `ConnorsRsi` | _`double`_ | ConnorsRSI |

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
    .ToConnorsRsi(..);
```

Results can be further processed on `ConnorsRsi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToConnorsRsi(..)
    .ToSma(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ConnorsRsiList connorsRsiList = new(rsiPeriods, streakPeriods, rankPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => connorsRsiList.Add(bar);

// based on `ICollection<ConnorsRsiResult>`; fills as the handler runs
IReadOnlyList<ConnorsRsiResult> results = connorsRsiList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ConnorsRsiHub observer = barHub.ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ConnorsRsiResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
