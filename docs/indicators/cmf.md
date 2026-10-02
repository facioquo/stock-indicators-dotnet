---
title: Chaikin Money Flow (CMF)
description: Created by Marc Chaikin, Chaikin Money Flow is the simple moving average of the directional Money Flow Volume.
---

# Chaikin Money Flow (CMF)

Created by Marc Chaikin, [Chaikin Money Flow](https://en.wikipedia.org/wiki/Chaikin_Analytics#Chaikin_Money_Flow) is the simple moving average of the directional Money Flow Volume.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/261 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Cmf" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<CmfResult> results =
  bars.ToCmf(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0.  Default is 20. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<CmfResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `CmfResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `MoneyFlowMultiplier` | _`double`_ | Money Flow Multiplier |
| `MoneyFlowVolume` | _`double`_ | Money Flow Volume |
| `Cmf` | _`double`_ | Chaikin Money Flow = SMA of MFV |

::: warning 🚩
absolute values in MFV and CMF are somewhat meaningless.  Use with caution.
:::

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Cmf` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToCmf(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
CmfList cmfList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => cmfList.Add(bar);

// based on `ICollection<CmfResult>`; fills as the handler runs
IReadOnlyList<CmfResult> results = cmfList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
CmfHub observer = barHub.ToCmfHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<CmfResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
