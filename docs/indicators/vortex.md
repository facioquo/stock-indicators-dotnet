---
title: Vortex Indicator (VI)
description: Created by Etienne Botes and Douglas Siepman, the Vortex Indicator is a measure of price directional movement.  It includes positive and negative indicators, and is often used to identify trends and reversals.
---

# Vortex Indicator (VI)

Created by Etienne Botes and Douglas Siepman, the [Vortex Indicator](https://en.wikipedia.org/wiki/Vortex_indicator) is a measure of price directional movement.  It includes positive and negative indicators, and is often used to identify trends and reversals.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/339 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Vortex" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<VortexResult> results =
  bars.ToVortex(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) to consider.  Must be greater than 1 and is usually between 14 and 30. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<VortexResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for VI since there's not enough data to calculate.

### `VortexResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Pvi` | _`double`_ | Positive Vortex Indicator (VI+) |
| `Nvi` | _`double`_ | Negative Vortex Indicator (VI-) |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
VortexList vortexList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => vortexList.Add(bar);

// based on `ICollection<VortexResult>`; fills as the handler runs
IReadOnlyList<VortexResult> results = vortexList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
VortexHub observer = barHub.ToVortexHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<VortexResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
