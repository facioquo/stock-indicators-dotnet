[![image](https://raw.githubusercontent.com/facioquo/stock-indicators-dotnet/6ac2854d7677b69abb1b615bdf93048cb12ea207/docs/.offline/social-banner.png)](https://dotnet.StockIndicators.dev)

[![GitHub Stars](https://img.shields.io/github/stars/facioquo/stock-indicators-dotnet?logo=github&label=Stars)](https://github.com/facioquo/stock-indicators-dotnet)
[![NuGet package](https://img.shields.io/nuget/v/facioquo.stock.indicators?color=blue&logo=NuGet&label=NuGet)](https://www.nuget.org/packages/FacioQuo.Stock.Indicators)

# Stock Indicators for .NET

**Stock Indicators for .NET** is a C# [library package](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) that produces financial market technical indicators.  Send in historical price bars and get back desired indicators such as moving averages, Relative Strength Index, Stochastic Oscillator, Parabolic SAR, etc.  Nothing more.

Build your technical analysis, trading algorithms, machine learning, charting, or other intelligent market software with this library and your own [OHLCV](https://dotnet.stockindicators.dev/guide/#historical-bars) price bars sources for equities, commodities, forex, cryptocurrencies, and others.

> _This **`FacioQuo.Stock.Indicators`** NuGet package was formerly named `Skender.Stock.Indicators`_

## Streaming support

Most indicators support three calculation styles, two of them built for real-time and incremental data processing:

- **Series** - Traditional batch processing for complete historical datasets
- **BufferList** - Incremental calculations with simple, efficient buffer management
- **StreamHub** - Real-time processing with observable patterns and state management

Quick example using streaming:

```csharp
// Create a hub for streaming price bars
BarHub barHub = new();

// Subscribe indicators to the hub
EmaHub emaHub = barHub.ToEmaHub(20);
RsiHub rsiHub = barHub.ToRsiHub(14);

// Call from your WebSocket or SSE message handler as each bar arrives
void OnBarReceived(Bar bar)
{
    barHub.Add(bar);

    // Access real-time results
    EmaResult emaResult = emaHub.Results[^1];
    RsiResult rsiResult = rsiHub.Results[^1];
}
```

[Migrate to v3 →](https://dotnet.stockindicators.dev/migration/v3)

Visit our project site for more information:

- [Overview](https://dotnet.stockindicators.dev/)
- [Indicators and overlays](https://dotnet.stockindicators.dev/indicators/)
- [Guide and Pro tips](https://dotnet.stockindicators.dev/guide/)
- [Demo site](https://charts.stockindicators.dev/) (a stock chart)
- [Release notes](https://github.com/facioquo/stock-indicators-dotnet/releases)
- [Discussions](https://github.com/facioquo/stock-indicators-dotnet/discussions)
- [Contributing](https://dotnet.stockindicators.dev/contributing/)
