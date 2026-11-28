---
title: Triple Exponential Moving Average (TEMA)
description: Created by Patrick G. Mulloy, the Triple Exponential Moving Average is a faster multi-smoothed moving average. TEMA is often confused with the alternative TRIX oscillator.
---

# Triple Exponential Moving Average (TEMA)

Created by Patrick G. Mulloy, the [Triple exponential moving average](https://en.wikipedia.org/wiki/Triple_exponential_moving_average) is a faster multi-smoothed EMA of the price over a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/808 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Tema" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<TemaResult> results =
  bars.ToTema(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
TemaList temaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => temaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<TemaResult> results = temaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
TemaHub observer = barHub.ToTemaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TemaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0.  Default is 20. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to produce any TEMA values.  However, due to the nature of the smoothing technique, we recommend you use at least `3×N+250` data points prior to the intended usage date for better precision.  See [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) guidance for more information.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<TemaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.  Also note that we are using the proper [weighted variant](https://en.wikipedia.org/wiki/Triple_exponential_moving_average) for TEMA.  If you prefer the unweighted raw 3 EMAs value, please use the `Ema3` output from the [TRIX](/indicators/trix) oscillator instead.
:::

::: buffer

```csharp
TemaList : IReadOnlyList<TemaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.  Also note that we are using the proper [weighted variant](https://en.wikipedia.org/wiki/Triple_exponential_moving_average) for TEMA.  If you prefer the unweighted raw 3 EMAs value, please use the `Ema3` output from the [TRIX](/indicators/trix) oscillator instead.
:::

::: stream

```csharp
IReadOnlyList<TemaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.  Also note that we are using the proper [weighted variant](https://en.wikipedia.org/wiki/Triple_exponential_moving_average) for TEMA.  If you prefer the unweighted raw 3 EMAs value, please use the `Ema3` output from the [TRIX](/indicators/trix) oscillator instead.
:::

**Example for TEMA(20)**:

```text
Period 1-19:  null values (incalculable)
Period 20:    first TEMA value (may have convergence issues)
Period 160+:  fully converged, reliable values
```

>&#9432; **Incalculable periods**: The first `N-1` periods will have `null` values since there's not enough data to calculate.
>
::: warning 🚩 ⚞ Convergence warning
The first `3×N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.  Use the `.RemoveWarmupPeriods()` method to remove these potentially unreliable values.
:::

### `TemaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Tema`<br>_`double?`_ | Triple exponential moving average |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToTema(..);
```

Results can be further processed on `Tema` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToTema(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
TemaList temaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    temaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Tema` further.

```csharp
// example
TemaList temaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    temaList.Add(bar);
    rsiList.Add(temaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
TemaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToTemaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Tema` further.

```csharp
// example
RsiHub observer = barHub
    .ToTemaHub(..)
    .ToRsiHub(..);
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

Call these on the list itself, as in `temaList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `temaList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
