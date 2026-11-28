---
title: ATR Trailing Stop
description: Created by Welles Wilder, the ATR Trailing Stop indicator attempts to determine the primary trend of financial market prices by using Average True Range (ATR) band thresholds.  It can indicate a buy/sell signal or a trailing stop when the trend changes.
---

# ATR Trailing Stop

Created by Welles Wilder, the ATR Trailing Stop indicator attempts to determine the primary trend of Close prices by using [Average True Range (ATR)](/indicators/atr) band thresholds.  It can indicate a buy/sell signal or a trailing stop when the trend changes.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/724 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="AtrStop" />

<ChartKey indicator="AtrStop" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AtrStopResult> results =
  bars.ToAtrStop(lookbackPeriods, multiplier, endType);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AtrStopList atrStopList = new(lookbackPeriods, multiplier: 3.0, endType: EndType.Close);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => atrStopList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AtrStopResult> results = atrStopList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods, multiplier: 3.0, endType: EndType.Close);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AtrStopResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the ATR evaluation.  Must be greater than 1.  Default is 21. |
| `multiplier`<br>_`double`_ | Multiplier sets the ATR band width.  Must be greater than 0 and is usually set around 2 to 3.  Default is 3. |
| `endType`<br>_`EndType`_ | Determines whether `Close` or `High/Low` is used as basis for stop offset. Default is `EndType.Close`. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` periods prior to the intended usage date for optimal precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

::: series

```csharp
IReadOnlyList<AtrStopResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` AtrStop values since there's not enough data to calculate.
:::

::: buffer

```csharp
AtrStopList : IReadOnlyList<AtrStopResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` AtrStop values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<AtrStopResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` AtrStop values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
the line segment before the first reversal and the first `N+100` periods are unreliable due to an initial guess of trend direction and precision convergence for the underlying ATR values.
:::

### `AtrStopResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `AtrStop`<br>_`double?`_ | ATR Trailing Stop line contains both Upper and Lower segments |
| `BuyStop`<br>_`double?`_ | Upper band only (green) |
| `SellStop`<br>_`double?`_ | Lower band only (red) |
| `Atr`<br>_`double?`_ | Average True Range |

`BuyStop` and `SellStop` values are provided to differentiate buy vs sell stop lines and to clearly demark trend reversal.  `AtrStop` is the contiguous combination of both upper and lower line data.

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.
