---
title: Williams %R
description: Created by Larry Williams, the Williams %R momentum oscillator compares current price with recent highs and lows and is presented on scale of -100 to 0.  It is exactly the same as the fast variant of Stochastic Oscillator, but with a different scaling.
---

# Williams %R

Created by Larry Williams, the [Williams %R](https://en.wikipedia.org/wiki/Williams_%25R) momentum oscillator compares current price with recent highs and lows and is presented on scale of -100 to 0.  It is exactly the same as the fast variant of [Stochastic Oscillator](/indicators/stoch), but with a different scaling.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/229 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="WilliamsR" withOverlay     showRightAxisLabels=false />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<WilliamsResult> results =
  bars.ToWilliamsR(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<WilliamsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` Oscillator values since there's not enough data to calculate.

### `WilliamsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `WilliamsR` | _`double`_ | Oscillator over prior `N` lookback periods |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `WilliamsR` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToWilliamsR(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
WilliamsRList williamsRList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => williamsRList.Add(bar);

// based on `ICollection<WilliamsResult>`
IReadOnlyList<WilliamsResult> results = williamsRList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
WilliamsRHub observer = barHub.ToWilliamsRHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<WilliamsResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
