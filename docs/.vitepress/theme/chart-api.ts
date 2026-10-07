// Shared chart-API connection settings. Single source of truth so the VitePress
// theme (every <StockIndicatorChart> instance) and the landing overlay talk to
// the same endpoint with identical resilience — mirrors chart-theme.ts.
//
// A failed request resolves in this order (indy-charts 0.13):
//   • retry — transient network / 5xx / 429 failures are retried with backoff.
//     On by default (3 attempts, 500 ms base), so it is intentionally unset here.
//   • staleCache — each successful response is cached in sessionStorage and
//     served when a later refetch exhausts its retries.
//   • offlineFallback — the snapshot committed under public/data/chart-api,
//     the only source that survives a visitor with an empty cache and a dead
//     API. Regenerate it with `pnpm run snapshot:charts`.

import type { ApiClientConfig } from '@facioquo/indy-charts'

export const CHART_API_BASE_URL = 'https://charts-api.stockindicators.dev'

/** Note in the dev console when a chart falls back to the last-good cache. */
function handleStale(context: string): void {
  console.warn(`[stock-charts] Live "${context}" request failed; showing recent cached data.`)
}

/** Snapshot root, served from `public/`; the site is hosted at the domain root, so no base prefix. */
export const CHART_SNAPSHOT_PATH = '/data/chart-api'

/** Note in the dev console when a chart falls back to the committed snapshot. */
function handleOffline(context: string): void {
  console.warn(`[stock-charts] Live "${context}" request failed; showing the bundled snapshot.`)
}

/**
 * Resilience options shared by every chart-API client. Spread into the
 * `createApiClient` / `setupIndyChartsForVue` config alongside `baseUrl`.
 */
export const CHART_API_RESILIENCE: Pick<
  ApiClientConfig,
  'staleCache' | 'onStale' | 'offlineFallback' | 'onOffline'
> = {
  staleCache: true,
  onStale: handleStale,
  offlineFallback: { baseUrl: CHART_SNAPSHOT_PATH },
  onOffline: handleOffline
}
