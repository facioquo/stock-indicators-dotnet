import { test, expect, type Page } from '@playwright/test'

import { getTestIdPrefix } from '@facioquo/indy-charts/vue'

import { indicatorPages } from './chart-pages'
import {
  serveChartsFromSnapshot,
  CHART_MARKERS,
  CHART_TERMINAL_SELECTOR,
  type ChartPhase,
} from './chart-helpers'

/**
 * Wait for a chart to reach a terminal state (ready, empty, or error).
 * Returns the phase that was reached.
 *
 * Both the wait and the phase check derive from `CHART_MARKERS`, so a new
 * terminal state added there is waited for here and scanned by the a11y suite
 * without either file being edited.
 */
async function waitForChartPhase(page: Page, testId: string): Promise<ChartPhase> {
  const root = page.locator(`[data-testid="${testId}-root"]`)
  await expect(root).toBeVisible({ timeout: 15_000 })

  // Wait for one terminal UI marker to become visible — atomic, avoids the DOM-present
  // but not-yet-visible race that `waitForFunction` with querySelector can hit.
  await root.locator(CHART_TERMINAL_SELECTOR).first()
    .waitFor({ state: 'visible', timeout: 20_000 })

  // Error before ready: prevents a residual canvas from masking an error state.
  for (const phase of ['error', 'empty', 'ready'] as const) {
    if (await root.locator(CHART_MARKERS[phase]).first().isVisible()) return phase
  }

  throw new Error(`Chart ${testId} did not reach a terminal state`)
}

// ---------------------------------------------------------------------------
// Overlay chart — SMA on the SMA indicator page
// ---------------------------------------------------------------------------

test('SMA overlay chart renders from the snapshot', async ({ page }) => {
  await serveChartsFromSnapshot(page)
  await page.goto('/indicators/sma')

  const prefix = getTestIdPrefix('Sma')
  const root = page.locator(`[data-testid="${prefix}-root"]`)
  await expect(root).toBeVisible({ timeout: 15_000 })

  const phase = await waitForChartPhase(page, prefix)
  expect(phase, `Expected chart to be ready but got "${phase}"`).toBe('ready')

  const canvas = root.locator(`[data-testid="${prefix}-overlay-canvas"]`)
  await expect(canvas).toBeVisible()
  await expect(canvas).toHaveAttribute('width')
})

// ---------------------------------------------------------------------------
// Oscillator chart — RSI on the RSI indicator page
// ---------------------------------------------------------------------------

test('RSI oscillator chart renders from the snapshot', async ({ page }) => {
  await serveChartsFromSnapshot(page)
  await page.goto('/indicators/rsi')

  const prefix = getTestIdPrefix('Rsi')
  const root = page.locator(`[data-testid="${prefix}-root"]`)
  await expect(root).toBeVisible({ timeout: 15_000 })

  const phase = await waitForChartPhase(page, prefix)
  expect(phase, `Expected chart to be ready but got "${phase}"`).toBe('ready')

  // Oscillator must show both overlay (price) and oscillator (RSI) canvases
  await expect(root.locator(`[data-testid="${prefix}-overlay-canvas"]`)).toBeVisible()
  await expect(root.locator(`[data-testid="${prefix}-oscillator-canvas"]`)).toBeVisible()
})

// ---------------------------------------------------------------------------
// Home page charts — BollingerBands, Macd, Stc
// ---------------------------------------------------------------------------

test('Home page charts render from the snapshot', async ({ page }) => {
  await serveChartsFromSnapshot(page)
  await page.goto('/')

  await expect(page.getByTestId('landing-charts-root')).toBeVisible({ timeout: 15_000 })
  await expect(page.getByTestId('landing-charts-root')).toHaveAttribute('data-state', 'ready', { timeout: 15_000 })
  await expect(page.getByTestId('landing-charts-overlay-canvas')).toBeVisible({ timeout: 15_000 })
  await expect(page.getByTestId('landing-charts-overlay-canvas')).toHaveAttribute('width')

  for (const id of ['landing-macd', 'landing-stc']) {
    const prefix = getTestIdPrefix(id)
    const root = page.locator(`[data-testid="${prefix}-root"]`)
    await expect(root).toBeVisible({ timeout: 15_000 })
    const phase = await waitForChartPhase(page, prefix)
    expect(phase, `Expected ${id} to be ready but got "${phase}"`).toBe('ready')
  }
})

// ---------------------------------------------------------------------------
// Bulk smoke test — every indicator page must render its chart with data
// ---------------------------------------------------------------------------

const INDICATOR_PAGES = indicatorPages()

for (const { page: pageName, indicator } of INDICATOR_PAGES) {
  test(`${pageName} - ${indicator} indicator page chart renders from the snapshot`, async ({ page }) => {
    await serveChartsFromSnapshot(page)
    await page.goto(`/indicators/${pageName}`)

    const prefix = getTestIdPrefix(indicator)
    const root = page.locator(`[data-testid="${prefix}-root"]`)
    await expect(root).toBeVisible({ timeout: 15_000 })

    const phase = await waitForChartPhase(page, prefix)

    // The snapshot holds data for every chart, so anything but ready is a gap
    // in it (or in the catalog) rather than an acceptable state.
    expect(phase, `Expected ${pageName} - ${indicator} to be ready but got "${phase}"`).toBe('ready')
  })
}
