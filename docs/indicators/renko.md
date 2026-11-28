---
title: Renko Chart
description: The Renko Chart is a Japanese price transformed candlestick pattern that uses "bricks" to show a defined increment of change over a non-linear time series.  Transitions can use either Close or High/Low price values.  An Average True Range (ATR) variant is also provided where brick size is determined by current Average True Range values.
---

# Renko Chart

The [Renko Chart](https://en.m.wikipedia.org/wiki/Renko_chart) is a Japanese price transformed candlestick pattern that uses "bricks" to show a defined increment of change over a non-linear time series.  Transitions can use either `Close` or `High/Low` price values.  An ATR variant is also provided where brick size is determined by current Average True Range values. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/478 "Community discussion about this indicator")

<StyleSwitch />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax (fixed brick size)
IReadOnlyList<RenkoResult> results =
  bars.ToRenko(brickSize, endType);

// C# usage syntax (ATR-derived brick size — Series only)
IReadOnlyList<RenkoResult> results =
  bars.ToRenkoAtr(atrPeriods, endType);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
RenkoList renkoList = new(brickSize, endType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => renkoList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<RenkoResult> results = renkoList;
```

::: warning 🚩 Fixed brick size only
The ATR variant, `ToRenkoAtr()`, has no buffer list.  Its brick size is derived from the full dataset and changes as new bars are added, making incremental output undefined.  Use the Series implementation with periodic recalculation instead.
:::

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
RenkoHub observer = barHub.ToRenkoHub(brickSize, endType);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<RenkoResult> results = observer.Results;
```

::: warning 🚩 Fixed brick size only
The ATR variant, `ToRenkoAtr()`, has no stream hub.  Its brick size is derived from the full dataset and changes as new bars are added, making incremental output undefined.  Use the Series implementation with periodic recalculation instead.
:::

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `endType`<br>_`EndType`_ | Price threshold used to spawn new bricks, on both variants.  Default is `EndType.Close` |

### Fixed brick size

| param | description |
| ----- | ----------- |
| `brickSize`<br>_`decimal`_ | Brick size.  Must be greater than 0. |

### ATR-derived brick size

| param | description |
| ----- | ----------- |
| `atrPeriods`<br>_`int`_ | Number of lookback periods (`A`) for ATR evaluation.  Must be greater than 1.  Default is 14. |

### Historical price bars requirements

**Fixed brick size**: You must have at least two periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

**ATR-derived brick size**: You must have at least `A+100` periods of `bars`.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

::: series

```csharp
IReadOnlyList<RenkoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It does not return a single incremental indicator value.
- `RenkoResult` is based on `IBar`, so it can be used as a direct replacement for `bars`.
- Each result record represents one Renko brick.
:::

::: buffer

```csharp
RenkoList : IReadOnlyList<RenkoResult>
```

- The list itself holds the results, one for each brick, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- `RenkoResult` is based on `IBar`, so it can be used as a direct replacement for `bars`.
:::

::: stream

```csharp
IReadOnlyList<RenkoResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each brick, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- `RenkoHub` is a bar provider, so other indicator hubs can subscribe to it as they would to a `BarHub`.
:::

::: warning 🚩 Fewer results than bars
Unlike most indicators in this library, this indicator DOES NOT return the same number of elements as there are in the historical price bars.  Renko bricks are added to the results once the `brickSize` change is achieved.  For example, if it takes 3 days for a $2.50 price change to occur an entry is made on the third day while the first two are skipped.  If a period change occurs at multiples of `brickSize`, multiple bricks are drawn with the same `Timestamp`.  See [online documentation](https://www.investopedia.com/terms/r/renkochart.asp) for more information.
:::

::: warning 🖌️ Repaint warning _(ATR variant)_
When using the `ToRenkoAtr()` variant, the last [Average True Range (ATR)](/indicators/atr) value is used to set `brickSize`.  Since the ATR changes over time, historical bricks will be repainted as new periods are added or updated in `bars`.
:::

### `RenkoResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Formation date of brick(s) |
| `Open`<br>_`decimal`_ | Brick open price |
| `High`<br>_`decimal`_ | Highest high during elapsed `bars` periods |
| `Low`<br>_`decimal`_ | Lowest low during elapsed `bars` periods |
| `Close`<br>_`decimal`_ | Brick close price |
| `Volume`<br>_`decimal`_ | Sum of Volume over elapsed `bars` periods |
| `IsUp`<br>_`bool`_ | Direction of brick (true=up,false=down) |

::: warning 🚩 Multiple bricks per period
When multiple bricks are drawn from a single `bar` period, the extra information about `High` and `Low` wicks and `Volume` is potentially confusing to interpret.  `High` and `Low` wicks will be the same across the multiple bricks; and `Volume` is portioning evenly across the number of bricks.  For example, if within one `bar` period 3 bricks are drawn, the `Volume` for each brick will be `(sum of bars Volume since last brick) / 3`.
:::

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results are based in `IBar` and can be further used in any indicator.

```csharp
// example
var results = bars
    .ToRenko(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Results are based in `IBar`.  One bar can add several bricks or none, so add each new brick to the next buffer list yourself.

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Results are based in `IBar`, so any indicator hub can subscribe to the Renko hub.

```csharp
// example
RsiHub observer = barHub
    .ToRenkoHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on the list itself, as in `renkoList.Find(lookupDate)`.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)

Call it on `observer.Results`, as in `observer.Results.Find(lookupDate)`.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
