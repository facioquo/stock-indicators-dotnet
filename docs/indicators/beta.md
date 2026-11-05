---
title: Beta coefficient
description: Beta coefficient with Beta+/Beta- shows how strongly one asset's price responds to systemic volatility of the entire market.  Upside Beta (Beta+) and Downside Beta (Beta-), popularized by Harry M. Markowitz, are also included.
---

# Beta coefficient (β)

[Beta](https://en.wikipedia.org/wiki/Beta_(finance)) shows how strongly one asset's price responds to systemic volatility of the entire market. Beta measures an asset's non-diversifiable systematic risk — its market exposure — not its idiosyncratic, asset-specific risk. [Upside Beta](https://en.wikipedia.org/wiki/Upside_beta) (Beta+) and [Downside Beta](https://en.wikipedia.org/wiki/Downside_beta) (Beta-), [popularized by Harry M. Markowitz](https://www.jstor.org/stable/j.ctt1bh4c8h), are also included.  Beta+ and Beta- capture asymmetric sensitivity during rising and falling markets. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/268 "Community discussion about this indicator")

<StockIndicatorChart indicator="Beta" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<BetaResult> results = barsEval
  .ToBeta(barsMarket, lookbackPeriods, type);
```

## Parameters

| param | description |
| ----- | ----------- |
| `sourceEval`<br>_`IReadOnlyList<IReusable>`_ | [Historical price bars](/guide/getting-started#historical-bars) used as the evaluation subject.  You must have the same number of periods as `sourceMrkt`. |
| `sourceMrkt`<br>_`IReadOnlyList<IReusable>`_ | [Historical price bars](/guide/getting-started#historical-bars) used as the benchmark basis for comparison.  This is usually market index data.  You must have the same number of periods as `sourceEval`. |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback window.  Must be greater than 0 to calculate; however we suggest a larger period for statistically appropriate sample size and especially when using Beta +/-. |
| `type`<br>_`BetaType`_ | Type of Beta to calculate.  Default is `BetaType.Standard`. See [`BetaType` options](#betatype-options) below. |

### Historical price bars requirements

You must have at least `N` periods of `sourceEval` and `sourceMrkt` to cover the warmup periods.  More than the minimum is typically provided, since a larger sample improves statistical quality — especially when using Beta +/-.

`sourceEval` and `sourceMrkt` must have consistent frequency (day, hour, minute, etc).  Mismatch histories will throw `InvalidBarsException`. See [the Guide](/guide/getting-started#historical-bars) for more information.

### `BetaType` options

| type                | description                                                                                |
| ------------------- | ------------------------------------------------------------------------------------------ |
| `BetaType.Standard` | Standard Beta only.  Uses all historical price bars. (default)                             |
| `BetaType.Up`       | Upside Beta only.  Uses market up bars only.                                               |
| `BetaType.Down`     | Downside Beta only.  Uses market down bars only.                                           |
| `BetaType.All`      | Returns all of the above.  Required for `Ratio` and `Convexity` values.  Note: 3× slower.  |

::: tip ✨ Pro tip
Financial institutions often depict a single number for Beta on their sites.  To get that same long-term Beta value, use 5 years of monthly bars for `bars` and a value of 60 for `lookbackPeriods`.  If you only have smaller bars, use the [Aggregate()](/utilities/bars#resize-bar-history) utility to convert it.

[Alpha](https://en.wikipedia.org/wiki/Alpha_(finance)) is calculated as `R – Rf – Beta (Rm - Rf)`, where `Rf` is the risk-free rate.
:::

## Response

```csharp
IReadOnlyList<BetaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `BetaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Beta`<br>_`double?`_ | Beta coefficient based |
| `BetaUp`<br>_`double?`_ | Beta+ (Up Beta) |
| `BetaDown`<br>_`double?`_ | Beta- (Down Beta) |
| `Ratio`<br>_`double?`_ | Beta ratio is `BetaUp/BetaDown` |
| `Convexity`<br>_`double?`_ | Beta convexity is <code>(BetaUp-BetaDown)<sup>2</sup></code> |
| `ReturnsEval`<br>_`double?`_ | Returns of evaluated bars (`R`) |
| `ReturnsMrkt`<br>_`double?`_ | Returns of market bars (`Rm`) |

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
var results = barsEval
    .Use(CandlePart.HL2)
    .ToBeta(barsMarket.Use(CandlePart.HL2), ..);
```

::: warning 🚩 Matching inputs required
Both eval and market arguments must contain the same number of elements and be the results of a chainable indicator or `.Use()` method.
:::

Results can be further processed on `Beta` with additional chain-enabled indicators.

```csharp
// example
var results = barsEval
    .ToBeta(barsMarket, ..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub.  Add each series separately as its values arrive; either one may run ahead, and a result is added once both hold the same timestamp:

```csharp
BetaList betaList = new(lookbackPeriods, type);

// call from each series' message handler
void OnEvalReceived(IReusable value) => betaList.AddSource(value);
void OnMarketReceived(IReusable value) => betaList.AddOther(value);

// based on `IReadOnlyList<BetaResult>`; fills as the handlers run
IReadOnlyList<BetaResult> results = betaList;
```

A timestamp that only one series holds produces no result, where the Series method throws.  Each series must arrive in strictly increasing order: an older or equal timestamp on a side throws an `InvalidOperationException`.  Each side stages at most `MaxListSize` values, so a side that falls further behind than that loses the pairs the leading side has dropped.

Subscribe to a `BarHub` for each series for advanced streaming scenarios:

```csharp
BarHub evalHub = new();
BarHub marketHub = new();
BetaHub observer = evalHub.ToBetaHub(marketHub, lookbackPeriods, type);

// call from each series' message handler
void OnEvalReceived(IBar bar) => evalHub.Add(bar);
void OnMarketReceived(IBar bar) => marketHub.Add(bar);

// results fill as the handlers run
IReadOnlyList<BetaResult> results = observer.Results;
```

A result is added once both providers hold the same timestamp.  A value that arrives late, or is revised or removed, on either side is handled by the usual rebuild.  Timestamps match exactly: each return is measured from the previous matched timestamp, and the lookback window spans `N` matched returns.  Drive both providers from one thread at a time.

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
