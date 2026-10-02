---
title: On-Balance Volume (OBV)
description: Popularized by Joseph Granville, On-balance Volume is a rolling accumulation of volume based on Close price direction.
---

# On-Balance Volume (OBV)

Popularized by Joseph Granville, [On-balance Volume](https://en.wikipedia.org/wiki/On-balance_volume) is a rolling accumulation of volume based on Close price direction.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/246 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Obv" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<ObvResult> results =
  bars.ToObv();
```

## Historical price bars requirements

You must have at least two historical price bars to cover the warmup periods; however, since this is a trendline, more is recommended.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<ObvResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period OBV will have a `0` value since there's not enough data to calculate.

### `ObvResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Obv` | _`double`_ | On-balance Volume |

::: warning 🚩
absolute values in OBV are somewhat meaningless. Use with caution.
:::

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Obv` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToObv(..)
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ObvList obvList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => obvList.Add(bar);

// based on `ICollection<ObvResult>`; fills as the handler runs
IReadOnlyList<ObvResult> results = obvList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ObvHub observer = barHub.ToObvHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ObvResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
