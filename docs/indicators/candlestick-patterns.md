---
title: Candlestick patterns
description: Chart patterns derived from candle shapes and price formations.
layout: home

hero:
  name: Candlestick patterns
  tagline: Chart patterns derived from candle shapes and price formations.
  actions:
    - theme: alt
      text: ← all categories
      link: /indicators/
    - theme: alt
      text: volume-based →
      link: /indicators/volume-based

features:

  - title: Doji
    details: Indecision candle pattern where open and close price are virtually identical
    icon:
      src: /assets/thumbs/indicators/doji.png
    link: /indicators/doji

  - title: Marubozu
    details: Uni-directional solid-body momentum candle pattern with consistent price movement
    icon:
      src: /assets/thumbs/indicators/marubozu.png
    link: /indicators/marubozu

  - title: Pivots
    details: Peak and trough chevron patterns with configurable periods wings and higher high, lower low identification
    icon:
      src: /assets/thumbs/indicators/pivots.png
    link: /indicators/pivots

  - title: Williams Fractal
    details: Peak and trough chevron patterns with ±2 fixed period wings for short term pivots
    icon:
      src: /assets/thumbs/indicators/fractal.png
    link: /indicators/fractal
    
---

## About candlestick patterns

Candlestick patterns uniquely share a common `IReadOnlyList<CandleResult>` response type.

<!--@include: ../shared/candle-result.md-->
