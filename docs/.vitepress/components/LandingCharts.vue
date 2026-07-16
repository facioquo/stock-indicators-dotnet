<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

import {
  ChartManager,
  createApiClient,
  createDefaultSelection,
  loadStaticIndicatorData,
  setupIndyCharts,
  type IndicatorListing,
  type IndicatorSelection
} from '@facioquo/indy-charts'

import { DARK_SURFACE, LIGHT_SURFACE } from '../theme/chart-theme'
import { CHART_API_BASE_URL, CHART_API_RESILIENCE } from '../theme/chart-api'

const BAR_COUNT = 250
const EMA_FAST_COLOR = '#ff4d8d'
const EMA_SLOW_COLOR = '#26c6da'
const LINEAR_COLOR = '#ff7f11'
const MARUBOZU_COLOR = '#9aa5b1'

const overlayCanvas = ref<HTMLCanvasElement | null>(null)
const phase = ref<'idle' | 'loading' | 'ready' | 'error'>('idle')
const errorMessage = ref('Unable to load chart preview.')
const chartBackground = ref(readThemeBackground())

let chartManager: ChartManager | null = null
let themeObserver: MutationObserver | null = null
let resizeHandler: (() => void) | null = null
let disposed = false
let loadToken = 0

interface ChartSpec {
  uiid: string
  label: string
  params?: Record<string, number>
  colors?: string[]
}

const overlaySpecs: ChartSpec[] = [
  { uiid: 'Ema', label: 'EMA(200)', params: { lookbackPeriods: 200 }, colors: [EMA_SLOW_COLOR] },
  { uiid: 'Ema', label: 'EMA(50)', params: { lookbackPeriods: 50 }, colors: [EMA_FAST_COLOR] },
  { uiid: 'LINEAR', label: 'LINEAR(30)', params: { lookbackPeriods: 30 }, colors: [LINEAR_COLOR] },
  { uiid: 'MARUBOZU', label: 'MARUBOZU(90%)', params: { minBodyPercent: 90 }, colors: [MARUBOZU_COLOR] },
  { uiid: 'ATR-STOP-CLOSE', label: 'ATR-STOP(21,3,CLOSE)', params: { lookbackPeriods: 21, multiplier: 3 } }
]

function isDark(): boolean {
  return typeof document !== 'undefined' && document.documentElement.classList.contains('dark')
}

// Returns the translucent backdrop colour for axis/annotation labels.
// We deliberately do NOT read `--vp-c-bg` here — that variable is solid
// and would obscure the chart data behind the labels.
function readThemeBackground(): string {
  return isDark() ? DARK_SURFACE : LIGHT_SURFACE
}

function currentSettings() {
  return {
    isDarkTheme: isDark(),
    showTooltips: false,
    background: chartBackground.value
  }
}

function destroyChart(): void {
  chartManager?.destroy()
  chartManager = null
}

function updateTheme(): void {
  chartManager?.updateTheme(currentSettings())
}

function normalizeSelection(selection: IndicatorSelection, label: string): IndicatorSelection {
  selection.label = label
  selection.results.forEach((result) => {
    result.label = label
  })
  return selection
}

function applySeriesColors(selection: IndicatorSelection, colors?: string[]): void {
  if (!colors?.length) return
  selection.results.forEach((result, index) => {
    const color = colors[index]
    if (color) {
      result.color = color
    }
  })
}

function findListing(listings: IndicatorListing[], uiid: string): IndicatorListing | undefined {
  return listings.find((listing) => listing.uiid.toLowerCase() === uiid.toLowerCase())
}

async function renderCharts(): Promise<void> {
  const token = ++loadToken
  phase.value = 'loading'
  errorMessage.value = 'Unable to load chart preview.'
  chartBackground.value = readThemeBackground()
  destroyChart()

  try {
    const canvas = overlayCanvas.value
    if (!canvas) {
      throw new Error('Overlay chart canvas is not available.')
    }

    setupIndyCharts()

    const client = createApiClient({ baseUrl: CHART_API_BASE_URL, ...CHART_API_RESILIENCE })
    const [quotes, listings] = await Promise.all([client.getQuotes(), client.getListings()])
    if (disposed || token !== loadToken) return

    const manager = new ChartManager({ settings: currentSettings() })
    chartManager = manager
    manager.initializeOverlay(canvas, quotes, BAR_COUNT)

    for (const spec of overlaySpecs) {
      const listing = findListing(listings, spec.uiid)
      if (!listing) {
        throw new Error(`Indicator listing not found for uiid "${spec.uiid}".`)
      }

      const selection = normalizeSelection(
        createDefaultSelection(listing, spec.params, 'landing-home-'),
        spec.label
      )
      applySeriesColors(selection, spec.colors)
      const data = loadStaticIndicatorData(await client.getSelectionData(selection, listing))
      if (disposed || token !== loadToken) return

      manager.processSelectionData(selection, listing, data)
      manager.displaySelection(selection, listing)
    }

    if (disposed || token !== loadToken) return
    phase.value = 'ready'
    await Promise.resolve()
    manager.resize()
  } catch (error) {
    if (disposed || token !== loadToken) return
    destroyChart()
    phase.value = 'error'
    errorMessage.value = error instanceof Error ? error.message : errorMessage.value
  }
}

onMounted(() => {
  void renderCharts()

  themeObserver = new MutationObserver(() => {
    chartBackground.value = readThemeBackground()
    updateTheme()
  })
  themeObserver.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class']
  })

  resizeHandler = () => {
    chartManager?.resize()
  }
  window.addEventListener('resize', resizeHandler)
})

onBeforeUnmount(() => {
  disposed = true
  loadToken += 1
  themeObserver?.disconnect()
  themeObserver = null
  if (resizeHandler) {
    window.removeEventListener('resize', resizeHandler)
    resizeHandler = null
  }
  destroyChart()
})
</script>

<template>
  <section class="home-charts-stack" data-testid="landing-charts-root" :data-state="phase">
    <div class="indy-demo__canvas-wrap indy-demo__canvas-wrap--overlay">
      <canvas
        ref="overlayCanvas"
        class="indy-demo__canvas"
        data-testid="landing-charts-overlay-canvas"
      />
      <div
        v-if="phase === 'loading'"
        class="indy-demo__status indy-demo__status--loading home-charts-stack__status"
      >
        Loading chart preview...
      </div>
      <div
        v-else-if="phase === 'error'"
        class="indy-demo__status indy-demo__status--error home-charts-stack__status"
      >
        {{ errorMessage }}
      </div>
    </div>

    <StockIndicatorChart indicator="Macd" id="landing-macd" :bar-count="BAR_COUNT" />
    <StockIndicatorChart indicator="Stc" id="landing-stc" :bar-count="BAR_COUNT" />
  </section>
</template>

<style scoped>
/* Stretch the loading/error message across the canvas instead of sitting beneath it. */
.home-charts-stack__status {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
</style>
