---
title: Force Index
description: Created by Alexander Elder, the Force Index depicts volume-based buying and selling pressure based on the change in price.
---

# Force Index

Created by Alexander Elder, the [Force Index](https://en.wikipedia.org/wiki/Force_index) depicts volume-based buying and selling pressure based on the change in price.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/382 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="ForceIndex" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<ForceIndexResult> results =
  bars.ToForceIndex(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Lookback window (`N`) for the EMA of Force Index.  Must be greater than 0 and is commonly 2 or 13 (shorter/longer view).  Default is 2. |

### Historical price bars requirements

You must have at least `N+100` for `2×N` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique for EMA, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<ForceIndexResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will be `null` since they cannot be calculated.

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `ForceIndexResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `ForceIndex` | _`double`_ | Force Index |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `ForceIndex` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToForceIndex(..)
    .ToEma(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ForceIndexList forceIndexList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => forceIndexList.Add(bar);

// based on `ICollection<ForceIndexResult>`
IReadOnlyList<ForceIndexResult> results = forceIndexList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<ForceIndexResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
