---
title: Custom chart (bring your own data)
description: Render a candlestick + volume chart from your own quote data using OverlayChart, with a locally computed EMA(20) overlay - no API required.
---

<script setup>
import StaticChart from './StaticChart.vue'
</script>

# Custom chart (bring your own data)

Render a candlestick + volume chart **and** a technical indicator overlay directly from your own quote data - no API required. Useful for offline demos, internal dashboards, or static documentation sites.

## Live demo

<ClientOnly>
  <StaticChart />
</ClientOnly>

The chart above plots OHLC + volume from a hard-coded `Quote[]` array, with an EMA(20) line computed locally. Everything ships in the page bundle - no network calls; see [`docs/examples/StaticChart.vue`](https://github.com/facioquo/stock-indicators-dotnet/blob/main/docs/examples/StaticChart.vue) for the source.

## How it works

[`OverlayChart`](https://github.com/facioquo/stock-charts/tree/main/libs/indy-charts) is a lower-level building block exported from `@facioquo/indy-charts` for rendering candlestick + volume charts directly from your own data, with no API required. `@facioquo/indy-charts` is maintained in the [facioquo/stock-charts](https://github.com/facioquo/stock-charts) repository, not here — see its [README](https://github.com/facioquo/stock-charts/blob/main/libs/indy-charts/README.md) for the full usage guide and API reference.

::: tip ✨ Tip: direct lower-level use is opt-in
For most pages you should use the higher-level `<StockIndicatorChart>` from `@facioquo/indy-charts/vue` (registered globally in `.vitepress/theme/index.ts`). It fetches quotes + indicators from the configured API, manages its own lifecycle, and respects the central indicator catalog. Drop down to `OverlayChart` only when you genuinely need to ship data inline.
:::
