import type { Page, Route } from '@playwright/test'
import { readFileSync } from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

const __dirname = dirname(fileURLToPath(import.meta.url))
const FIXTURES = join(__dirname, '../.vitepress/public/data/chart-api')

// Static fixture data loaded once
const quotesJson = readFileSync(join(FIXTURES, 'quotes.json'), 'utf8')
const indicatorsJson = readFileSync(join(FIXTURES, 'indicators.json'), 'utf8')
const smaJson = readFileSync(join(FIXTURES, 'sma.json'), 'utf8')
const rsiJson = readFileSync(join(FIXTURES, 'rsi.json'), 'utf8')

/**
 * Intercept all stock-charts API requests and respond with static fixture data,
 * so suites are hermetic and never depend on the live API.
 *
 * The API serves indicator data from per-indicator endpoints keyed by UIID
 * (e.g. `/SMA/`, `/RSI/`, `/MACD/`) — NOT `/indicators/<name>`. The listings
 * fixture (`indicators.json`) carries those absolute endpoints, so the client
 * requests `/<UIID>/` and the routes below must match that shape.
 *
 * Every pattern is anchored to the API host. Bare path patterns would also
 * match same-named routes on the docs site itself — `/indicators` is a real
 * page — and would answer that document request with JSON.
 */
const API = 'charts-api\\.stockindicators\\.dev'

export async function mockStockChartsApi(page: Page): Promise<void> {
  // Routes are matched LIFO (last-registered = highest priority). This catch-all
  // is registered first, so every specific route below shadows it. Any indicator
  // endpoint we don't explicitly fixture returns an empty array → the chart
  // reaches the (tolerated) empty state instead of touching the network.
  await page.route(new RegExp(`${API}/.+`), (route: Route) =>
    route.fulfill({ contentType: 'application/json', body: '[]' })
  )

  await page.route(new RegExp(`${API}/quotes(?:\\?|$)`), (route: Route) =>
    route.fulfill({ contentType: 'application/json', body: quotesJson })
  )

  await page.route(new RegExp(`${API}/indicators(?:\\?|$)`), (route: Route) =>
    route.fulfill({ contentType: 'application/json', body: indicatorsJson })
  )

  // SMA indicator data — endpoint is `/SMA/?lookbackPeriods=...`
  await page.route(new RegExp(`${API}/SMA/`, 'i'), (route: Route) =>
    route.fulfill({ contentType: 'application/json', body: smaJson })
  )

  // RSI indicator data — endpoint is `/RSI/?lookbackPeriods=...`
  await page.route(new RegExp(`${API}/RSI/`, 'i'), (route: Route) =>
    route.fulfill({ contentType: 'application/json', body: rsiJson })
  )
}

/**
 * Selector matching any terminal state of a chart: rendered, empty, or errored.
 * Waiting on this is the web-first way to know a chart has stopped changing.
 */
export const CHART_TERMINAL_SELECTOR =
  '[data-testid$="-overlay-canvas"], [data-testid$="-empty"], [data-testid$="-error"]'
