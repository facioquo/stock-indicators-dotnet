---
title: Slope and linear regression
description: Slope of the best fit line is determined by an ordinary least-squares simple linear regression on price.  It can be used to help identify trend strength and direction.  This indicator can be used to produce both a rolling slope value and a straight line through a specified lookback window.
---

# Slope and linear regression

[Slope of the best fit line](https://www.google.com/search?q=Slope+linear+regression+indicator) is determined by an [ordinary least-squares simple linear regression](https://en.wikipedia.org/wiki/Simple_linear_regression) on price.  It can be used to help identify trend strength and direction.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/241 "Community discussion about this indicator")

<StockIndicatorChart indicator="Slope" with="Linear" />

```csharp
// C# usage syntax
IReadOnlyList<SlopeResult> results =
  bars.ToSlope(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) for the linear regression.  Must be greater than 1. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<SlopeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values for `Slope` since there's not enough data to calculate.
- `Line` values are only provided for the last `N` periods of your bar history

::: warning 🖌️ Repaint warning
The `Line` is continuously repainted since it is based on the last bar and lookback period.
:::

### `SlopeResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Slope` | _`double`_ | Slope `𝑚` of the best-fit line of price |
| `Intercept` | _`double`_ | Y-intercept `𝑏` of the best-fit line |
| `StdDev` | _`double`_ | Standard deviation of price over `N` lookback periods |
| `RSquared` | _`double`_ | R-squared (R&sup2;), aka Coefficient of determination |
| `Line` | _`decimal`_ | Best-fit line `𝑦` over the last `N` periods (i.e. `𝑦=𝑚𝑥+𝑏` using last period values) |

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

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
SlopeList slopeList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => slopeList.Add(bar);

// based on `ICollection<SlopeResult>`; fills as the handler runs
IReadOnlyList<SlopeResult> results = slopeList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

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

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
