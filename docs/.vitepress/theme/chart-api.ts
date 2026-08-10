// Shared chart-API connection settings. Single source of truth so the VitePress
// theme (every <StockIndicatorChart> instance) and the landing overlay talk to
// the same endpoint with identical resilience — mirrors chart-theme.ts.
//
// indy-charts 0.8.0 added client-side resilience (facioquo/stock-charts#522):
//   • retry — transient network / 5xx / 429 failures are retried with backoff.
//     On by default (3 attempts, 500 ms base), so it is intentionally unset here.
//   • staleCache — each successful response is cached in sessionStorage; if a
//     later refetch exhausts its retries, the last-good value is served instead
//     of erroring. Opt-in, enabled below.
//
// This lets the docs charts self-heal from a market-open API hiccup with no user
// interaction, replacing the compensating fetch logic the docs used to carry.

import type { ApiClientConfig } from '@facioquo/indy-charts'

export const CHART_API_BASE_URL = 'https://charts-api.stockindicators.dev'

/** Note in the dev console when a chart falls back to the last-good cache. */
function handleStale(context: string): void {
  console.warn(`[stock-charts] Live "${context}" request failed; showing recent cached data.`)
}

/**
 * Resilience options shared by every chart-API client. Spread into the
 * `createApiClient` / `setupIndyChartsForVue` config alongside `baseUrl`.
 */
export const CHART_API_RESILIENCE: Pick<ApiClientConfig, 'staleCache' | 'onStale'> = {
  staleCache: true,
  onStale: handleStale
}
