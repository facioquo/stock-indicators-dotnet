---
title: MESA Adaptive Moving Average (MAMA)
description: Created by John Ehlers, the MAMA indicator is a 5-period adaptive moving average of high/low price that uses classic electrical radio-frequency signal processing algorithms to reduce noise.
---

# MESA Adaptive Moving Average (MAMA)

Created by John Ehlers, the [MAMA](https://mesasoftware.com/papers/MAMA.pdf) indicator is a 5-period adaptive moving average of high/low price that uses classic electrical radio-frequency signal processing algorithms to reduce noise.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/211 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Mama" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<MamaResult> results =
  bars.ToMama(fastLimit, slowLimit);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `fastLimit` | _`double`_ | Fast limit threshold.  Must be greater than `slowLimit` and less than 1.  Default is 0.5. |
| `slowLimit` | _`double`_ | Slow limit threshold.  Must be greater than 0.  Default is 0.05. |

### Historical price bars requirements

You must have at least `50` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<MamaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `5` periods will have `null` values for `Mama` since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `50` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `MamaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Mama` | _`double`_ | MESA adaptive moving average (MAMA) |
| `Fama` | _`double`_ | Following adaptive moving average (FAMA) |

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
    .Use(CandlePart.HL2)
    .ToMama(..);
```

Results can be further processed on `Mama` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToMama(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
MamaList mamaList = new(fastLimit, slowLimit);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => mamaList.Add(bar);

// based on `ICollection<MamaResult>`; fills as the handler runs
IReadOnlyList<MamaResult> results = mamaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
MamaHub observer = barHub.ToMamaHub(fastLimit, slowLimit);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MamaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
