---
title: True Strength Index (TSI)
description: Created by William Blau, the True Strength Index is a momentum oscillator that uses a series of exponential moving averages to depicts trends in price changes.
---

# True Strength Index (TSI)

Created by William Blau, the [True Strength Index](https://en.wikipedia.org/wiki/True_strength_index) is a momentum oscillator that uses a series of exponential moving averages to depicts trends in price changes.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/300 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Tsi" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<TsiResult> results =
  bars.ToTsi(lookbackPeriods, smoothPeriods, signalPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) for the first EMA.  Must be greater than 0.  Default is 25. |
| `smoothPeriods` | _`int`_ | Number of periods (`M`) for the second smoothing.  Must be greater than 0.  Default is 13. |
| `signalPeriods` | _`int`_ | Number of periods (`S`) in the TSI moving average.  Must be greater than or equal to 0.  Default is 7. |

### Historical price bars requirements

You must have at least `N+M+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a two-stage EMA smoothing technique, we recommend you use at least `N+M+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<TsiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N+M-1` periods will have `null` values since there's not enough data to calculate.
- `Signal` will be `null` for all periods if `signalPeriods=0`.

::: warning 🚩 ⚞ Convergence warning
The first `N+M+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `TsiResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Tsi` | _`double`_ | True Strength Index |
| `Signal` | _`double`_ | Signal line (EMA of TSI) |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
TsiList tsiList = new(lookbackPeriods, smoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => tsiList.Add(bar);

// based on `ICollection<TsiResult>`; fills as the handler runs
IReadOnlyList<TsiResult> results = tsiList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
TsiHub observer = barHub.ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TsiResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToTsi(..);
```

Results can be further processed on `Tsi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToTsi(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
