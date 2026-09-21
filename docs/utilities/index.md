---
title: Utilities and helpers
description: The Stock Indicators for .NET library includes utilities to help you use and transform historical price bars and indicator results, and to create custom indicators.
layout: home

hero:
  name: Utilities and helpers
  tagline: Tools to prepare bars, process results, and build custom indicators
  actions:
    - theme: alt
      text: Indicators reference
      link: /indicators
    - theme: alt
      text: Indicator catalog
      link: /utilities/catalog

features:

  - title: Utilities for price bars
    details: Prepare and transform historical price bars before using them with indicators.
    link: /utilities/bars
    linkText: 6 utilities

  - title: Utilities for indicator results
    details: Work with indicator results after calculation and analysis.
    link: /utilities/results
    linkText: 4 utilities

  - title: Additional utilities
    details: Numerical methods and math helpers for creating custom indicators.
    link: /utilities/helpers
    linkText: Slope, StdDev, NullMath, DeMath

  - title: Indicator catalog utility
    details: Programmatic access to indicator metadata for dynamic UIs.
    link: /utilities/catalog
    linkText: Learn more
---

## [Utilities for price bars](/utilities/bars)

- [use alternate price](/utilities/bars#use-alternate-price)
- [sort price bars](/utilities/bars#sort-bars)
- [validate bar history](/utilities/bars#validate-bar-history)
- [resize price bar history](/utilities/bars#resize-bar-history)
- [streaming aggregator hubs](/utilities/bars#streaming-aggregator-hubs)
- [extended candle properties](/utilities/bars#extended-candle-properties)

## [Utilities for indicator results](/utilities/results)

- [remove warmup periods](/utilities/results#remove-warmup-periods)
- [condense](/utilities/results#condense)
- [find by date](/utilities/results#find-by-date)
- [sort results](/utilities/results#sort-results).

## [Additional helper utilities](/utilities/helpers)

Numerical and math tools for [custom indicator development](/guide/customization):

- [numerical methods](/utilities/helpers#numerical-methods) (slope, standard deviation)
- [`NullMath`](/utilities/helpers#nullmath) null-safe math operations
- [`DeMath`](/utilities/helpers#demath) deterministic precision math

## [Indicator catalog utility](/utilities/catalog)

Use the [Indicator catalog](/utilities/catalog) to discover indicator metadata programmatically — build pickers and configuration UIs, or execute a selected indicator without hard-coding its method call.
