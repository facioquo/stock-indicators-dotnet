---
title: Getting started
description: Install the Stock Indicators for .NET library and calculate your first indicator in minutes.
---

# Getting started

## Installation and setup

Find and install the [FacioQuo.Stock.Indicators](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) NuGet package into your Project.

```bash
# dotnet CLI example
dotnet add package FacioQuo.Stock.Indicators

# package manager example
Install-Package FacioQuo.Stock.Indicators
```

> See more [help for installing packages](https://www.google.com/search?q=install+nuget+package).

## Prerequisite data

Most indicators require that you provide historical aggregate OHLCV price bar data and additional configuration parameters.

Historical price data can be provided as a `List`, `IReadOnlyList`, or `ICollection` of the `Bar` class ([see _**Historical bars**_ section below](#historical-bars)); however, it can also be supplied as a generic [custom `TBar` type](#using-custom-bar-classes) if you prefer to use your own `IBar` derived model.

For configurable indicator parameters, default values are provided when there is an industry standard.  You can, of course, override these and provide your own values.

## Implementation pattern

The library supports three indicator styles, each use the same pattern:

```csharp
using FacioQuo.Stock.Indicators;

[..]

// step 1: get price bar(s) from your source
// step 2: calculate indicator value(s)
```

The examples on this page depict the **Batch (Series)** style because it is the simplest starting point and covers most use cases. Buffer lists and stream hubs are first-class alternatives for incremental and streaming scenarios — see [Indicator styles](/guide/styles/) for a side-by-side comparison and guidance on choosing.

- **[Batch (Series)](/guide/styles/batch)** — convert a full price data collection at once. This is the standard style.
- **[Buffer lists](/guide/styles/buffer)** — self-managed incrementing lists, for adding price bars one at a time.
- **[Stream hubs](/guide/styles/stream)** — subscription-based hub-observer pattern, for live/streaming data and chained, real-time architectures.

### Example usage

```csharp
using FacioQuo.Stock.Indicators;

[..]

// fetch historical price bars from your feed (your method)
IReadOnlyList<Bar> bars = GetBarsFromFeed("MSFT");

// calculate 20-period SMA
IReadOnlyList<SmaResult> results = bars
  .ToSma(20);

// use results as needed for your use case (example only)
foreach (SmaResult r in results)
{
    Console.WriteLine($"SMA on {r.Timestamp:d} was ${r.Sma:N4}");
}
```

```console
SMA on 4/19/2018 was $255.0590
SMA on 4/20/2018 was $255.2015
SMA on 4/23/2018 was $255.6135
SMA on 4/24/2018 was $255.5105
SMA on 4/25/2018 was $255.6570
SMA on 4/26/2018 was $255.9705
..
```

See [individual indicator pages](/indicators) for specific usage guidance.

More examples available:

- [Example usage code](/examples/) on GitHub
- [Demo site](https://charts.stockindicators.dev) (a stock chart)

::: tip For a deeper guide
See the [Guide](/guide/) for batch, buffer, and stream styles; chaining; custom indicators.
:::

## Historical bars

You must provide historical price bars to the library in the standard OHLCV `IReadOnlyList<Bar>` or a compatible `List` or `ICollection` format.  It should have a consistent period frequency (day, hour, minute, etc).  See [using custom bar classes](#using-custom-bar-classes) if you prefer to use your own `IBar` derived class.

| property | type | description |
| :------- | :--- | :---------- |
| `Timestamp` | _`DateTime`_ | Close date  |
| `Open`      | _`decimal`_  | Open price  |
| `High`      | _`decimal`_  | High price  |
| `Low`       | _`decimal`_  | Low price   |
| `Close`     | _`decimal`_  | Close price |
| `Volume`    | _`decimal`_  | Volume      |

### Where can I get historical bar data?

::: info BYOD: bring your own data
You must get price bar data from your own provider. _The `GetBarsFromFeed()` method shown in our examples represents your own acquisition of price data and **is not part of this library**._
:::

There are many places to get financial market data.  Check with your brokerage or other commercial sites.  If you're looking for a free developer API, see our ongoing [discussion on market data](https://github.com/facioquo/stock-indicators-dotnet/discussions/579) for ideas.

### How much historical bar data do I need?

Each indicator will need different amounts of price `bars` to calculate.  You can find guidance on the individual indicator documentation pages for minimum requirements; however, **most use cases will require that you provide more than the minimum**.  As a general rule of thumb, you will be safe if you provide 750 points of historical bar data (e.g. 3 years of daily data).

::: warning 🚩 IMPORTANT
Applying the _minimum_ amount of bar history as possible is NOT a good way to optimize your system. Some indicators use a smoothing technique that converges to better precision over time. While you can calculate these with the minimum amount of bar data, the precision to two decimal points often requires 250 or more preceding historical records.

For example, if you are using daily data and want one year of precise EMA(250) data, you need to provide 3 years of historical price bars (1 extra year for the lookback period and 1 extra year for convergence); thereafter, you would discard or not use the first two years of results. Occasionally, even more is required for optimal precision.

See [discussion on warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) for more information.
:::

### Using custom bar classes

If you would like to use your own custom `MyCustomBar` class, to avoid needing to transpose into the library `Bar` class, you only need to add the `IBar` interface.

```csharp
using FacioQuo.Stock.Indicators;

[..]

public record MyCustomBar : IBar
{
    // required base properties
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }

    // IReusable interface (enables chaining)
    [JsonIgnore]
    public double Value => (double)Close;

    // custom properties
    public int MyOtherProperty { get; set; }
}
```

```csharp
// fetch historical price bars from your favorite feed
IReadOnlyList<MyCustomBar> myBars = GetBarsFromFeed("MSFT");

// example: get 20-period simple moving average
IReadOnlyList<SmaResult> results = myBars.ToSma(20);
```

::: warning 🚩 Custom bars must have value based equality
When implementing your custom bar type, it must be either `record` class or implement `IEquatable<T>` to be compatible with the streaming hub internal de-duplication logic.
:::

## Chaining indicators

If you want to compute an indicator of indicators, such as an SMA of an ADX or an [RSI of an OBV](https://medium.com/@robswc/this-is-what-happens-when-you-combine-the-obv-and-rsi-indicators-6616d991773d), use _**chaining**_ to calculate an indicator from prior results. Example:

```csharp
// fetch historical price bars from your feed (your method)
IReadOnlyList<Bar> bars = GetBarsFromFeed("SPY");

// calculate RSI of OBV
IReadOnlyList<RsiResult> results
  = bars
    .ToObv()
    .ToRsi(14);
```

See [Chaining indicators](/guide/chaining) for more.

## Utilities

See [Utilities and helper functions](/utilities/) for additional tools.
