---
name: stock-indicators-dotnet
description: Calculate technical analysis indicators (EMA, RSI, MACD, ADX, Bollinger Bands, and dozens more) from OHLCV price bars, in batch or live from a market feed, in C# with the FacioQuo.Stock.Indicators v3 NuGet package. Use when adding, migrating, or reviewing .NET code that computes market indicators, including code written for the older Skender.Stock.Indicators v2 package.
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
3. Follow each page's parameter constraints, warmup requirements, and result type exactly. Load <https://dotnet.stockindicators.dev/llms-full.txt>, the complete reference in one large file, only for broad analysis that needs most of the documentation.

## Helping someone get started

Follow the setup guide at <https://dotnet.stockindicators.dev/agents/start.md>. It routes to playbooks for live signals, backtests, integration, a feature tour, and v2 migration, and lists keyless live feeds and real historical datasets. Never invent price data.

## Core pattern

```csharp
using FacioQuo.Stock.Indicators;

IReadOnlyList<Bar> bars = GetBarsFromFeed("MSFT"); // your data source

IReadOnlyList<EmaResult> fast = bars.ToEma(21);
IReadOnlyList<EmaResult> slow = bars.ToEma(55);
IReadOnlyList<RsiResult> rsiOfObv = bars.ToObv().ToRsi(14); // indicators chain
```

Choose the indicator style that fits the workload; all three return identical values:

- **Batch (Series)**: `bars.ToEma(21)` for a full collection at once.
- **Buffer list**: `new EmaList(21)`, then `.Add(bar)` for incremental, in-order bar-by-bar updates.
- **Stream hub**: `BarHub barHub = new(); EmaHub ema = barHub.ToEmaHub(21);` for live feeds; hubs absorb late, out-of-order, and revised bars, and several indicators share one `BarHub`.

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
