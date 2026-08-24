---
title: Renko Chart
description: The Renko Chart is a Japanese price transformed candlestick pattern that uses "bricks" to show a defined increment of change over a non-linear time series.  Transitions can use either Close or High/Low price values.  An Average True Range (ATR) variant is also provided where brick size is determined by current Average True Range values.
---

# Renko Chart

The [Renko Chart](https://en.m.wikipedia.org/wiki/Renko_chart) is a Japanese price transformed candlestick pattern that uses "bricks" to show a defined increment of change over a non-linear time series.  Transitions can use either `Close` or `High/Low` price values.  An ATR variant is also provided where brick size is determined by current Average True Range values. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/478 "Community discussion about this indicator")

```csharp
// C# usage syntax (fixed brick size)
IReadOnlyList<RenkoResult> results =
  bars.ToRenko(brickSize, endType);

// C# usage syntax (ATR-derived brick size — Series only)
IReadOnlyList<RenkoResult> results =
  bars.ToRenkoAtr(atrPeriods, endType);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `endType` | _`EndType`_ | Price threshold used to spawn new bricks, on both variants.  Default is `EndType.Close` |

### Fixed brick size

| param | type | description |
| ----- | ---- | ----------- |
| `brickSize` | _`decimal`_ | Brick size.  Must be greater than 0. |

### ATR-derived brick size

| param | type | description |
| ----- | ---- | ----------- |
| `atrPeriods` | _`int`_ | Number of lookback periods (`A`) for ATR evaluation.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

**Fixed brick size**: You must have at least two periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

**ATR-derived brick size**: You must have at least `A+100` periods of `bars`.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

```csharp
IReadOnlyList<RenkoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It does not return a single incremental indicator value.
- `RenkoResult` is based on `IBar`, so it can be used as a direct replacement for `bars`.
- Each result record represents one Renko brick.

::: warning 🚩
Unlike most indicators in this library, this indicator DOES NOT return the same number of elements as there are in the historical price bars.  Renko bricks are added to the results once the `brickSize` change is achieved.  For example, if it takes 3 days for a $2.50 price change to occur an entry is made on the third day while the first two are skipped.  If a period change occurs at multiples of `brickSize`, multiple bricks are drawn with the same `Timestamp`.  See [online documentation](https://www.investopedia.com/terms/r/renkochart.asp) for more information.
:::

::: warning ️🖌️ Repaint warning _(ATR variant)_
When using the `ToRenkoAtr()` variant, the last [Average True Range (ATR)](/indicators/atr) value is used to set `brickSize`.  Since the ATR changes over time, historical bricks will be repainted as new periods are added or updated in `bars`.
:::

### `RenkoResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Formation date of brick(s) |
| `Open` | _`decimal`_ | Brick open price |
| `High` | _`decimal`_ | Highest high during elapsed `bars` periods |
| `Low` | _`decimal`_ | Lowest low during elapsed `bars` periods |
| `Close` | _`decimal`_ | Brick close price |
| `Volume` | _`decimal`_ | Sum of Volume over elapsed `bars` periods |
| `IsUp` |  _`bool`_ | Direction of brick (true=up,false=down) |

::: warning 🚩
When multiple bricks are drawn from a single `bar` period, the extra information about `High` and `Low` wicks and `Volume` is potentially confusing to interpret.  `High` and `Low` wicks will be the same across the multiple bricks; and `Volume` is portioning evenly across the number of bricks.  For example, if within one `bar` period 3 bricks are drawn, the `Volume` for each brick will be `(sum of bars Volume since last brick) / 3`.
:::

### Utilities

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results are based in `IBar` and can be further used in any indicator.

```csharp
// example
var results = bars
    .ToRenko(..)
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

**Fixed brick size only** — Streaming implementations are available for the fixed brick size variant only.

Use a `BufferList` for incremental processing:

```csharp
RenkoList buffer = new(brickSize, endType);

foreach (IBar bar in bars)  // simulating incremental data
{
  buffer.Add(bar);
}

IReadOnlyList<RenkoResult> results = buffer;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
RenkoHub observer = barHub.ToRenkoHub(brickSize);

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<RenkoResult> results = observer.Results;
```

::: warning 🚩
`ToRenkoAtr()` does not support streaming. The ATR brick size is derived from the full dataset and changes as new bars are added, making incremental output undefined. Use the Series implementation with periodic recalculation instead.
:::

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
