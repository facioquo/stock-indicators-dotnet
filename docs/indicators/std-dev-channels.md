---
title: Standard Deviation Channels
description: Standard Deviation Channels are price ranges based on an linear regression centerline and standard deviations band widths.
---

# Standard Deviation Channels

Standard Deviation Channels are prices ranges based on an linear regression centerline and standard deviations band widths.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/368 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="StdDevChannels" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<StdDevChannelsResult> results =
  bars.ToStdDevChannels(lookbackPeriods, stdDeviations);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Size (`N`) of the evaluation window.  Must be `null` or greater than 1 to calculate.  A `null` value will produce a full `bars` evaluation window ([see below](#alternative-depiction-for-full-bars-variant)).  Default is 20. |
| `stdDeviations` | _`double`_ | Width of bands.  Standard deviations (`D`) from the regression line.  Must be greater than 0.  Default is 2. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<StdDevChannelsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- Up to `N-1` periods will have `null` values since there's not enough data to calculate.

::: warning 🖌️ Repaint warning
Historical results are a function of the current period window position and will fluctuate over time.  Recommended for visualization; not recommended for backtesting.
:::

### `StdDevChannelsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Centerline` | _`double`_ | Linear regression line (center line) |
| `UpperChannel` | _`double`_ | Upper line is `D` standard deviations above the center line |
| `LowerChannel` | _`double`_ | Lower line is `D` standard deviations below the center line |
| `BreakPoint` | _`bool`_ | Helper information.  Indicates first point in new window. |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Alternative depiction for full bars variant

If you specify `null` for the `lookbackPeriods`, you will get a regression line over the entire provided `bars`.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = barsEval
    .Use(CandlePart.HL2)
    .ToStdDevChannels(..);
```

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

**⚠️ Streaming not supported**: Due to the reverse-window algorithm that recalculates the entire dataset on each new data point, Standard Deviation Channels is only available as a batch Series implementation. The computational cost grows quadratically (O(n²)) as the dataset size increases, making it impractical for incremental streaming (StreamHub) or buffer (BufferList) scenarios.

**Recommendation**: Use the Series implementation (`ToStdDevChannels()`) with periodic batch recalculation. For real-time scenarios, consider recalculating at appropriate intervals (e.g., end of period, every N bars) rather than on every tick.
