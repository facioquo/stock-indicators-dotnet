---
title: Hilbert Transform Instantaneous Trendline
description: Created by John Ehlers, the Hilbert Transform Instantaneous Trendline is a 5-period trendline of high/low price that that uses classic electrical radio-frequency signal processing algorithms reduce noise.
---

# Hilbert Transform Instantaneous Trendline

Created by John Ehlers, the Hilbert Transform Instantaneous Trendline is a 5-period trendline of high/low price that that uses classic electrical radio-frequency signal processing algorithms reduce noise.  Dominant Cycle Periods information is also provided.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/363 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="HtTrendline" with="DcPeriods" />

<ChartKey indicator="HtTrendline" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<HtlResult> results =
  bars.ToHtTrendline();
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
HtTrendlineList htlList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => htlList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<HtlResult> results = htlList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
HtTrendlineHub observer = barHub.ToHtTrendlineHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<HtlResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Historical price bars requirements

You must have at least `100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<HtlResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `6` periods will have `null` values for `SmoothPrice` since there's not enough data to calculate.
- The first `7` periods will have `null` values for `DcPeriods` since there is not enough data to calculate; and are generally unreliable for the first ~25 periods.
:::

::: buffer

```csharp
HtTrendlineList : IReadOnlyList<HtlResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `6` periods will have `null` values for `SmoothPrice` since there's not enough data to calculate.
- The first `7` periods will have `null` values for `DcPeriods` since there is not enough data to calculate; and are generally unreliable for the first ~25 periods.
:::

::: stream

```csharp
IReadOnlyList<HtlResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `6` periods will have `null` values for `SmoothPrice` since there's not enough data to calculate.
- The first `7` periods will have `null` values for `DcPeriods` since there is not enough data to calculate; and are generally unreliable for the first ~25 periods.
:::

::: warning 🚩 ⚞ Convergence warning
The first `100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `HtlResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `DcPeriods`<br>_`int?`_ | Dominant cycle periods (smoothed) |
| `Trendline`<br>_`double?`_ | HT Trendline |
| `SmoothPrice`<br>_`double?`_ | Weighted moving average of `(H+L)/2` price |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HLC3)
    .ToHtTrendline(..);
```

Results can be further processed on `Trendline` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToHtTrendline(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
HtTrendlineList htlList = new();

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    htlList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Trendline` further.

```csharp
// example
HtTrendlineList htlList = new();
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    htlList.Add(bar);
    rsiList.Add(htlList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
HtTrendlineHub observer = barHub
    .ToBarPartHub(CandlePart.HLC3)
    .ToHtTrendlineHub();
```

Subscribe other chain-enabled hubs to it to process `Trendline` further.

```csharp
// example
RsiHub observer = barHub
    .ToHtTrendlineHub()
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

Call these on the list itself, as in `htlList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `htlList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
