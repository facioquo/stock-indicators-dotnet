---
title: IndyMath
description: Arithmetic and rolling-window math on chainable indicator results — add, subtract, multiply, or divide two series or a series and a fixed amount, take the larger or smaller of two series, and find a rolling highest, lowest, or median value.
---

# IndyMath

IndyMath combines or transforms chainable indicator results — any series of `IReusable` values, including price bars — so simple custom indicators need no hand-written loop.  It adds, subtracts, multiplies, or divides two series or a series and a fixed amount, takes the larger or smaller of two series, and finds a rolling highest, lowest, or median value.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/977 "Community discussion about this indicator")

```csharp
// C# usage syntax (two series)
IReadOnlyList<MathResult> results =
  basisSource.Plus(otherSource);
  // also .Minus, .Times, .DividedBy, .MaxWith, .MinWith

// with a fixed amount
IReadOnlyList<MathResult> results =
  source.Plus(amount);
  // also .Minus(amount), .Times(factor), .DividedBy(divisor)

// with a percent offset
IReadOnlyList<MathResult> results =
  source.PlusPercent(percentOffset);

// over a rolling window
IReadOnlyList<MathResult> results =
  source.RollingMax(lookbackPeriods);
  // also .RollingMin, .RollingMedian
```

| method | returns |
| ------ | ------- |
| `.Plus(otherSource)` / `.Plus(amount)` | sum of two series, or the series plus a fixed amount |
| `.PlusPercent(percentOffset)` | each value offset by a percent of itself |
| `.Minus(otherSource)` / `.Minus(amount)` | difference of two series, or the series minus a fixed amount |
| `.Times(otherSource)` / `.Times(factor)` | product of two series, or the series times a fixed factor |
| `.DividedBy(otherSource)` / `.DividedBy(divisor)` | quotient of two series, or the series over a fixed divisor |
| `.MaxWith(otherSource)` / `.MinWith(otherSource)` | larger or smaller of two series at each period |
| `.RollingMax(lookbackPeriods)` / `.RollingMin(lookbackPeriods)` | highest or lowest value over a rolling window |
| `.RollingMedian(lookbackPeriods)` | median over a rolling window, averaging the two middle values when the window is even; its cost grows with the window, O(n·k) |

## Parameters

| param | description |
| ----- | ----------- |
| `basisSource`<br>_`IReadOnlyList<IReusable>`_ | Two-series methods: the left-hand series, such as an indicator's results or price bars.  Price bars contribute their close; select another part with [`.ToBarPart()`](/indicators/bar-part) first. |
| `otherSource`<br>_`IReadOnlyList<IReusable>`_ | Two-series methods: the right-hand series, with the same length and timestamps as `basisSource`. |
| `source`<br>_`IReadOnlyList<IReusable>`_ | Fixed-amount and rolling-window methods: the series to transform, such as an indicator's results or price bars. |
| `amount`<br>_`double`_ | Fixed amount to add or subtract.  Must be a finite number. |
| `factor`<br>_`double`_ | Fixed multiplier.  Must be a finite number. |
| `divisor`<br>_`double`_ | Fixed divisor.  Must be a finite number other than 0. |
| `percentOffset`<br>_`double`_ | Percent of each value to add, where `2.5` adds 2.5% and `-2.5` subtracts 2.5%.  Must be a finite number. |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the rolling window, including the current one.  Must be greater than 0. |

A fixed number is always the right-hand operand.  An invalid fixed number or lookback throws an `ArgumentOutOfRangeException`.

### Historical price bars requirements

Fixed-amount and two-series methods need no warmup.  Rolling windows need at least `N` periods.

`basisSource` and `otherSource` must have the same number of results with identical timestamps at every position, which holds for any two indicators calculated from the same bars.  In the Series methods, a length or timestamp mismatch throws an `InvalidBarsException` rather than pairing values from different periods; the stream hub skips the unmatched timestamp instead.  Series with different date ranges are not aligned for you: trim both to the same range first.  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<MathResult>
```

- This method returns a time series of all available indicator values for the `source` provided.
- The Series method always returns the same number of elements as there are in the source series.
- It does not return a single incremental indicator value.
- A period is `null` where an input is `null`, such as an indicator warmup period, and where a two-series divisor is 0.  Rolling windows are also `null` for the first `N-1` periods and while the window holds a `null` value.

### `MathResult`

| property | description |
| -------- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from the evaluated source |
| `Result`<br>_`double?`_ | Result of the operation |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// spread between a fast and a slow moving average
IReadOnlyList<MathResult> spread = bars
    .ToSma(10)
    .Minus(bars.ToSma(20));

// MACD less its own 9-period EMA, plus a 9-period SMA of price
IReadOnlyList<MacdResult> macd = bars.ToMacd(12, 26, 9);
IReadOnlyList<MathResult> composite = macd
    .Minus(macd.ToEma(9))
    .Plus(bars.ToSma(9));

// highest high over the last 20 periods
IReadOnlyList<MathResult> highestHigh = bars
    .ToBarPart(CandlePart.High)
    .RollingMax(20);
```

Results can be further processed on `Result` with additional chain-enabled indicators.

```csharp
// RSI of the spread between a fast and a slow moving average
IReadOnlyList<RsiResult> spreadRsi = bars
    .ToSma(10)
    .Minus(bars.ToSma(20))
    .ToRsi(14);

// percent below the 20-period closing high, smoothed by a 5-period EMA
IReadOnlyList<EmaResult> drawdown = bars
    .DividedBy(bars.RollingMax(20))
    .Minus(1)
    .Times(100)
    .ToEma(5);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Every method streams through a stream hub, which keeps the Series name and is chosen by its receiver: called on a hub, `.Minus(otherSource)` or `.RollingMax(lookbackPeriods)` returns a stream hub, so a Series chain such as `bars.ToSma(10).Minus(bars.ToSma(20))` streams as `barHub.ToSmaHub(10).Minus(barHub.ToSmaHub(20))`.

The rolling-window methods also have a buffer-style form, `.ToRollingMaxList(lookbackPeriods)`, `.ToRollingMinList(lookbackPeriods)`, and `.ToRollingMedianList(lookbackPeriods)`, for incremental calculations without a hub.  Fixed-amount and two-series math has no buffer list.  Plain `double` arithmetic on the values you hold gives `NaN` or infinity where these methods give `null`, such as for a warmup value or a zero divisor, and does not pair two series by timestamp; use the Series method or a stream hub to match these results.

```csharp
RollingMaxList rollingMaxList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rollingMaxList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<MathResult> results = rollingMaxList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
RollingMaxHub observer = barHub.RollingMax(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MathResult> results = observer.Results;
```

Fixed-operand math uses one hub type for every operation, `ScalarMathHub`, such as `barHub.Plus(1)`.

Two-series stream hubs take a second chain provider, such as `.Minus(otherSource)`, built on `PairMathHub`.  A result is added once both providers hold the same timestamp.  A timestamp that only one provider holds produces no result, where the Series method throws.  A value that arrives late, or is revised or removed, on either side is handled by the usual rebuild, and each side stages up to the hub's cache size.  This streams the MACD composite from the example above:

```csharp
BarHub barHub = new();
MacdHub macd = barHub.ToMacdHub(12, 26, 9);

PairMathHub composite = macd
    .Minus(macd.ToEmaHub(9))
    .Plus(barHub.ToSmaHub(9));

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<MathResult> results = composite.Results;
```

Timestamps match exactly; there is no tolerance for clock skew between the two series.  Drive both providers from one thread at a time, as when both descend from one `BarHub`.

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
