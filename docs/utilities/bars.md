---
title: Bar utilities
description: Utilities for preparing and transforming historical price bars before using them with indicators.
---

# Bar utilities

Utilities for preparing and transforming historical price bars before using them with indicators.

## Use alternate price

`bars.Use()` selects which price element to analyze before most indicator calls — for example the median (`HL2`) or typical (`HLC3`) price instead of `Close`. It cannot be used with indicators that require the full OHLCV bar profile.

```csharp
// median price (HL2) into an SMA
var results = bars
  .Use(CandlePart.HL2)
  .ToSma(20);
```

**`candlePart`** — the price element to use for calculations.

<!--@include: ../shared/enum-candlepart.md-->

::: warning 🚩 Incompatible indicators
Some indicators require the full OHLCV bar profile and cannot be used with `.Use()`:

- Indicators that explicitly use multiple price points (ATR, Stochastic, etc.)
- Volume-based indicators (OBV, CMF, etc.) unless using `CandlePart.Volume`
- Candlestick pattern indicators
:::

The bar-part selector also has incremental and live streaming variants that emit `TimeValue` results:

```csharp
// BufferList — incremental, single-threaded
BarPartList parts = bars.ToBarPartList(CandlePart.HL2);

// StreamHub — chain into a live pipeline
BarHub barHub = new();
BarPartHub partHub = barHub.ToBarPartHub(CandlePart.HLC3);
EmaHub emaHub = partHub.ToEmaHub(20);  // chain downstream indicators
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.

## Sort bars

`bars.ToSortedList()` sorts any collection of `TBar` (or `ISeries`) and returns an `IReadOnlyList` ordered by ascending `Timestamp`. Library indicators expect chronological bars, so use this when you cannot guarantee the sort order from your data source.

```csharp
// sort, then use inline with indicators
var results = bars
  .ToSortedList()
  .ToRsi(14);
```

::: info Pre-sorted data
Most providers already return bars in chronological order. Sorting large datasets has a cost, so skip this step when the order is guaranteed.
:::

## Validate bar history

`bars.Validate()` is an advanced check of your `IReadOnlyList<IBar>`. It detects duplicate timestamps and out-of-sequence (non-ascending) dates, throwing an `InvalidBarsException` if either is found. On success it returns the validated collection, so it can be used inline.

```csharp
try
{
  var results = bars
    .Validate()
    .ToRsi(14);
}
catch (InvalidBarsException ex)
{
  Console.WriteLine($"Invalid bars: {ex.Message}");
}
```

::: info When to use
`.Validate()` performs thorough checks and has a cost. Use it when data quality is uncertain — importing from untrusted sources, production data-quality gates, or debugging unexpected results — but avoid re-validating the same dataset repeatedly. To simply fix ordering, use [Sort bars](#sort-bars) instead.
:::

## Resize bar history

`bars.Aggregate()` combines intraday bars into larger timeframes — minute to hourly, hourly to daily, or any custom `TimeSpan`.

```csharp
// using the BarInterval enum
IReadOnlyList<Bar> hourlyBars = minuteBars.Aggregate(BarInterval.OneHour);

// using a custom TimeSpan
IReadOnlyList<Bar> customBars = minuteBars.Aggregate(TimeSpan.FromMinutes(45));
```

| param | type | description |
| ----- | ---- | ----------- |
| `newSize` | `BarInterval` | Target period size: `Month`, `Week`, `Day`, `FourHours`, `TwoHours`, `OneHour`, `ThirtyMinutes`, `FifteenMinutes`, `FiveMinutes`, `ThreeMinutes`, `TwoMinutes`, `OneMinute`. |
| `timeSpan` | `TimeSpan` | Any value greater than `TimeSpan.Zero`, for custom periods. |

Each aggregated bar takes the first **Open**, highest **High**, lowest **Low**, last **Close**, summed **Volume**, and the period's starting **Timestamp**.

Periods are fixed calendar buckets, not rolling windows, in the clock your timestamps already use; no time zone is applied. `Month` starts on the 1st of each month. `Week` starts Monday 00:00 and runs through Sunday. `Day` and shorter sizes start at midnight. A custom `TimeSpan` counts its buckets from `DateTime.MinValue`, so a size that does not divide 24 hours evenly, such as 7 minutes, does not restart at midnight. The streaming aggregator hubs use the same boundaries for every size except `Month`, which they do not support.

::: warning 🚩 Partially populated periods
Partial period windows at the start, end, or market open/close can be misleading. For example, a lone 4:00pm minute bar aggregated into 15-minute bars yields a 4:00pm bar holding just one minute of data, while the prior 3:45pm bar holds the full 15 minutes. Filter out partial periods if they could skew your analysis.
:::

### Streaming aggregator hubs

For live feeds, the aggregator hubs convert small bars or raw ticks into larger period bars in real time. Both accept a `BarInterval` or custom `TimeSpan` (`BarInterval.Month` is not supported in streaming — use the `TimeSpan` overload):

```csharp
// bar → bar (e.g. 1-minute bars to 5-minute bars)
BarHub barHub = new();
BarAggregatorHub fiveMinHub = barHub.ToBarAggregatorHub(BarInterval.FiveMinutes);

// tick → bar (raw trades to 1-minute OHLCV)
TradeTickHub tickHub = new();
TradeTickAggregatorHub oneMinHub = tickHub.ToTradeTickAggregatorHub(BarInterval.OneMinute);
```

Both aggregator hubs accept a `GapFillMode` to control how silent buckets — periods with no upstream input — are handled:

| value | behavior |
| ----- | -------- |
| `None` (default) | Silent buckets are omitted from the output stream. |
| `ForwardFill` | Silent buckets are filled with zero-volume bars, carrying the prior bar's `Close` into `Open`, `High`, `Low`, and `Close`. |
| `Interpolate` | Silent buckets are filled with zero-volume bars whose `Open`, `High`, `Low`, and `Close` are linearly interpolated between the prior bar's `Close` and the next real bar's `Open` (the next real tick's price, for ticks), evenly spaced across the missing buckets. |

```csharp
// bar → bar
BarAggregatorHub fiveMinHub = barHub.ToBarAggregatorHub(BarInterval.FiveMinutes, GapFillMode.Interpolate);

// tick → bar
TradeTickAggregatorHub oneMinHub = tickHub.ToTradeTickAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
```

An optional `fillGaps` boolean overload also remains for convenience: `fillGaps: false` (default) is equivalent to `GapFillMode.None`, and `fillGaps: true` is equivalent to `GapFillMode.ForwardFill`. Volume is always `0` on a synthesized bar, regardless of mode. `Interpolate` prices are not rounded; a quotient that does not terminate (e.g. two silent buckets, which divide the move into thirds) carries full `decimal` precision.

`Interpolate` also looks ahead: each synthesized bar uses the next real bar's `Open`, so its value is not knowable at its own timestamp. A backtest that trades on interpolated bars acts on data it could not have had yet; use `ForwardFill` when look-ahead matters. When a late or revised input changes that next bar, the hub regenerates the affected gap bars, so revising the `Open` of the bar after a long gap costs time proportional to the gap length.

## Extended candle properties

`bar.ToCandle()` and `bars.ToCandles()` convert a bar into an extended `CandleProperties` format with additional calculated candle measurements (body size, wicks, etc.).

```csharp
// single bar
CandleProperties candle = bar.ToCandle();

// collection of bars
IReadOnlyList<CandleProperties> candles = bars.ToCandles();

// access extended properties
decimal? bodySize = candle.Body;
decimal? upperWick = candle.UpperWick;
bool isBullish = candle.IsBullish;
```

<!--@include: ../shared/candle-properties.md-->

## See also

- [Result utilities](/utilities/results) — work with indicator results after calculation
- [Additional helper utilities](/utilities/helpers) — math and numerical methods for custom indicators
- [Indicator catalog](/utilities/catalog) — discover indicator metadata programmatically
