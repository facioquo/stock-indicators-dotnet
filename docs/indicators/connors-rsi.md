---
title: ConnorsRSI
description: Created by Laurence Connors, the ConnorsRSI is a composite oscillator that incorporates RSI, winning/losing streaks, and percentile gain metrics on scale of 0 to 100.
---

# ConnorsRSI {#connorsrsi-indicator}

Created by Laurence Connors, the [ConnorsRSI](https://alvarezquanttrading.com/wp-content/uploads/2016/05/ConnorsRSIGuidebook.pdf) is a composite oscillator that incorporates RSI, winning/losing streaks, and percentile gain metrics on scale of 0 to 100.  See [analysis](https://alvarezquanttrading.com/blog/connorsrsi-analysis).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/260 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="ConnorsRsi" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<ConnorsRsiResult> results =
  bars.ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
ConnorsRsiList connorsRsiList = new(rsiPeriods, streakPeriods, rankPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => connorsRsiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<ConnorsRsiResult> results = connorsRsiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
ConnorsRsiHub observer = barHub.ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ConnorsRsiResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `rsiPeriods`<br>_`int`_ | Lookback period (`R`) for the price RSI.  Must be greater than 1.  Default is 3. |
| `streakPeriods`<br>_`int`_ | Lookback period (`S`) for the streak RSI.  Must be greater than 1.  Default is 2. |
| `rankPeriods`<br>_`int`_ | Lookback period (`P`) for the Percentile Rank.  Must be greater than 1.  Default is 100. |

### Historical price bars requirements

`N` is the greater of `R+100`, `S`, and `P+2`.  You must have at least `N` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+150` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<ConnorsRsiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `MAX(R,S,P)+1` periods will have `null` `ConnorsRsi` values since there's not enough data to calculate all three component scores (RSI of close, RSI of streak, percent rank) and combine them.
:::

::: buffer

```csharp
ConnorsRsiList : IReadOnlyList<ConnorsRsiResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `MAX(R,S,P)+1` periods will have `null` `ConnorsRsi` values since there's not enough data to calculate all three component scores (RSI of close, RSI of streak, percent rank) and combine them.
:::

::: stream

```csharp
IReadOnlyList<ConnorsRsiResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `MAX(R,S,P)+1` periods will have `null` `ConnorsRsi` values since there's not enough data to calculate all three component scores (RSI of close, RSI of streak, percent rank) and combine them.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ConnorsRsiResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Streak`<br>_`double`_ | Consecutive up (positive) or down (negative) periods in the price |
| `Rsi`<br>_`double?`_ | `RSI(R)` of the price. |
| `RsiStreak`<br>_`double?`_ | `RSI(S)` of the Streak. |
| `PercentRank`<br>_`double?`_ | Percentile rank of the period gain value. |
| `ConnorsRsi`<br>_`double?`_ | ConnorsRSI |

## Chaining

::: series
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
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
ConnorsRsiList connorsRsiList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    connorsRsiList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `ConnorsRsi` further.

```csharp
// example
ConnorsRsiList connorsRsiList = new(..);
SmaList smaList = new(..);

void OnBarReceived(IBar bar)
{
    connorsRsiList.Add(bar);
    smaList.Add(connorsRsiList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
ConnorsRsiHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToConnorsRsiHub(..);
```

Subscribe other chain-enabled hubs to it to process `ConnorsRsi` further.

```csharp
// example
SmaHub observer = barHub
    .ToConnorsRsiHub(..)
    .ToSmaHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `connorsRsiList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `connorsRsiList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
