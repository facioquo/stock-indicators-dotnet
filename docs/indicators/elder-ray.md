---
title: Elder-ray Index
description: Created by Alexander Elder, the Elder-ray Index, also known as Bull and Bear Power, is an oscillator that depicts buying and selling pressure.  It compares current high/low prices against an Exponential Moving Average.
---

# Elder-ray Index

Created by Alexander Elder, the [Elder-ray Index](https://www.investopedia.com/terms/e/elderray.asp), also known as Bull and Bear Power, is an oscillator that depicts buying and selling pressure.  It compares current high/low prices against an Exponential Moving Average.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/378 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="ElderRay" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<ElderRayResult> results =
  bars.ToElderRay(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) for the underlying EMA evaluation.  Must be greater than 0.  Default is 13. |

### Historical price bars requirements

You must have at least `2×N` or `N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<ElderRayResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` indicator values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ElderRayResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Ema` | _`double`_ | Exponential moving average |
| `BullPower` | _`double`_ | Bull Power |
| `BearPower` | _`double`_ | Bear Power |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `(BullPower+BearPower)` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToElderRay(..)
    .ToEma(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ElderRayList elderRayList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => elderRayList.Add(bar);

// based on `ICollection<ElderRayResult>`
IReadOnlyList<ElderRayResult> results = elderRayList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ElderRayHub observer = barHub.ToElderRayHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<ElderRayResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
