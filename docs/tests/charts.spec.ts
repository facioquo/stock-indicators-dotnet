import { test, expect, type Page } from '@playwright/test'
import { readdirSync, readFileSync } from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

import { getTestIdPrefix } from '@facioquo/indy-charts/vue'

import {
  mockStockChartsApi,
  CHART_MARKERS,
  CHART_TERMINAL_SELECTOR,
  type ChartPhase,
} from './chart-api-mock'

const __dirname = dirname(fileURLToPath(import.meta.url))

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
    if (await root.locator(CHART_MARKERS[phase]).isVisible()) return phase
  }

  throw new Error(`Chart ${testId} did not reach a terminal state`)
}

/**
 * Read the data-error-kind attribute on the error block, when phase === 'error'.
 * Returns 'author' for misconfigured registries / missing uiids / missing setup,
 * 'data' for upstream API or fixture issues.
 */
async function getErrorKind(page: Page, testId: string): Promise<string | null> {
  return page
    .locator(`[data-testid="${testId}-error"]`)
    .getAttribute('data-error-kind')
}

// ---------------------------------------------------------------------------
// Overlay chart — SMA on the SMA indicator page
// ---------------------------------------------------------------------------

test('SMA overlay chart renders from static fixture data', async ({ page }) => {
  await mockStockChartsApi(page)
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

test('RSI oscillator chart renders from static fixture data', async ({ page }) => {
  await mockStockChartsApi(page)
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

test('Home page charts reach a terminal state', async ({ page }) => {
  await mockStockChartsApi(page)
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
    expect(['ready', 'empty']).toContain(phase)
  }
})

// ---------------------------------------------------------------------------
// Bulk smoke test — every indicator page must reach a non-author terminal state
// ---------------------------------------------------------------------------

function indicatorPages(): Array<{ page: string; indicator: string }> {
  const indicatorsDir = join(__dirname, '../indicators')
  const files = readdirSync(indicatorsDir)

  return files
    .filter((file) => file.endsWith('.md'))
    .flatMap((file) => {
      const body = readFileSync(join(indicatorsDir, file), 'utf8')
      // Matches only self-closing <StockIndicatorChart indicator="..." ... />
      // forms; all current pages use this form. Broaden the regex if the
      // long form <StockIndicatorChart ...></StockIndicatorChart> ever appears.
      const matches = [...body.matchAll(/<StockIndicatorChart indicator="([^"]+)"[^/]*\/>/g)]
      if (matches.length === 0) return []

      const page = file.replace(/\.md$/, '')
      return matches.map((m) => ({ page, indicator: m[1] }))
    })
}

const INDICATOR_PAGES = indicatorPages()

for (const { page: pageName, indicator } of INDICATOR_PAGES) {
  test(`${pageName} - ${indicator} indicator page chart reaches terminal state`, async ({ page }) => {
    await mockStockChartsApi(page)
    await page.goto(`/indicators/${pageName}`)

    const prefix = getTestIdPrefix(indicator)
    const root = page.locator(`[data-testid="${prefix}-root"]`)
    await expect(root).toBeVisible({ timeout: 15_000 })

    const phase = await waitForChartPhase(page, prefix)

    if (phase === 'error') {
      // Author-facing errors (typoed catalog, missing uiid, missing setup) must
      // never reach CI. Data errors (offline fixture gap) are tolerated.
      const kind = await getErrorKind(page, prefix)
      expect(
        kind,
        `Author-facing error on ${pageName} — fix the catalog entry or fixture`
      ).toBe('data')
    } else {
      expect(['ready', 'empty']).toContain(phase)
    }
  })
}
