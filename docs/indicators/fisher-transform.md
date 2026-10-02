---
title: Ehlers Fisher Transform
description: Created by John Ehlers, the Fisher Transform converts financial market prices into a Gaussian normal distribution.
---

# Ehlers Fisher Transform

Created by John Ehlers, the [Fisher Transform](https://www.investopedia.com/terms/f/fisher-transform.asp) converts prices into a Gaussian normal distribution.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/409 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="FisherTransform" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<FisherTransformResult> results =
  bars.ToFisherTransform(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback window.  Must be greater than 0.  Default is 10. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<FisherTransformResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.

::: warning 🚩 ⚞ Convergence warning
The first `N+15` warmup periods will have unusable decreasing magnitude, convergence-related precision errors that can be as high as ~25% deviation in earlier indicator values.
:::

### `FisherTransformResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Fisher` | _`double`_ | Fisher Transform |
| `Trigger` | _`double`_ | FT offset by one period |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

For pruning of warmup periods, we recommend using the following guidelines:

```csharp
bars.ToFisherTransform(lookbackPeriods)
  .RemoveWarmupPeriods(lookbackPeriods+15);
```

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToFisherTransform(..);
```

Results can be further processed on `Alma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToFisherTransform(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
FisherTransformList fisherList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => fisherList.Add(bar);

// based on `ICollection<FisherTransformResult>`
IReadOnlyList<FisherTransformResult> results = fisherList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
FisherTransformHub observer = barHub.ToFisherTransformHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<FisherTransformResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
