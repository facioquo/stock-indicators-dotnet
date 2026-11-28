---
title: Volatility Stop
description: Created by J. Welles Wilder, Volatility Stop, also known his Volatility System, is an ATR based indicator used to determine trend direction, stops, and reversals.  It is similar to Wilder's Parabolic SAR, SuperTrend, and more contemporary ATR Trailing Stop.
---

# Volatility Stop

Created by J. Welles Wilder, [Volatility Stop](https://archive.org/details/newconceptsintec00wild), also known his Volatility System, is an [ATR](/indicators/atr) based indicator used to determine trend direction, stops, and reversals.  It is similar to Wilder's [Parabolic SAR](/indicators/parabolic-sar) and [SuperTrend](/indicators/super-trend).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/564 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="VolatilityStop" />

<ChartKey indicator="VolatilityStop" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<VolatilityStopResult> results =
  bars.ToVolatilityStop(lookbackPeriods, multiplier);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
VolatilityStopList volatilityStopList = new(lookbackPeriods, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => volatilityStopList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<VolatilityStopResult> results = volatilityStopList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
VolatilityStopHub observer = barHub.ToVolatilityStopHub(lookbackPeriods, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<VolatilityStopResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) ATR lookback window.  Must be greater than 1.  Default is 7. |
| `multiplier`<br>_`double`_ | ATR multiplier for the offset.  Must be greater than 0.  Default is 3.0. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since the underlying ATR uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.  Initial values prior to the first reversal are not accurate and are excluded from the results.  Therefore, provide sufficient bars to capture prior trend reversals.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<VolatilityStopResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first trend will have `null` values since it is not accurate and based on an initial guess.
:::

::: buffer

```csharp
VolatilityStopList : IReadOnlyList<VolatilityStopResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first trend will have `null` values since it is not accurate and based on an initial guess.
:::

::: stream

```csharp
IReadOnlyList<VolatilityStopResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first trend will have `null` values since it is not accurate and based on an initial guess.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `VolatilityStopResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Sar`<br>_`double?`_ | Stop and Reverse value contains both Upper and Lower segments |
| `IsStop`<br>_`bool?`_ | Indicates a trend reversal |
| `UpperBand`<br>_`double?`_ | Upper band only (bearish/red) |
| `LowerBand`<br>_`double?`_ | Lower band only (bullish/green) |

`UpperBand` and `LowerBand` values are provided to differentiate bullish vs bearish trends and to clearly demark trend reversal.  `Sar` is the contiguous combination of both upper and lower line data.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Sar` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToVolatilityStop(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Sar` further.

```csharp
// example
VolatilityStopList volatilityStopList = new(..);
EmaList emaList = new(..);

void OnBarReceived(IBar bar)
{
    volatilityStopList.Add(bar);
    emaList.Add(volatilityStopList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Stream hub results **cannot** be further chained with additional hubs.
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

Call these on the list itself, as in `volatilityStopList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `volatilityStopList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
