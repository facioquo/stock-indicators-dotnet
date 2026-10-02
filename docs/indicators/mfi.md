---
title: Money Flow Index (MFI)
description: Created by Quong and Soudack, the Money Flow Index is a price-volume oscillator that shows buying and selling momentum.
---

# Money Flow Index (MFI)

Created by Quong and Soudack, the [Money Flow Index](https://en.wikipedia.org/wiki/Money_flow_index) is a price-volume oscillator that shows buying and selling momentum. Values outside of the 80 / 20 thresholds are considered overbought / oversold.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/247 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Mfi" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<MfiResult> results =
  bars.ToMfi(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 1. Default is 14. |

### Historical price bars requirements

You must have at least `N+1` historical price bars to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<MfiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` MFI values since they cannot be calculated.

### `MfiResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Mfi` | _`double`_ | Money Flow Index |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
MfiList mfiList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => mfiList.Add(bar);

// based on `ICollection<MfiResult>`
IReadOnlyList<MfiResult> results = mfiList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
MfiHub observer = barHub.ToMfiHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<MfiResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.

## Chaining

Results can be further processed on `Mfi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToMfi(..)
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.
