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
      text: indicators
      link: /indicators/
---

<script setup>
import LandingCharts from './.vitepress/components/LandingCharts.vue'
import NuGetBadge from './.vitepress/components/NuGetBadge.vue'
</script>

<NuGetBadge class="nuget-badge-body" />

**Stock Indicators for .NET** is a C# [library package](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) that turns your own [OHLCV](/guide/getting-started#historical-bars) price bars, from any market, into moving averages, Relative Strength Index, Stochastic Oscillator, Parabolic SAR, and [many other indicators](/indicators). Use them in trading algorithms, charting apps, machine learning models, and market analysis tools, in batch or in real time.

::: tip ✨ v3 adds streaming support
[**`FacioQuo.Stock.Indicators`**](https://www.nuget.org/packages/FacioQuo.Stock.Indicators), formerly `Skender.Stock.Indicators`, adds buffer list and stream hub indicators for incremental and real-time price data.
Upgrading from v2? See the [migration guide →](/migration/v3)
:::

## Get started in minutes

Paste this prompt into your coding agent:

```prompt
Read https://dotnet.stockindicators.dev/llms.txt and its getting started guide,
then help me install the FacioQuo.Stock.Indicators NuGet package
and calculate my first indicator from my own price data.
```

Or install it yourself and calculate your first indicator:

```bash
dotnet add package FacioQuo.Stock.Indicators
```

```csharp
// bars: your own historical price data
IReadOnlyList<SmaResult> results = bars.ToSma(20);
```

Continue with [Getting started](/guide/getting-started) or [Agent setup](/guide/agent-setup).

## Industry-standard indicators with extensibility

Access a comprehensive library of battle-tested technical indicators used by traders worldwide. Extend functionality by creating your own [custom indicators](/guide/customization) that integrate seamlessly with the library.

<ClientOnly>
  <LandingCharts />
</ClientOnly>

## Three styles to meet your needs

| Style | Best for |
| ----- | -------- |
| [Batch (Series)](/guide/styles/batch) | Once-and-done bulk calculations on complete datasets |
| [Buffer lists](/guide/styles/buffer) | Self-managed incremental data, sequential processing |
| [Stream hubs](/guide/styles/stream) | Live data feeds with coordinated multi-indicator updates |

See the [Indicator styles](/guide/styles/) guide for a full feature comparison.

## Incrementally add data with buffer lists <Badge type="tip" text="new in v3" />

When bars arrive one at a time, buffer lists update without recalculating the entire history.

```csharp
// create list
SmaList smaList = new(lookbackPeriods: 20);

// add new bars incrementally
smaList.Add(newBar);
```

## Streaming hubs with observer patterns <Badge type="tip" text="new in v3" />

Stream hubs push each new bar through every subscribed indicator, including chained ones, in the correct sequence.

```csharp
// one provider feeds any number of subscribers
BarHub barHub = new();
EmaHub emaFast = barHub.ToEmaHub(50);
EmaHub emaSlow = barHub.ToEmaHub(200);
RsiHub rsiHub = emaFast.ToRsiHub(14); // hubs chain, too

// add each bar as it arrives; all hubs stay in sync
barHub.Add(newBar);

// example: detect a bullish crossover
if (emaFast.Results[^2].Ema < emaSlow.Results[^2].Ema
 && emaFast.Results[^1].Ema > emaSlow.Results[^1].Ema)
{
    // fast EMA crossed above slow EMA
}
```

## Powerful chaining for advanced analysis

Create indicators of indicators, calculate [slope](/indicators/slope) (direction) of any result, or apply [moving averages](/indicators/sma) to indicator outputs.

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

The library directly targets all actively [supported .NET versions](https://dotnet.microsoft.com/platform/support/policy) (10.0, 9.0, and 8.0) for peak performance. It's [CLS compliant](https://learn.microsoft.com/dotnet/standard/common-type-system), so it works well beyond C#, in other languages and platforms that interoperate with .NET.

## Community and support

- **Need help or have ideas?** [Start a discussion 💬](https://github.com/facioquo/stock-indicators-dotnet/discussions) or [submit an issue](https://github.com/facioquo/stock-indicators-dotnet/issues) if it is publicly relevant. You can also direct message [@daveskender](https://twitter.com/messages/compose?recipient_id=27475431).
- **Contribute** bug reports, fixes, new indicators, or features: see our [contributing guidelines](/contributing) and [the backlog](https://github.com/orgs/facioquo/projects/27).
- **Show your support:** this library is crafted by unpaid enthusiasts. Add a ⭐ on [GitHub](https://github.com/facioquo/stock-indicators-dotnet) or [become a sponsor](https://github.com/sponsors/facioquo) 💖.

Special thanks to all of our community code contributors!

<Contributors />
