---
title: Chandelier Exit
description: Created by Charles Le Beau, the Chandelier Exit is an adjusted Average True Range (ATR) offset from price that is typically used for stop-loss and can be computed for both long or short types.
---

# Chandelier Exit

Created by Charles Le Beau, the [Chandelier Exit](https://www.google.com/search?q=Chandelier+Exit+indicator) is an adjusted Average True Range (ATR) offset from price that is typically used for stop-loss and can be computed for both long or short types.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/263 "Community discussion about this indicator")

<StockIndicatorChart indicator="Chandelier" />

```csharp
// C# usage syntax
IReadOnlyList<ChandelierResult> results =
  bars.ToChandelier(lookbackPeriods, multiplier, type);
```

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the lookback evaluation.  Must be greater than 1.  Default is 22. |
| `multiplier`<br>_`double`_ | Multiplier number must be a positive value.  Default is 3. |
| `type`<br>_`Direction`_ | Direction of exit. Default is `Direction.Long`. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-direction.md-->

## Response

```csharp
IReadOnlyList<ChandelierResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` Chandelier values since there's not enough data to calculate.

### `ChandelierResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `ChandelierExit`<br>_`double?`_ | Exit line |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `ChandelierExit` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToChandelier(..)
    .ToEma(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
ChandelierList chandelierList = new(lookbackPeriods, multiplier, type);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => chandelierList.Add(bar);

// based on `IReadOnlyList<ChandelierResult>`; fills as the handler runs
IReadOnlyList<ChandelierResult> results = chandelierList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
ChandelierHub observer = barHub.ToChandelierHub(lookbackPeriods, multiplier, type);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ChandelierResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
