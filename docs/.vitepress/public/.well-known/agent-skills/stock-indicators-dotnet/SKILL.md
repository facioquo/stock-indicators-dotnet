---
name: stock-indicators-dotnet
description: Calculate technical analysis indicators (SMA, EMA, RSI, MACD, Bollinger Bands, and dozens more) from OHLCV price bars in C# with the FacioQuo.Stock.Indicators v3 NuGet package. Use when adding, migrating, or reviewing .NET code that computes market indicators, including code written for the older Skender.Stock.Indicators v2 package.
license: Apache-2.0
metadata:
  package: FacioQuo.Stock.Indicators
  docs_version: v3
  documentation: https://dotnet.stockindicators.dev
---

# Stock Indicators for .NET (v3)

## Identify the library correctly

- NuGet package and namespace: `FacioQuo.Stock.Indicators`
- Install: `dotnet add package FacioQuo.Stock.Indicators`
- `Skender.Stock.Indicators` is the superseded v2 package. Do not install it, and do not write v2 APIs from memory.

## Read the documentation before writing code

1. Fetch the index at <https://dotnet.stockindicators.dev/llms.txt>.
2. Fetch the page for the indicator you need; every page has a Markdown version at the same URL plus `.md` (for example `https://dotnet.stockindicators.dev/indicators/sma.md`).
3. Follow each page's parameter constraints, warmup requirements, and result type exactly. For the complete reference in one file, use <https://dotnet.stockindicators.dev/llms-full.txt>.

## Core pattern

```csharp
using FacioQuo.Stock.Indicators;

IReadOnlyList<Bar> bars = GetBarsFromFeed("MSFT"); // your data source

IReadOnlyList<SmaResult> results = bars.ToSma(20);
```

Choose the indicator style that fits the workload:

- **Batch (Series)**: `bars.ToSma(20)` for a full collection at once.
- **Buffer list**: `new SmaList(20)`, then `.Add(bar)` for incremental bar-by-bar updates.
- **Stream hub**: `BarHub barHub = new(); SmaHub sma = barHub.ToSmaHub(20);` for live feeds with coordinated subscribers.

## v2 to v3 corrections

| v2 (do not use) | v3 |
| --------------- | -- |
| `using Skender.Stock.Indicators;` | `using FacioQuo.Stock.Indicators;` |
| `quotes.GetSma(20)` | `bars.ToSma(20)` |
| `Quote`, `IQuote` | `Bar`, `IBar` |
| `Date` property | `Timestamp` |
| `GetBaseQuote()` | `Use(CandlePart)` |

Full migration guide: <https://dotnet.stockindicators.dev/migration/v3.md>.

## Rules

- Supply bars in chronological order with a consistent frequency; see a page's warmup guidance before trimming history.
- Check each page's Response section: most results have one entry per input bar, with `null` values during warmup periods that `.RemoveWarmupPeriods()` drops.
- Custom bar types implement `IBar`; chain indicators by calling one indicator on another's results.
