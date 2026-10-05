---
title: Accumulation / Distribution Line (ADL)
description: Created by Marc Chaikin, the Accumulation / Distribution Line is a rolling accumulation of Chaikin Money Flow Volume.  It can be a leading momentum indicator for financial market price movements.
---

# Accumulation / Distribution Line (ADL)

Created by Marc Chaikin, the [Accumulation/Distribution Line/Index](https://en.wikipedia.org/wiki/Accumulation/distribution_index) is a rolling accumulation of Chaikin Money Flow Volume.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/271 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Adl" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<AdlResult> results =
  bars.ToAdl();
```

## Historical price bars requirements

You must have at least two historical price bars to cover the warmup periods; however, since this is a trendline, more is recommended.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AdlResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.

### `AdlResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `MoneyFlowMultiplier` | _`double`_ | Money Flow Multiplier |
| `MoneyFlowVolume` | _`double`_ | Money Flow Volume |
| `Adl` | _`double`_ | Accumulation Distribution Line (ADL) |

::: warning 🚩 Relative values only
Absolute values in ADL and MFV are somewhat meaningless.  Use with caution.
:::

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Adl` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAdl()
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
AdlList adlList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => adlList.Add(bar);

// based on `ICollection<AdlResult>`; fills as the handler runs
IReadOnlyList<AdlResult> results = adlList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AdlHub observer = barHub.ToAdlHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AdlResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
