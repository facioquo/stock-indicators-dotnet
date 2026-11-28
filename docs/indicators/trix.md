---
title: Triple EMA Oscillator (TRIX)
description: Created by Jack Hutson, TRIX is a rolling rate of change for a 3 EMA smoothing of the price over a lookback window.  TRIX is often confused with Triple EMA (TEMA).
---

# Triple EMA Oscillator (TRIX)

Created by Jack Hutson, [TRIX](https://en.wikipedia.org/wiki/Trix_(technical_analysis)) is the rate of change for a 3 EMA smoothing of the price over a lookback window.  TRIX is often confused with [TEMA](/indicators/tema).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/234 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Trix" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax for Trix
IReadOnlyList<TrixResult> results =
  bars.ToTrix(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
TrixList trixList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => trixList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<TrixResult> results = trixList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
TrixHub observer = barHub.ToTrixHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TrixResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in each of the exponential moving averages.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `4×N` or `3×N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `3×N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<TrixResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `3×N-3` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
TrixList : IReadOnlyList<TrixResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `3×N-3` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<TrixResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `3×N-3` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `3×N+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `TrixResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Ema3`<br>_`double?`_ | 3 EMAs of the price |
| `Trix`<br>_`double?`_ | Rate of Change of 3 EMAs |

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
    .ToTrix(..);
```

Results can be further processed on `Trix` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToTrix(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
TrixList trixList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    trixList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Trix` further.

```csharp
// example
TrixList trixList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    trixList.Add(bar);
    rsiList.Add(trixList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
TrixHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToTrixHub(..);
```

Subscribe other chain-enabled hubs to it to process `Trix` further.

```csharp
// example
RsiHub observer = barHub
    .ToTrixHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
