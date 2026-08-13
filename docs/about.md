---
title: About
description: Information about Stock Indicators for .NET, sources, and attributions
head:
  - - meta
    - name: robots
      content: noindex, nofollow
---

# About this library

**Stock Indicators for .NET** is an open-source C# [NuGet library](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) that transforms historical price bars into technical indicators. The library provides industry-standard technical analysis indicators with a focus on accuracy, performance, and developer ergonomics.

## Project information

| | |
| --- | --- |
| **License** | [Apache 2.0](https://opensource.org/licenses/Apache-2.0) |
| **Source code** | [GitHub repository](https://github.com/facioquo/stock-indicators-dotnet) |
| **Package** | [FacioQuo.Stock.Indicators](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) (NuGet.org) |
| **Documentation** | [dotnet.stockindicators.dev](https://dotnet.stockindicators.dev) |
| **Demo site** | [charts.stockindicators.dev](https://charts.stockindicators.dev) |

An alternate Python variant of this library is available at [python.stockindicators.dev](https://python.stockindicators.dev).

## Attributions

This NuGet library project is developed with support from [community contributors](/#contribute-to-help-others).

Our documentation, examples, and testing is additionally built upon the work of several excellent open-source projects and data providers.

::: tip Documentation and testing only
The `FacioQuo.Stock.Indicators` NuGet library itself does not contain work products or inherit licensing from any of these providers.
:::

### Chart libraries

- **[indy-charts](https://github.com/facioquo/stock-charts)** - Reusable stock indicator chart components used throughout this documentation. Developed by [facioquo](https://github.com/facioquo) and licensed under the Apache License 2.0.
- **[Chart.js](https://www.chartjs.org)** - Underlying charting library used by chart components (including our [demo site](https://charts.stockindicators.dev)). Licensed under the MIT License.

### Data providers and SDK libraries

- **[Alpaca Markets](https://alpaca.markets)** - Market data used for bars in our [demo site](https://charts.stockindicators.dev) and examples projects.
- **[Coinbase](https://www.coinbase.com)** - Live cryptocurrency market data via WebSocket feeds for real-time testing.
- **[JKorf/Coinbase.Net](https://github.com/JKorf/Coinbase.Net)** - .NET client library for Coinbase WebSocket API used for live market data streaming in testing tools. Licensed under the MIT License.

### Development tools and frameworks

- **[VitePress](https://vitepress.dev)** - Static site generator used for this documentation website (Vue.js and Vite).
- **[BenchmarkDotNet](https://benchmarkdotnet.org)** - Performance benchmarking framework used for indicator optimization and regression testing. Licensed under the MIT License.
- **[Cloudflare Pages](https://pages.cloudflare.com)** - Documentation hosting and deployment platform.

## Community and support

### Get help

- [GitHub Discussions](https://github.com/facioquo/stock-indicators-dotnet/discussions) - Ask questions and share ideas
- [GitHub Issues](https://github.com/facioquo/stock-indicators-dotnet/issues) - Report bugs or request features
- [Twitter/X](https://twitter.com/messages/compose?recipient_id=27475431) - Direct message @daveskender

### Contributing

This library is maintained by [Dave Skender](https://github.com/DaveSkender) and community contributors. If you'd like to contribute:

- Review the [contributing guidelines](/contributing)
- Check the [project backlog](https://github.com/orgs/facioquo/projects/27)
- Submit pull requests with improvements

Special thanks to all [community contributors](https://github.com/facioquo/stock-indicators-dotnet/graphs/contributors) who have helped improve this library.

### Support the project

If you find this library useful, consider:

- Adding a ⭐ on [GitHub](https://github.com/facioquo/stock-indicators-dotnet)
- [Becoming a sponsor](https://github.com/sponsors/facioquo)
- Sharing the library with others

## Technical details

### Target frameworks

The library targets actively supported Microsoft .NET platforms:

- .NET 10.0
- .NET 9.0
- .NET 8.0

The compiled library is [Common Language Specification (CLS) compliant](https://docs.microsoft.com/en-us/dotnet/standard/common-type-system) and can be used in other .NET languages.

### Quality standards

This library follows strict quality standards:

- Mathematical precision validation against authoritative sources and against manually calculated results.
- Comprehensive unit test and regression test coverage
- Performance testing and benchmarking with `BenchmarkDotNet`
- Continuous integration and automated testing
- Designed with a balanced set of [guiding principles](https://github.com/facioquo/stock-indicators-dotnet/discussions/648)

## Resources and references

Technical indicator formulas and methodologies are based on industry-standard references from [reputable authors](https://github.com/facioquo/stock-indicators-dotnet/discussions/1024) including:

- Published financial analysis textbooks
- Academic papers on technical analysis
- Established trading platforms and charting software
- Community-validated calculations

Specific references are documented in individual indicator pages where applicable.

## Related projects

- [Stock Indicators for Python](https://python.stockindicators.dev) - Python port
- [Demo chart site](https://charts.stockindicators.dev) - Interactive visualization
