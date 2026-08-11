---
title: Stock Indicators for .NET
titleTemplate: Transform price quotes into trading insights
layout: home
isHome: true

hero:
  name: stock indicators <small>for .NET</small>
  tagline: Transform price quotes into trade indicators and market insights.
  actions:
    - theme: brand
      text: get started
      link: /guide/getting-started
    - theme: alt
      text: guide
      link: /guide/
    - theme: alt
      text: indicators
      link: /indicators/
---

<script setup>
import LandingCharts from './.vitepress/components/LandingCharts.vue'
</script>

<a href="https://www.nuget.org/packages/FacioQuo.Stock.Indicators" aria-label="Get the NuGet package."><img src="https://img.shields.io/nuget/v/facioquo.stock.indicators?logo=NuGet&label=NuGet&color=blue&cacheSeconds=259200" alt="NuGet Package" /></a>

**Stock Indicators for .NET** is a C# [library package](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) that transforms batch and streaming financial market price data into technical indicators. Get moving averages, Relative Strength Index, Stochastic Oscillator, Parabolic SAR, and [many other indicators](/indicators).

Build trading algorithms, charting applications, machine learning models, or market analysis tools with your own [OHLCV](/guide/getting-started#historical-bars) price bars from any market: equities, commodities, forex, or cryptocurrencies.

::: tip ✨ v3 adds streaming support
Our new **`FacioQuo.Stock.Indicators`** NuGet library, formerly `Skender.Stock.Indicators`, adds stream hub and buffer list style indicators to enable your incremental and real-time price data scenarios.
Still on v2? See our [migration guide →](/migration/v3)
:::

## Industry-standard indicators with extensibility

Access a comprehensive library of battle-tested technical indicators used by traders worldwide. Extend functionality by creating your own [custom indicators](/guide/customization) that integrate seamlessly with the library.

<ClientOnly>
  <LandingCharts />
</ClientOnly>

## Simple, intuitive API

```csharp
// example: calculate 20-period simple moving average
IReadOnlyList<SmaResult> results = bars.ToSma(20);
```

See more [usage examples](/guide/getting-started#example-usage).

## Three styles to meet your needs

The library provides three indicator styles for different use cases.

| Style | Best for |
| ----- | -------- |
| [Batch (Series)](/guide/styles/batch) | Once-and-done bulk calculations on complete datasets |
| [Buffer lists](/guide/styles/buffer) | Self-managed incremental data, sequential processing |
| [Stream hubs](/guide/styles/stream) | Live data feeds with coordinated multi-indicator updates |

See the [Indicator styles](/guide/styles/) guide for a full feature comparison.

## Incrementally add data with buffer lists <Badge type="warning" text="preview" />

For scenarios where bars arrive one at a time, buffer lists provide efficient incremental processing without recalculating the entire history.

```csharp
// create list
SmaList smaList = new(lookbackPeriods: 20);

// add new bars incrementally
smaList.Add(newBar);
```

Buffer lists maintain internal state and automatically manage the warmup period, making them ideal for basic live data feeds and incremental updates.

## Streaming hubs with observer patterns <Badge type="warning" text="preview" />

Hubs provides a reactive, subscription-based pattern for streaming market data with automatic cascading calculations for advances scenarios.

### Example 1: Chained hubs

```csharp
// create provider and subscribe observers
BarHub barHub = new(); // provider
SmaHub smaHub = barHub.ToSmaHub(20);
RsiHub rsiHub = smaHub.ToRsiHub(14);  // RSI of SMA

// publish bars - observers auto-update in cascade
barHub.Add(newBar);

// consume downstream hubs indicators
IReadOnlyList<RsiResult> results = rsiHub.Results;
```

### Example 2: signal-based strategy

```csharp
// create provider and subscribe observers
BarHub barHub = new();
EmaHub emaFast = barHub.ToEmaHub(50);
EmaHub emaSlow = barHub.ToEmaHub(200);

// add bars to barHub (from stream)
barHub.Add(newBar);
// and the 2 EmaHubs will be in sync

if(emaFast.Results[^2].Ema < emaSlow.Results[^2].Ema
&& emaFast.Results[^1].Ema > emaSlow.Results[^1].Ema)
{
    // cross over occurred
}
```

The observer cascade ensures that when a new bar arrives, all chained indicators update automatically in the correct sequence.

See the [guide](/guide/getting-started) and the [full list of indicators and overlays](/indicators) for more information.

## Powerful chaining for advanced analysis

Chain indicators together for sophisticated technical analysis: create indicators of indicators, calculate [slope](/indicators/slope) (direction) of any result, or apply [moving averages](/indicators/sma) to indicator outputs.

```csharp
// example: calculate RSI of On-Balance Volume
IReadOnlyList<RsiResult> results
  = bars.ToObv()
        .ToRsi(14);

// example: use custom candle price variants
IReadOnlyList<EmaResult> results
  = bars.Use(CandlePart.HL2)
        .ToEma(20);
```

See [Chaining indicators](/guide/chaining) for more.

## Optimized for modern .NET frameworks

Our [NuGet library](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) directly targets all actively [supported Microsoft .NET platforms](https://dotnet.microsoft.com/platform/support/policy) for peak performance.

- .NET 10.0, 9.0, 8.0

The compiled library package is [Common Language Specification (CLS) compliant](https://docs.microsoft.com/en-us/dotnet/standard/common-type-system) and can be used in other programming languages, including Python and everything in the .NET universe.

## Licensed for everyone

<a href="https://opensource.org/licenses/Apache-2.0"><img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg?style=flat-square&cacheSeconds=259200" alt="Apache 2.0 license badge" /></a>

This repository uses the standard Apache 2.0 open-source license. Please review the [license](https://opensource.org/licenses/Apache-2.0) before using or contributing to the software.

## Share your ideas with the community

**Need help?** Have ideas? [Start a new discussion, ask a question 💬](https://github.com/facioquo/stock-indicators-dotnet/discussions), or [submit an issue](https://github.com/facioquo/stock-indicators-dotnet/issues) if it is publicly relevant. You can also direct message [@daveskender](https://twitter.com/messages/compose?recipient_id=27475431).

## Give back with patronage

Thank you for your support! This software is crafted with care by unpaid enthusiasts who 💖 all forms of encouragement. If you or your organization use this library or like what we're doing, add a ⭐ on the [GitHub Repo](https://github.com/facioquo/stock-indicators-dotnet) as a token of appreciation.

If you want to buy me a beer or are interested in ongoing support as a patron, [become a sponsor](https://github.com/sponsors/facioquo). Patronage motivates continued maintenance and evolution of open-source projects, and to inspire new ones.

## Contribute to help others

This NuGet package is an open-source project [on GitHub](https://github.com/facioquo/stock-indicators-dotnet). If you want to report bugs or contribute fixes, new indicators, or new features, please review our [contributing guidelines](/contributing) and [the backlog](https://github.com/orgs/facioquo/projects/27).

Special thanks to all of our community code contributors!

<Contributors />

Visit our [GitHub repository](https://github.com/facioquo/stock-indicators-dotnet) to begin contributing, or browse the [full list of indicators and overlays](/indicators).
