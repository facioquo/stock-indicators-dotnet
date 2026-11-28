---
title: Parabolic SAR
description: Created by J. Welles Wilder, Parabolic SAR (stop and reverse) is a price-time based indicator used to determine trend direction and reversals.  It can be used to identify trend direction, reversals, and stop-loss signals.
---

# Parabolic SAR

Created by J. Welles Wilder, [Parabolic SAR](https://en.wikipedia.org/wiki/Parabolic_SAR) (stop and reverse) is a price-time based indicator used to determine trend direction and reversals.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/245 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="ParabolicSar" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax (standard)
IReadOnlyList<ParabolicSarResult> results =
  bars.ToParabolicSar(accelerationStep, maxAccelerationFactor);

// alternate usage with custom initial Factor
IReadOnlyList<ParabolicSarResult> results =
  bars.ToParabolicSar(accelerationStep, maxAccelerationFactor, initialFactor);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
ParabolicSarList psarList = new(accelerationStep, maxAccelerationFactor);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => psarList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<ParabolicSarResult> results = psarList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
ParabolicSarHub observer = barHub.ToParabolicSarHub(accelerationStep, maxAccelerationFactor);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ParabolicSarResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `accelerationStep`<br>_`double`_ | Incremental step size for the Acceleration Factor.  Must be greater than 0.  Default is 0.02 |
| `maxAccelerationFactor`<br>_`double`_ | Maximum factor limit.  Must be greater than `accelerationStep`.  Default is 0.2 |
| `initialFactor`<br>_`double`_ | Optional.  Initial Acceleration Factor.  Must be greater than 0 and not larger than `maxAccelerationFactor`.  Default is `accelerationStep`. |

### Historical price bars requirements

You must have at least two historical price bars to cover the warmup periods; however, we recommend at least 100 data points.  Initial Parabolic SAR values prior to the first reversal are not accurate and are excluded from the results.  Therefore, provide sufficient bars to capture prior trend reversals, before your intended usage period.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<ParabolicSarResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first trend will have `null` values since it is not accurate and based on an initial guess.
:::

::: buffer

```csharp
ParabolicSarList : IReadOnlyList<ParabolicSarResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first trend will have `null` values since it is not accurate and based on an initial guess.
:::

::: stream

```csharp
IReadOnlyList<ParabolicSarResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first trend will have `null` values since it is not accurate and based on an initial guess.
:::

### `ParabolicSarResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Sar`<br>_`double?`_ | Stop and Reverse value |
| `IsReversal`<br>_`bool?`_ | Indicates a trend reversal |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Sar` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToParabolicSar(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Sar` further.

```csharp
// example
ParabolicSarList psarList = new(..);
EmaList emaList = new(..);

void OnBarReceived(IBar bar)
{
    psarList.Add(bar);
    emaList.Add(psarList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Sar` further.

```csharp
// example
EmaHub observer = barHub
    .ToParabolicSarHub(..)
    .ToEmaHub(..);
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

Call these on the list itself, as in `psarList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `psarList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
