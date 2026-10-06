---
title: Hurst Exponent
description: Hurst Exponent (H) with Rescaled Range Analysis is a random-walk path analysis that measures trending and mean-reverting tendencies of incremental return values.  When H is greater than 0.5 it depicts trending.  When H is less than 0.5 it is is more likely to revert to the mean.  When H is around 0.5 it represents a random walk.
---

# Hurst Exponent

The [Hurst Exponent](https://en.wikipedia.org/wiki/Hurst_exponent) (`H`) is part of a Rescaled Range Analysis, a [random-walk](https://en.wikipedia.org/wiki/Random_walk) path analysis that measures trending and mean-reverting tendencies of incremental return values.  When `H` is greater than 0.5 it depicts trending.  When `H` is less than 0.5 it is is more likely to revert to the mean.  When `H` is around 0.5 it represents a random walk.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/477 "Community discussion about this indicator")

<StockIndicatorChart indicator="Hurst" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<HurstResult> results =
  bars.ToHurst(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the Hurst Analysis.  Must be at least 20.  Default is 100. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<HurstResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values since there's not enough data to calculate.

### `HurstResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `HurstExponent` | _`double`_ | Hurst Exponent (`H`) from raw rescaled range (R/S) analysis |
| `HurstExponentAL` | _`double`_ | [Anis-Lloyd corrected](https://en.wikipedia.org/wiki/Hurst_exponent#Rescaled_range_(R/S)_analysis) Hurst Exponent (`H`). Removes finite-sample bias from the raw R/S estimate. |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

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

## References

- Inputs are log returns `ln(c_t / c_{t-1})`, which provide additive, scale-consistent increments.
- `HurstExponent` (raw `H`) is the slope of `log(R/S)` against `log(n)` across chunk sizes, following the rescaled-range analysis of Hurst (1951) and Mandelbrot & Wallis (1969).
- `HurstExponentAL` applies the Anis & Lloyd (1976) finite-sample expectation `E[R/S]_n = Γ((n-1)/2) / (√π · Γ(n/2)) · Σ_{r=1}^{n-1} √((n-r)/r)`, then corrects each observed R/S as `R/S − E[R/S]_n + √(π·n/2)` before regressing. The expected value uses an exact `LogGamma` evaluation for `n ≤ 340` and Peters' (1994) Stirling approximation `√(2 / (π·(n-1)))` for larger `n`.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
HurstList hurstList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => hurstList.Add(bar);

// based on `ICollection<HurstResult>`; fills as the handler runs
IReadOnlyList<HurstResult> results = hurstList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
HurstHub observer = barHub.ToHurstHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<HurstResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
