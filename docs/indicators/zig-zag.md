---
title: Zig Zag
description: Zig Zag is a financial market price chart overlay that simplifies the up and down movements and transitions based on a percent change smoothing threshold.
---

# Zig Zag

[Zig Zag](https://www.google.com/search?q=Zig+Zag+(ZIGZAG)+indicator) is a price chart overlay that simplifies the up and down movements and transitions based on a percent change smoothing threshold. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/226 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="ZigZag" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<ZigZagResult> results =
  bars.ToZigZag(endType, percentChange);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `endType` | _`EndType`_ | Determines whether `Close` or `High/Low` are used for percent change threshold. Default is `EndType.Close`. |
| `percentChange` | _`decimal`_ | Percent change required to establish a line endpoint.  **Example: 3.5% would be entered as 3.5 (not 0.035)**.  Must be greater than 0.  Typical values range from 3 to 10.  Default is 5. |

::: tip ✨ Tip: adjust `percentChange` for bar interval sizes
Use a percent change threshold that is appropriate for your price bar interval size. For example, `5%` works well for `Day`-sized price bar aggregates, but `Hourly` or `Minute`-sized bars typically need smaller values (e.g., `1%` or `0.1%`), depending on the asset class and typical price volatility.
:::

### Historical price bars requirements

You must have at least two periods of `bars` to cover the warmup periods, but notably more is needed to be useful.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

```csharp
IReadOnlyList<ZigZagResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- If you do not supply enough points to cover the percent change, there will be no Zig Zag points or lines.
- The first line segment starts after the first confirmed point; ZigZag values before the first confirmed point will be `null`.
- The last line segment is an approximation as the direction is indeterminate.

::: warning 🚩
depending on the specified `endType`, the indicator cannot be initialized if the first `Bar` in `bars` has a `High`,`Low`, or `Close` value of 0 (zero).
:::

::: warning ️🖌️ Repaint warning
The last line segment will always be redrawn back to the last known pivot.  Do not attempt to calculate incremental values since previous values may change based on newer bars.
:::

### `ZigZagResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `ZigZag` | _`decimal`_ | Zig Zag line for `percentChange` |
| `PointType` | _`string`_ | Zig Zag endpoint type (`H` for high point, `L` for low point) |
| `RetraceHigh` | _`decimal`_ | Retrace line for high points |
| `RetraceLow` | _`decimal`_ | Retrace line for low points |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `ZigZag` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToZigZag(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Streaming is not supported for this indicator. This indicator requires lookahead to confirm reversal points; output repaints as new data arrives, making incremental results undefined. Use the Series (batch) implementation with periodic recalculation instead.
