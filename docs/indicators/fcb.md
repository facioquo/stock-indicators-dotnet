---
title: Fractal Chaos Bands (FCB)
description: Created by Edward William Dreiss, Fractal Chaos Bands outline high and low price channels to depict broad less-chaotic price movements.  FCB is a channelized depiction of Williams Fractal.
---

# Fractal Chaos Bands (FCB)

Created by Edward William Dreiss, Fractal Chaos Bands outline high and low price channels to depict broad less-chaotic price movements.  FCB is a channelized depiction of [Williams Fractal](/indicators/fractal).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/347 "Community discussion about this indicator")

<StockIndicatorChart indicator="Fcb" />

```csharp
// C# usage syntax
IReadOnlyList<FcbResult> results =
  bars.ToFcb(windowSpan);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `windowSpan` | _`int`_ | Fractal evaluation window span width (`S`).  Must be at least 2.  Default is 2. |

The total evaluation window size is `2×S+1`, representing `±S` from the evaluation date.  See [Williams Fractal](/indicators/fractal) for more information about Fractals and `windowSpan`.

### Historical price bars requirements

You must have at least `2×S+1` periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<FcbResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The periods before the first fractal are `null` since they cannot be calculated.

::: warning 🖌️ Repaint warning
Fractal Chaos Bands are based on [Williams Fractal](/indicators/fractal), which uses future bars.  This indicator will never identify bands in the last `S` periods of `bars` since fractals are retroactively identified.
:::

### `FcbResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `UpperBand` | _`decimal`_ | FCB upper band |
| `LowerBand` | _`decimal`_ | FCB lower band |

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
FcbList fcbList = new(windowSpan);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => fcbList.Add(bar);

// based on `ICollection<FcbResult>`; fills as the handler runs
IReadOnlyList<FcbResult> results = fcbList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
FcbHub observer = barHub.ToFcbHub(windowSpan);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<FcbResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
