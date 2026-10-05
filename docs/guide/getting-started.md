---
title: Getting started
description: Install the Stock Indicators for .NET library and calculate your first indicator in minutes.
---

# Getting started

Install the library, give it your price history, and calculate your first indicator. Using a coding agent? Paste this prompt and it will walk you through these steps:

```prompt
Read https://dotnet.stockindicators.dev/llms.txt and its getting started guide,
then help me install the FacioQuo.Stock.Indicators NuGet package
and calculate my first indicator from my own price data.
```

See [Agent setup](/guide/agent-setup) for more prompts.

## Installation and setup

Install the [FacioQuo.Stock.Indicators](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) NuGet package into your project.

::: code-group

```bash [.NET CLI]
dotnet add package FacioQuo.Stock.Indicators
```

```powershell [Package Manager]
Install-Package FacioQuo.Stock.Indicators
```

:::

## Calculate your first indicator

Indicators take a list of historical price bars, either the library's [`Bar` class](#historical-bars) or [your own bar class](#using-custom-bar-classes). Parameters have standard defaults where an industry convention exists, and you can override any of them.

This page uses the **[Batch (Series)](/guide/styles/batch)** style, the simplest starting point, which converts a full collection at once. For bars that arrive one at a time, see **[Buffer lists](/guide/styles/buffer)** and **[Stream hubs](/guide/styles/stream)**, or [compare all three styles](/guide/styles/).

### Try it now

This complete console app builds sample bars in memory, so you can see results before connecting a market data provider.

```bash
dotnet new console -n FirstIndicator
cd FirstIndicator
dotnet add package FacioQuo.Stock.Indicators
```

Replace the contents of `Program.cs`, then run `dotnet run`.

```csharp
using FacioQuo.Stock.Indicators;

// sample bars stand in for data from your own provider
List<Bar> bars = [];
DateTime date = new(2025, 1, 2);
decimal close = 100m;

for (int i = 0; i < 30; i++)
{
    close += i % 3 == 0 ? -1.5m : 1m;
    bars.Add(new Bar(date.AddDays(i), close - 0.5m, close + 1m, close - 1m, close, 1_000_000m));
}

// calculate 20-period SMA
IReadOnlyList<SmaResult> results = bars.ToSma(20);

foreach (SmaResult r in results.TakeLast(5))
{
    Console.WriteLine($"SMA on {r.Timestamp:yyyy-MM-dd} was {r.Sma:N4}");
}
```

```console
SMA on 2025-01-27 was 101.8750
SMA on 2025-01-28 was 102.1250
SMA on 2025-01-29 was 102.2500
SMA on 2025-01-30 was 102.3750
SMA on 2025-01-31 was 102.6250
```

### Use your own price data

Replace the sample bars with historical bars from your data provider. `GetBarsFromFeed()` stands in for [your own data acquisition](#where-can-i-get-historical-bar-data).

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

### Next steps

- Browse the [indicators](/indicators) for each one's parameters, warmup, and results.
- Explore [example code](/examples/) and the [demo charts](https://charts.stockindicators.dev).
- Read the [Guide](/guide/) for indicator styles, chaining, and custom indicators.

## Historical bars

Provide historical price bars as an OHLCV `IReadOnlyList<Bar>`, `List`, or `ICollection`, with a consistent frequency (day, hour, minute, etc.). To use your own class instead, see [using custom bar classes](#using-custom-bar-classes).

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

Check with your brokerage or a commercial data provider. For free developer APIs, see our ongoing [discussion on market data](https://github.com/facioquo/stock-indicators-dotnet/discussions/579) for ideas.

### How much historical bar data do I need?

Each indicator page lists its minimum, but **most use cases need more than the minimum**. As a rule of thumb, 750 bars (about 3 years of daily data) is safe.

::: warning 🚩 Use more than the minimum history
Supplying only the _minimum_ bar history is NOT a good optimization. Some indicators use smoothing that converges to better precision over time, and two-decimal precision often needs 250 or more preceding bars.

For example, if you are using daily data and want one year of precise EMA(250) data, you need to provide 3 years of historical price bars (1 extra year for the lookback period and 1 extra year for convergence); thereafter, you would discard or not use the first two years of results. Occasionally, even more is required for optimal precision.

See [discussion on warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) for more information.
:::

### Using custom bar classes

To use your own bar class without converting to the library `Bar` class, add the `IBar` interface.

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
Make your custom bar type a `record` class or implement `IEquatable<T>`, so stream hubs can detect duplicate bars.
:::

## Chaining indicators

To calculate an indicator of an indicator, such as an SMA of ADX or an [RSI of OBV](https://medium.com/@robswc/this-is-what-happens-when-you-combine-the-obv-and-rsi-indicators-6616d991773d), _**chain**_ them:

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
