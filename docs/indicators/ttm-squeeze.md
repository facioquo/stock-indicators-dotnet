---
title: TTM Squeeze
description: Created by John Carter, the TTM Squeeze flags low volatility periods, when the Bollinger Bands move inside the Keltner Channels, and adds a momentum histogram to show the likely direction of the breakout.
---

# TTM Squeeze

Created by John Carter, the [TTM Squeeze](https://chartschool.stockcharts.com/table-of-contents/technical-indicators-and-overlays/technical-indicators/ttm-squeeze) flags low volatility periods, when the Bollinger Bands move inside the Keltner Channels, and adds a momentum histogram to show the likely direction of the breakout.

```csharp
// C# usage syntax
IReadOnlyList<TtmSqueezeResult> results =
  bars.ToTtmSqueeze(lookbackPeriods, standardDeviations, multiplier);
```

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for the Bollinger Bands, Keltner Channels, and momentum.  Must be greater than 1.  Default is 20. |
| `standardDeviations`<br>_`double`_ | Width of the Bollinger Bands, in standard deviations (`K`).  Must be greater than 0.  Default is 2. |
| `multiplier`<br>_`double`_ | Width of the Keltner Channels, in average true ranges (`M`).  Must be greater than 0.  Default is 1.5. |

### Historical price bars requirements

You must have at least `2×N-1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<TtmSqueezeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for `IsSqueezeOn`, and the first `2×N-2` periods will have `null` values for `Momentum`, since there's not enough data to calculate.

### `TtmSqueezeResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Momentum`<br>_`double?`_ | Momentum histogram: the `N`-period linear regression of `Close` less the average of the Donchian Channel midline and the SMA, at the current period |
| `IsSqueezeOn`<br>_`bool?`_ | `true` when the Bollinger Bands (SMA ± `K` standard deviations) are inside the Keltner Channels (SMA ± `M` × the `N`-period simple average of True Range) |

Both envelopes use the `N`-period SMA of `Close` as their centerline.

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Momentum` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToTtmSqueeze(..)
    .ToEma(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
TtmSqueezeList ttmSqueezeList = new(lookbackPeriods, standardDeviations, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => ttmSqueezeList.Add(bar);

// based on `ICollection<TtmSqueezeResult>`; fills as the handler runs
IReadOnlyList<TtmSqueezeResult> results = ttmSqueezeList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
TtmSqueezeHub observer = barHub.ToTtmSqueezeHub(lookbackPeriods, standardDeviations, multiplier);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TtmSqueezeResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
