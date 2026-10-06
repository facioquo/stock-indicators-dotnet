---
title: Awesome Oscillator (AO)
description: Created by Bill Williams, the Awesome Oscillator (AO), also known as Super AO, is a measure of the gap between a fast and slow period modified moving average.
---

# Awesome Oscillator (AO)

Created by Bill Williams, the Awesome Oscillator (aka Super AO) is a measure of the gap between a fast and slow period modified moving average.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/282 "Community discussion about this indicator")

<StockIndicatorChart indicator="Awesome" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<AwesomeResult> results =
  bars.ToAwesome(fastPeriods, slowPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `fastPeriods` | _`int`_ | Number of periods (`F`) for the faster moving average.  Must be greater than 0.  Default is 5. |
| `slowPeriods` | _`int`_ | Number of periods (`S`) for the slower moving average.  Must be greater than `fastPeriods`.  Default is 34. |

### Historical price bars requirements

You must have at least `S` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AwesomeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period `S-1` periods will have `null` values since there's not enough data to calculate.

### `AwesomeResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Oscillator` | _`double`_ | Awesome Oscillator |
| `Normalized` | _`double`_ | `100 × Oscillator ÷ (median price)` |

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
    .ToAwesome(..);
```

Results can be further processed on `Oscillator` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAwesome(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
AwesomeList awesomeList = new(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => awesomeList.Add(bar);

// based on `ICollection<AwesomeResult>`; fills as the handler runs
IReadOnlyList<AwesomeResult> results = awesomeList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AwesomeHub observer = barHub.ToAwesomeHub(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AwesomeResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
