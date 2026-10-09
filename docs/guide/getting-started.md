---
title: Getting started
description: Install the Stock Indicators for .NET library and calculate your first indicator in minutes.
---

# Getting started

Install the library, give it your price history, and calculate your first indicator. Using a coding agent? Paste this prompt; it runs the library on live market data itself and shows you the result:

<!--@include: ../shared/agent-prompt.md-->

See [Agent setup](/guide/agent-setup) for what it covers.

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

This complete console app downloads real Microsoft daily bars, so you can see results before connecting a market data provider.

```bash
dotnet new console -n FirstIndicator
cd FirstIndicator
dotnet add package FacioQuo.Stock.Indicators
```

Replace the contents of `Program.cs`, then run `dotnet run`.

```csharp
using System.Globalization;
using FacioQuo.Stock.Indicators;

// 32 years of real Microsoft daily bars from the library's test data
string url = "https://raw.githubusercontent.com/facioquo/stock-indicators-dotnet/main/tests/Library/TestData/quotes/msft.csv";
string csv = await new HttpClient().GetStringAsync(url);

List<Bar> bars = csv
    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Skip(1) // header: date,open,high,low,close,volume
    .Select(line => line.Trim().Split(','))
    .Select(f => new Bar(
        Timestamp: DateTime.Parse(f[0], CultureInfo.InvariantCulture),
        Open: decimal.Parse(f[1], CultureInfo.InvariantCulture),
        High: decimal.Parse(f[2], CultureInfo.InvariantCulture),
        Low: decimal.Parse(f[3], CultureInfo.InvariantCulture),
        Close: decimal.Parse(f[4], CultureInfo.InvariantCulture),
        Volume: decimal.Parse(f[5], CultureInfo.InvariantCulture)))
    .ToList();

// calculate 50-period SMA and 14-period RSI
IReadOnlyList<SmaResult> sma = bars.ToSma(50);
IReadOnlyList<RsiResult> rsi = bars.ToRsi(14);

for (int i = bars.Count - 5; i < bars.Count; i++)
{
    Console.WriteLine($"{bars[i].Timestamp:yyyy-MM-dd}  close {bars[i].Close:N2}  SMA {sma[i].Sma:N2}  RSI {rsi[i].Rsi:N1}");
}
```

```console
2022-03-04  close 289.86  SMA 307.53  RSI 43.1
2022-03-07  close 278.91  SMA 306.45  RSI 36.8
2022-03-08  close 275.85  SMA 305.29  RSI 35.3
2022-03-09  close 288.50  SMA 304.23  RSI 45.5
2022-03-10  close 285.59  SMA 303.13  RSI 43.8
```

### Use your own price data

Replace the downloaded bars with historical bars from your data provider. `GetBarsFromFeed()` stands in for [your own data acquisition](#where-can-i-get-historical-bar-data).

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
