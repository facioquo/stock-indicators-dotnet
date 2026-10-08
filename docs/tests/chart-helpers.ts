import type { Page, Route } from '@playwright/test'

/**
 * Make the live chart API unavailable so every chart is served by the library's
 * own offline fallback from the committed snapshot (`pnpm run snapshot:charts`),
 * the same files and code path production uses when the API is gone. Suites are
 * hermetic and never depend on the live API.
 *
 * Requests fail with a 404 rather than a connection error: the client does not
 * retry it, so the fallback answers at once instead of after a backoff window.
 *
 * The pattern is anchored to the API host. A bare path pattern would also match
 * same-named routes on the docs site itself — `/indicators` is a real page.
 */
const API = 'charts-api\\.stockindicators\\.dev'

export async function serveChartsFromSnapshot(page: Page): Promise<void> {
  await page.route(new RegExp(`${API}/`), (route: Route) =>
    route.fulfill({ status: 404, contentType: 'application/json', body: '{}' })
  )
}

/**
 * The terminal states a chart can settle into, and the marker that identifies
 * each. This is the single definition both suites derive from: `charts.spec.ts`
 * needs the markers individually to report which state was reached, while
 * `a11y.spec.ts` only needs to know a chart stopped changing. Adding a state
 * here reaches both automatically.
 */
export const CHART_MARKERS = {
  // Any pane: an oscillator-only chart draws no overlay canvas.
  ready: 'canvas[data-testid*="-canvas"]',
  empty: '[data-testid$="-empty"]',
  error: '[data-testid$="-error"]',
} as const

export type ChartPhase = keyof typeof CHART_MARKERS

/**
 * Selector matching any terminal state of a chart: rendered, empty, or errored.
 * Waiting on this is the web-first way to know a chart has stopped changing.
 */
export const CHART_TERMINAL_SELECTOR = Object.values(CHART_MARKERS).join(', ')
