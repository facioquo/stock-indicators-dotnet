import { h } from 'vue'
import type { Theme } from 'vitepress'
import { useData } from 'vitepress'
import DefaultTheme from 'vitepress/theme'
import './custom.scss'
import Contributors from '../components/Contributors.vue'
import NuGetBadge from '../components/NuGetBadge.vue'
import CopyOrDownloadAsMarkdownButtons from 'vitepress-plugin-llms/vitepress-components/CopyOrDownloadAsMarkdownButtons.vue'
import { setupIndyChartsForVue } from '@facioquo/indy-charts/vue'
import { DARK_SURFACE, LIGHT_SURFACE } from './chart-theme'
import { CHART_API_BASE_URL, CHART_API_RESILIENCE } from './chart-api'
import { CHART_INDICATORS } from './chart-indicators'
import { installWebMcpTools } from './webmcp'

const CHART_API_HOST = new URL(CHART_API_BASE_URL).hostname
const DEV_PROXY_PATH = '/chart-api-proxy'

// In local development only, rewrite chart-API requests to Vite's
// `/chart-api-proxy` (see config.mts) so they are forwarded server-side,
// avoiding CORS — `localhost` is not in the API's allow-list. In production the
// docs origin IS allow-listed, so requests go straight to the API.
//
// This is purely a dev CORS shim. Transient-failure handling now lives inside
// indy-charts (auto-retry with backoff + last-good stale cache, configured in
// chart-api.ts), so the docs no longer compensate for API blips here.
function installDevApiProxy(): void {
  if (!import.meta.env.DEV) return
  if (typeof window === 'undefined' || typeof window.fetch !== 'function') return

  const originalFetch = window.fetch.bind(window)

  window.fetch = (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
    const requestUrl = new URL(
      input instanceof Request ? input.url : String(input),
      window.location.href
    )
    if (requestUrl.hostname !== CHART_API_HOST) {
      return originalFetch(input, init)
    }
    return originalFetch(`${DEV_PROXY_PATH}${requestUrl.pathname}${requestUrl.search}`, init)
  }
}

export default {
  extends: DefaultTheme,
  Layout: () => {
    const { frontmatter } = useData()

    // A `layout: home` page (other than the true homepage) has no markdown H1
    // for the plugin's own markdown-it hook to inject after, so its Copy-page
    // control is placed here instead, beside the hero title.
    const isHubPage = frontmatter.value.layout === 'home' && !frontmatter.value.isHome

    // The homepage also renders its NuGet badge in the hero; custom.scss shows
    // it beside the title only where the row has room (see `.nuget-badge-hero`).
    const heroInfoAfter = isHubPage
      ? () => h(CopyOrDownloadAsMarkdownButtons)
      : frontmatter.value.isHome
        ? () => h(NuGetBadge, { class: 'nuget-badge-hero' })
        : undefined

    return h(DefaultTheme.Layout, null, {
      'nav-bar-title-after': () => h('span', { class: 'nav-title-below' }, 'for .NET'),
      ...(heroInfoAfter ? { 'home-hero-info-after': heroInfoAfter } : {})
    })
  },
  enhanceApp({ app }) {
    installDevApiProxy()
    installWebMcpTools()

    setupIndyChartsForVue(app, {
      api: { baseUrl: CHART_API_BASE_URL, ...CHART_API_RESILIENCE },
      defaults: {
        barCount: 250,
        quoteCount: 250,
        showTooltips: false,
        showRightAxisLabels: false
      },
      theme: {
        observeVitePressDarkMode: true,
        darkBackground: DARK_SURFACE,
        lightBackground: LIGHT_SURFACE
      },
      indicators: CHART_INDICATORS
    })

    app.component('Contributors', Contributors)
    app.component('CopyOrDownloadAsMarkdownButtons', CopyOrDownloadAsMarkdownButtons)
  }
} satisfies Theme
