---
title: SuperTrend
description: Created by Oliver Seban, the SuperTrend indicator attempts to determine the primary trend of financial market prices by using Average True Range (ATR) band thresholds around an HL2 midline.  It can indicate a buy/sell signal or a trailing stop when the trend changes.
---

# SuperTrend {#supertrend-indicator}

Created by Oliver Seban, the SuperTrend indicator attempts to determine the primary trend of prices by using [Average True Range (ATR)](/indicators/atr) band thresholds around an HL2 midline.  It can indicate a buy/sell signal or a trailing stop when the trend changes.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/235 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="SuperTrend" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<SuperTrendResult> results =
  bars.ToSuperTrend(lookbackPeriods, multiplier);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) for the ATR evaluation.  Must be greater than 1 and is usually set between 7 and 14.  Default is 10. |
| `multiplier` | _`double`_ | Multiplier sets the ATR band width.  Must be greater than 0 and is usually set around 2 to 3.  Default is 3. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` periods prior to the intended usage date for optimal precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<SuperTrendResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` SuperTrend values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
the line segment before the first reversal and the first `N+100` periods are unreliable due to an initial guess of trend direction and precision convergence for the underlying ATR values.
:::

### `SuperTrendResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `SuperTrend` | _`decimal`_ | SuperTrend line contains both Upper and Lower segments |
| `UpperBand` | _`decimal`_ | Upper band only (bearish/red) |
| `LowerBand` | _`decimal`_ | Lower band only (bullish/green) |

`UpperBand` and `LowerBand` values are provided to differentiate bullish vs bearish trends and to clearly demark trend reversal.  `SuperTrend` is the contiguous combination of both upper and lower line data.

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
SuperTrendList superTrendList = new(lookbackPeriods, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => superTrendList.Add(bar);

// based on `ICollection<SuperTrendResult>`; fills as the handler runs
IReadOnlyList<SuperTrendResult> results = superTrendList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
SuperTrendHub observer = barHub.ToSuperTrendHub(lookbackPeriods, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SuperTrendResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
