import { test, expect, type Page } from '@playwright/test'

import { getTestIdPrefix } from '@facioquo/indy-charts/vue'

import { indicatorPages } from './chart-pages'
import { CHART_MARKERS } from './chart-api-mock'

/**
 * Acceptance test for the offline guarantee: with the chart API unreachable and
 * nothing cached, a first-time visitor still sees every chart rendered with data
 * from the snapshot bundled with the site.
 *
 * Playwright gives each test a fresh context, so no storage carries over. The API
 * host is aborted outright; the only data source left is `/data/chart-api`.
 */
const API_HOST = 'charts-api.stockindicators.dev'
const SNAPSHOT_PREFIX = '/data/chart-api/'

// Every aborted request is retried with backoff before the snapshot is read, and
// the landing page makes seven in sequence.
test.describe.configure({ timeout: 60_000 })

interface Traffic {
  apiRequests: number
  snapshotResponses: string[]
}

async function blockApi(page: Page): Promise<Traffic> {
  const traffic: Traffic = { apiRequests: 0, snapshotResponses: [] }

  await page.route(new RegExp(`^https?://${API_HOST.replaceAll('.', '\\.')}/`), (route) => {
    traffic.apiRequests += 1
    return route.abort('connectionrefused')
  })
  page.on('response', (response) => {
    const { pathname } = new URL(response.url())
    if (pathname.startsWith(SNAPSHOT_PREFIX) && response.ok()) {
      traffic.snapshotResponses.push(pathname)
    }
  })

  return traffic
}

// A chart with an oscillator pane but no overlay draws no overlay canvas, so
// `CHART_MARKERS.ready` alone would miss it.
const ANY_CANVAS = 'canvas[data-testid*="-canvas"]'

/** A chart that is rendered with data: a canvas is up and no status block shows. */
async function expectChartWithData(page: Page, prefix: string): Promise<void> {
  const root = page.locator(`[data-testid="${prefix}-root"]`)
  await expect(root).toBeVisible({ timeout: 15_000 })
  await expect(root.locator(ANY_CANVAS).first()).toBeVisible({ timeout: 30_000 })
  await expect(root.locator(CHART_MARKERS.empty)).toHaveCount(0)
  await expect(root.locator(CHART_MARKERS.error)).toHaveCount(0)
}

function expectServedFromSnapshot(traffic: Traffic): void {
  expect(traffic.apiRequests, 'the page never tried the live API').toBeGreaterThan(0)
  expect(traffic.snapshotResponses).toContain(`${SNAPSHOT_PREFIX}quotes.json`)
  expect(traffic.snapshotResponses).toContain(`${SNAPSHOT_PREFIX}indicators.json`)
}

test('home page charts render from the snapshot with the API gone', async ({ page }) => {
  const traffic = await blockApi(page)
  await page.goto('/')

  const landing = page.getByTestId('landing-charts-root')
  await expect(landing).toHaveAttribute('data-state', 'ready', { timeout: 30_000 })
  await expect(page.getByTestId('landing-charts-overlay-canvas')).toBeVisible()

  for (const id of ['landing-macd', 'landing-stc']) {
    await expectChartWithData(page, getTestIdPrefix(id))
  }
  expectServedFromSnapshot(traffic)
})

for (const { page: pageName, indicator } of indicatorPages()) {
  test(`${pageName} - ${indicator} chart renders from the snapshot with the API gone`, async ({ page }) => {
    const traffic = await blockApi(page)
    await page.goto(`/indicators/${pageName}`)

    await expectChartWithData(page, getTestIdPrefix(indicator))
    expectServedFromSnapshot(traffic)
  })
}
