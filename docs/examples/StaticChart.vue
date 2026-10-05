<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'

import {
  OverlayChart,
  loadStaticQuotes,
  setupIndyCharts,
  type Bar
} from '@facioquo/indy-charts'

import type { ChartDataset, ScatterDataPoint } from 'chart.js'

import { SAMPLE_QUOTES } from './sample-quotes'

const quotes = loadStaticQuotes(SAMPLE_QUOTES)

const canvasEl = ref<HTMLCanvasElement | null>(null)
let overlayChart: OverlayChart | null = null
let observer: MutationObserver | null = null
let lastIsDark: boolean | null = null

function isDark(): boolean {
  return document.documentElement.classList.contains('dark')
}

function computeEma(closes: number[], period: number): number[] {
  if (!Number.isInteger(period) || period <= 0) {
    throw new Error(`EMA period must be a positive integer, got ${period}`)
  }
  const k = 2 / (period + 1)
  const result: number[] = new Array(closes.length).fill(NaN)
  if (closes.length < period) return result

  let sum = 0
  for (let i = 0; i < period; i++) sum += closes[i]
  result[period - 1] = sum / period

  for (let i = period; i < closes.length; i++) {
    result[i] = closes[i] * k + result[i - 1] * (1 - k)
  }
  return result
}

function buildEmaDataset(
  quotes: Bar[],
  period: number
): ChartDataset<'line', ScatterDataPoint[]> {
  const ema = computeEma(quotes.map(q => q.close), period)
  return {
    type: 'line',
    label: `EMA(${period})`,
    data: quotes.map((q, i) => ({ x: new Date(q.timestamp).valueOf(), y: ema[i] })),
    yAxisID: 'y',
    borderColor: '#FFA726',
    backgroundColor: '#FFA726',
    borderWidth: 1.5,
    pointRadius: 0,
    fill: false,
    spanGaps: false,
    order: 0
  }
}

function renderChart(): void {
  if (!canvasEl.value) return
  setupIndyCharts()
  // OverlayChart.destroy() releases the wrapper state and the underlying Chart.js instance.
  overlayChart?.destroy()
  const dark = isDark()
  overlayChart = new OverlayChart(canvasEl.value, {
    isDarkTheme: dark,
    showTooltips: false
  })
  overlayChart.render(quotes)
  lastIsDark = dark

  // Push an EMA(20) line onto the candlestick + volume chart.
  overlayChart.chart?.data.datasets.push(buildEmaDataset(quotes, 20))
  overlayChart.chart?.update('none')
}

function onHtmlClassChange(): void {
  // VitePress mutates `<html>`'s class for things other than dark-mode toggle
  // (nav-open state, scroll lock, etc.). Re-render only when the theme actually
  // flips, otherwise every interaction would rebuild the chart.
  const dark = isDark()
  if (dark === lastIsDark) return
  renderChart()
}

onMounted(() => {
  renderChart()
  observer = new MutationObserver(onHtmlClassChange)
  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class']
  })
})

onBeforeUnmount(() => {
  observer?.disconnect()
  observer = null
  overlayChart?.destroy()
  overlayChart = null
})
</script>

<template>
  <div class="static-chart-wrap">
    <canvas ref="canvasEl" />
  </div>
</template>

<style scoped>
.static-chart-wrap {
  position: relative;
  width: 100%;
  aspect-ratio: 2.5;
}

@media (width <= 880px) {
  .static-chart-wrap {
    aspect-ratio: 2;
  }
}
</style>
