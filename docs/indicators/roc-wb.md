---
title: Rate of Change with Bands
description: Rate of Change with Bands, created by Vitali Apirine, is a volatility banded variant of the basic Rate of Change (ROC) indicator.
---

# Rate of Change with Bands

Rate of Change (ROC) with Bands, created by Vitali Apirine, is a volatility banded variant of [Rate of Change (ROC)](/indicators/roc).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/242 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="RocWb" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<RocWbResult> results =
  bars.ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
RocWbList rocWbList = new(lookbackPeriods, emaPeriods, stdDevPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rocWbList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<RocWbResult> results = rocWbList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
RocWbHub observer = barHub.ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<RocWbResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) to go back.  Must be greater than 0.  Typical values range from 10-20.  Default is 20. |
| `emaPeriods`<br>_`int`_ | Number of periods for the ROC EMA line.  Must be greater than 0.  Default is 5. |
| `stdDevPeriods`<br>_`int`_ | Number of periods the standard deviation for upper/lower band lines.  Must be greater than 0 and not more than `lookbackPeriods`.  Default is 5. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<RocWbResult>
```

:::

::: buffer

```csharp
RocWbList : IReadOnlyList<RocWbResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
:::

::: stream

```csharp
IReadOnlyList<RocWbResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
:::

### `RocWbResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Roc`<br>_`double?`_ | Rate of Change over `N` lookback periods (%, not decimal) |
| `RocEma`<br>_`double?`_ | Exponential moving average (EMA) of `Roc` |
| `UpperBand`<br>_`double?`_ | Upper band of ROC (overbought indicator) |
| `LowerBand`<br>_`double?`_ | Lower band of ROC (oversold indicator) |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToRocWb(..);
```

Results can be further processed on `Roc` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToRocWb(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
RocWbList rocWbList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    rocWbList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Roc` further.

```csharp
// example
RocWbList rocWbList = new(..);
EmaList emaList = new(..);

void OnBarReceived(IBar bar)
{
    rocWbList.Add(bar);
    emaList.Add(rocWbList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
RocWbHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToRocWbHub(..);
```

Subscribe other chain-enabled hubs to it to process `Roc` further.

```csharp
// example
EmaHub observer = barHub
    .ToRocWbHub(..)
    .ToEmaHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Find(lookupDate)](/utilities/results#find-by-date)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on the list itself, as in `rocWbList.Find(lookupDate)`.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on `observer.Results`, as in `observer.Results.Find(lookupDate)`.
:::

See [Utilities and helpers](/utilities/) for more information.
