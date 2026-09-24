---
title: Indicator styles
description: Three distinct indicator styles to support different use cases.
layout: home

hero:
  name: Indicator styles
  tagline: Choose the right one for your use case
  actions:
    - theme: alt
      text: Indicators reference
      link: /indicators
    - theme: alt
      text: v2→v3 migration
      link: /migration/v3

features:
  - title: Batch (Series)
    details: Convert full bar collections to indicators, best for once-and-done bulk conversions.
    link: /guide/styles/batch
    linkText: Learn more

  - title: Buffer lists
    details: Standalone incrementing `IReadOnlyList` results you append to, best for simple self-managed incremental data.
    link: /guide/styles/buffer
    linkText: Learn more

  - title: Stream hubs
    details: Subscription-based hub-observer pattern, best for streaming/live data and advanced architectures.
    link: /guide/styles/stream
    linkText: Learn more
    
---

## Feature comparison

The library provides three distinct indicator styles to support different use cases.

| Feature | [Batch (Series)](/guide/styles/batch) | [Buffer lists](/guide/styles/buffer) | [Stream hubs](/guide/styles/stream) |
| ------- | ------------ | ------------ | ----------- |
| Incrementing | no | yes | yes |
| Batch speed | fastest | faster | fast |
| Scaling | low | moderate | high |
| Class type | static | instance | instance |
| Base interface | `IReadOnlyList` | `IReadOnlyList` (+ `Add`) | `IStreamHub` |
| Complexity | lowest | moderate | highest |
| Chainable | yes | manual | yes |
| Pruning | with utility | auto-preset | auto-preset |
| Healing | no | no | yes |

Healing: Built-in handling of out-of-order and de-deplication of incoming data.

## Which style to use?

Start with **Batch (Series)** style unless you have a specific need for incremental processing or real-time streaming.

- Use **[Batch](/guide/styles/batch)** when you have a complete historical dataset and need to calculate indicators once. Fastest and simplest.
- Use **[Buffer lists](/guide/styles/buffer)** when bars arrive one at a time and you need incremental processing without the overhead of a full hub infrastructure.
- Use **[Stream hubs](/guide/styles/stream)** when you need coordinated, automatic updates across multiple chained indicators from a live data feed with self-healing.

### Bars that arrive one at a time

Don't re-run a Series method for every new bar. Its cost grows with the history it recalculates, while a stream hub's cost per new bar stays flat, and the hub gets cheaper after only a few dozen bars of history. Measured for EMA, SMA, RSI and MACD, the Series method catches up with the hub at about 12 to 25 bars. At 1,000 bars it costs 40 to 110 times as much per bar. To measure it on your own hardware, run the `StreamCrossover` benchmark in `tools/performance`.

## Getting started

See [Getting started](/guide/getting-started) for installation, first steps, and example usage.
