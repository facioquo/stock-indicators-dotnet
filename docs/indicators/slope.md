---
title: Slope and linear regression
description: Slope of the best fit line is determined by an ordinary least-squares simple linear regression on price.  It can be used to help identify trend strength and direction.  This indicator can be used to produce both a rolling slope value and a straight line through a specified lookback window.
---

# Slope and linear regression

[Slope of the best fit line](https://www.google.com/search?q=Slope+linear+regression+indicator) is determined by an [ordinary least-squares simple linear regression](https://en.wikipedia.org/wiki/Simple_linear_regression) on price.  It can be used to help identify trend strength and direction.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/241 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Slope" with="Linear" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<SlopeResult> results =
  bars.ToSlope(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
SlopeList slopeList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => slopeList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<SlopeResult> results = slopeList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
SlopeHub observer = barHub.ToSlopeHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SlopeResult> results = observer.Results;
```

::: warning 🖌️ Repaint warning
The streaming implementation exhibits the same repaint behavior as the series version. `Line` values are recalculated for the last `N` periods as new data arrives, matching the series implementation's behavior.
:::

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the linear regression.  Must be greater than 1.  Default is 14. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<SlopeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values for `Slope` since there's not enough data to calculate.
- `Line` values are only provided for the last `N` periods of your bar history
:::

::: buffer

```csharp
SlopeList : IReadOnlyList<SlopeResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values for `Slope` since there's not enough data to calculate.
- `Line` values are only provided for the last `N` periods of your bar history
:::

::: stream

```csharp
IReadOnlyList<SlopeResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values for `Slope` since there's not enough data to calculate.
- `Line` values are only provided for the last `N` periods of your bar history
:::

::: warning 🖌️ Repaint warning
The `Line` is continuously repainted since it is based on the last bar and lookback period.
:::

### `SlopeResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Slope`<br>_`double?`_ | Slope `𝑚` of the best-fit line of price |
| `Intercept`<br>_`double?`_ | Y-intercept `𝑏` of the best-fit line |
| `StdDev`<br>_`double?`_ | Standard deviation of price over `N` lookback periods |
| `RSquared`<br>_`double?`_ | R-squared (R&sup2;), aka Coefficient of determination |
| `Line`<br>_`decimal?`_ | Best-fit line `𝑦` over the last `N` periods (i.e. `𝑦=𝑚𝑥+𝑏` using last period values) |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .ToEma(..)
    .ToSlope(..);
```

Results can be further processed on `Slope` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSlope(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    slopeList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Slope` further.

```csharp
// example
SlopeList slopeList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    slopeList.Add(bar);
    rsiList.Add(slopeList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
SlopeHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToSlopeHub(..);
```

Subscribe other chain-enabled hubs to it to process `Slope` further.

```csharp
// example
RsiHub observer = barHub
    .ToSlopeHub(..)
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

Call these on the list itself, as in `slopeList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `slopeList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
