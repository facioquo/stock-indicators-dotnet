---
title: Hurst Exponent
description: Hurst Exponent (H) with Rescaled Range Analysis is a random-walk path analysis that measures trending and mean-reverting tendencies of incremental return values.  When H is greater than 0.5 it depicts trending.  When H is less than 0.5 it is is more likely to revert to the mean.  When H is around 0.5 it represents a random walk.
---

# Hurst Exponent

The [Hurst Exponent](https://en.wikipedia.org/wiki/Hurst_exponent) (`H`) is part of a Rescaled Range Analysis, a [random-walk](https://en.wikipedia.org/wiki/Random_walk) path analysis that measures trending and mean-reverting tendencies of incremental return values.  When `H` is greater than 0.5 it depicts trending.  When `H` is less than 0.5 it is is more likely to revert to the mean.  When `H` is around 0.5 it represents a random walk.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/477 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Hurst" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<HurstResult> results =
  bars.ToHurst(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
HurstList hurstList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => hurstList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<HurstResult> results = hurstList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
HurstHub observer = barHub.ToHurstHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<HurstResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the Hurst Analysis.  Must be at least 20.  Default is 100. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<HurstResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
HurstList : IReadOnlyList<HurstResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<HurstResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

### `HurstResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `HurstExponent`<br>_`double?`_ | Hurst Exponent (`H`) from raw rescaled range (R/S) analysis |
| `HurstExponentAL`<br>_`double?`_ | [Anis-Lloyd corrected](https://en.wikipedia.org/wiki/Hurst_exponent#Rescaled_range_(R/S)_analysis) Hurst Exponent (`H`). Removes finite-sample bias from the raw R/S estimate. |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HLC3)
    .ToHurst(..);
```

Results can be further processed on `HurstExponent` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToHurst(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
HurstList hurstList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    hurstList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `HurstExponent` further.

```csharp
// example
HurstList hurstList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    hurstList.Add(bar);
    slopeList.Add(hurstList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
HurstHub observer = barHub
    .ToBarPartHub(CandlePart.HLC3)
    .ToHurstHub(..);
```

Subscribe other chain-enabled hubs to it to process `HurstExponent` further.

```csharp
// example
SlopeHub observer = barHub
    .ToHurstHub(..)
    .ToSlopeHub(..);
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

Call these on the list itself, as in `hurstList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `hurstList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.

## References

- Inputs are log returns `ln(c_t / c_{t-1})`, which provide additive, scale-consistent increments.
- `HurstExponent` (raw `H`) is the slope of `log(R/S)` against `log(n)` across chunk sizes, following the rescaled-range analysis of Hurst (1951) and Mandelbrot & Wallis (1969).
- `HurstExponentAL` applies the Anis & Lloyd (1976) finite-sample expectation `E[R/S]_n = Γ((n-1)/2) / (√π · Γ(n/2)) · Σ_{r=1}^{n-1} √((n-r)/r)`, then corrects each observed R/S as `R/S − E[R/S]_n + √(π·n/2)` before regressing. The expected value uses an exact `LogGamma` evaluation for `n ≤ 340` and Peters' (1994) Stirling approximation `√(2 / (π·(n-1)))` for larger `n`.
