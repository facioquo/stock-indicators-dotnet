---
title: SMA, MAD, MAPE, and MSE analysis
description: Mean absolute deviation (MAD), mean square error (MSE), and mean absolute percentage error (MAPE).
---

# SMA with extended analysis

[Simple Moving Average](https://en.wikipedia.org/wiki/Moving_average#Simple_moving_average) with extended statistical analysis including mean absolute deviation (MAD), mean square error (MSE), and mean absolute percentage error (MAPE).  See also [Simple Moving Average](/indicators/sma).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/240 "Community discussion about this indicator")

Each error metric charts on its own scale — MAD is in price units, MSE in price units squared, and MAPE is a fraction — so they are depicted separately.

<StyleSwitch />

<StockIndicatorChart indicator="SmaMad" withOverlay />
<StockIndicatorChart indicator="SmaMape" />
<StockIndicatorChart indicator="SmaMse" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<SmaAnalysisResult> results =
  bars.ToSmaAnalysis(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each value as it arrives:

```csharp
SmaAnalysisList smaAnalysisList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnValueReceived(IReusable value) => smaAnalysisList.Add(value);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<SmaAnalysisResult> results = smaAnalysisList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a chain-enabled hub for advanced streaming scenarios:

```csharp
BarHub barHub = new();
SmaAnalysisHub observer = barHub.ToSmaAnalysisHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SmaAnalysisResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback window. Must be greater than 0. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<SmaAnalysisResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
SmaAnalysisList : IReadOnlyList<SmaAnalysisResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<SmaAnalysisResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

### `SmaAnalysisResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Sma`<br>_`double?`_ | Simple moving average |
| `Mad`<br>_`double?`_ | Mean absolute deviation |
| `Mse`<br>_`double?`_ | Mean square error |
| `Mape`<br>_`double?`_ | Mean absolute percentage error |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToSmaAnalysis(..);
```

Results can be further processed on `Sma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSmaAnalysis(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
SmaAnalysisList smaAnalysisList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    smaAnalysisList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Sma` further.

```csharp
// example
SmaAnalysisList smaAnalysisList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    smaAnalysisList.Add(bar);
    rsiList.Add(smaAnalysisList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
SmaAnalysisHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToSmaAnalysisHub(..);
```

Subscribe other chain-enabled hubs to it to process `Sma` further.

```csharp
// example
RsiHub observer = barHub
    .ToSmaAnalysisHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
