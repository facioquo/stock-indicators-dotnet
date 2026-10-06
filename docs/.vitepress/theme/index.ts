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
      // Every indicator referenced from a .md page must be keyed here.
      // Keys are the slug used in `<StockIndicatorChart indicator="...">`.
      indicators: {
        Adl:             { uiid: 'Adl',             title: 'Accumulation Distribution Line', chartType: 'oscillator' },
        Adx:             { uiid: 'Adx',             title: 'Average Directional Index', chartType: 'oscillator' },
        Alligator:       { uiid: 'Alligator',       title: 'Williams Alligator' },
        Alma:            { uiid: 'Alma',            title: 'Arnaud Legoux Moving Average' },
        Aroon:           { uiid: 'AROON UP/DOWN',   title: 'Aroon', chartType: 'oscillator' },
        AroonOsc:        { uiid: 'AROON OSC',       title: 'Aroon Oscillator', chartType: 'oscillator' },
        Atr:             { uiid: 'Atr',             title: 'Average True Range', chartType: 'oscillator' },
        Atrp:            { uiid: 'ATRP',            title: 'Average True Range Percent', chartType: 'oscillator' },
        AtrStop:         { uiid: 'ATR-STOP-HL',     title: 'ATR Trailing Stop (High/Low)' },
        AtrStopClose:    { uiid: 'ATR-STOP-CLOSE',  title: 'ATR Trailing Stop (Close)' },
        Awesome:         { uiid: 'AO',              title: 'Awesome Oscillator', chartType: 'oscillator' },
        Beta:            { uiid: 'Beta',            title: 'Beta', chartType: 'oscillator' },
        BollingerBands:  { uiid: 'BB',              title: 'Bollinger Bands®' },
        BollingerBandsPctB: { uiid: 'BB-PCTB',      title: 'Bollinger Bands® %B', chartType: 'oscillator' },
        Bop:             { uiid: 'Bop',             title: 'Balance of Power', chartType: 'oscillator' },
        Cci:             { uiid: 'Cci',             title: 'Commodity Channel Index', chartType: 'oscillator' },
        ChaikinOsc:      { uiid: 'CHAIKIN',         title: 'Chaikin Oscillator', chartType: 'oscillator' },
        Chandelier:      { uiid: 'CHEXIT-LONG',     title: 'Chandelier Exit (long)' },
        ChandelierShort: { uiid: 'CHEXIT-SHORT',    title: 'Chandelier Exit (short)' },
        Chop:            { uiid: 'Chop',            title: 'Choppiness Index', chartType: 'oscillator' },
        Cmf:             { uiid: 'Cmf',             title: 'Chaikin Money Flow', chartType: 'oscillator' },
        Cmo:             { uiid: 'Cmo',             title: 'Chande Momentum Oscillator', chartType: 'oscillator' },
        ConnorsRsi:      { uiid: 'CRSI',            title: 'ConnorsRSI', chartType: 'oscillator' },
        DcPeriods:       { uiid: 'DCPERIOD',        title: 'Dominant Cycle Periods', chartType: 'oscillator' },
        Dema:            { uiid: 'Dema',            title: 'Double Exponential Moving Average' },
        Doji:            { uiid: 'DOJI',            title: 'Doji' },
        Donchian:        { uiid: 'Donchian',        title: 'Donchian Channels' },
        Dpo:             { uiid: 'Dpo',             title: 'Detrended Price Oscillator', chartType: 'oscillator' },
        Dynamic:         { uiid: 'DYN',             title: 'McGinley Dynamic' },
        ElderRay:        { uiid: 'ELDER-RAY',       title: 'Elder-ray Index', chartType: 'oscillator' },
        Ema:             { uiid: 'Ema',             title: 'Exponential Moving Average' },
        Epma:            { uiid: 'Epma',            title: 'Endpoint Moving Average' },
        Fcb:             { uiid: 'Fcb',             title: 'Fractal Chaos Bands' },
        FisherTransform: { uiid: 'FISHER',          title: 'Ehlers Fisher Transform', chartType: 'oscillator' },
        ForceIndex:      { uiid: 'FORCE',           title: 'Force Index', chartType: 'oscillator' },
        Fractal:         { uiid: 'Fractal',         title: 'Williams Fractal' },
        Gator:           { uiid: 'GATOR',           title: 'Gator Oscillator', chartType: 'oscillator' },
        HeikinAshi:      { uiid: 'HEIKIN-ASHI',     title: 'Heikin-Ashi' },
        HL2:             { uiid: 'HL2',             title: 'Median Price (HL2)' },
        HLC3:            { uiid: 'HLC3',            title: 'Typical Price (HLC3)' },
        Hma:             { uiid: 'Hma',             title: 'Hull Moving Average' },
        HtTrendline:     { uiid: 'HT Trendline',    title: 'Hilbert Transform Instantaneous Trendline' },
        Hurst:           { uiid: 'Hurst',           title: 'Hurst Exponent', chartType: 'oscillator' },
        Ichimoku:        { uiid: 'Ichimoku',        title: 'Ichimoku Cloud' },
        Kama:            { uiid: 'Kama',            title: "Kaufman's Adaptive Moving Average" },
        Keltner:         { uiid: 'Keltner',         title: 'Keltner Channels' },
        Kvo:             { uiid: 'Kvo',             title: 'Klinger Volume Oscillator', chartType: 'oscillator' },
        Linear:          { uiid: 'LINEAR',          title: 'Linear regression' },
        Macd:            { uiid: 'Macd',            title: 'Moving Average Convergence / Divergence', chartType: 'oscillator' },
        MaEnvelopes:     { uiid: 'MA-ENV',          title: 'Moving Average Envelopes' },
        Mama:            { uiid: 'Mama',            title: 'MESA Adaptive Moving Average' },
        Marubozu:        { uiid: 'MARUBOZU',        title: 'Marubozu' },
        Mfi:             { uiid: 'Mfi',             title: 'Money Flow Index', chartType: 'oscillator' },
        Obv:             { uiid: 'Obv',             title: 'On-Balance Volume', chartType: 'oscillator' },
        OC2:             { uiid: 'OC2',             title: 'Open-Close Average (OC2)' },
        OHL3:            { uiid: 'OHL3',            title: 'Open-High-Low Average (OHL3)' },
        OHLC4:           { uiid: 'OHLC4',           title: 'Average Price (OHLC4)' },
        ParabolicSar:    { uiid: 'PSAR',            title: 'Parabolic SAR' },
        PivotPoints:     { uiid: 'PIVOT-POINTS',    title: 'Pivot Points' },
        Pivots:          { uiid: 'PIVOTS',          title: 'Pivots' },
        Pmo:             { uiid: 'Pmo',             title: 'Price Momentum Oscillator', chartType: 'oscillator' },
        Pvo:             { uiid: 'Pvo',             title: 'Percentage Volume Oscillator', chartType: 'oscillator' },
        Roc:             { uiid: 'Roc',             title: 'Rate of Change', chartType: 'oscillator' },
        RocWb:           { uiid: 'RocWb',           title: 'Rate of Change with Bands', chartType: 'oscillator' },
        RollingPivots:   { uiid: 'ROLLING-PIVOTS',  title: 'Rolling Pivot Points' },
        Rsi:             { uiid: 'RSI',             title: 'Relative Strength Index', chartType: 'oscillator' },
        Slope:           { uiid: 'Slope',           title: 'Slope and linear regression', chartType: 'oscillator' },
        Sma:             { uiid: 'SMA',             title: 'Simple Moving Average' },
        SmaMad:          { uiid: 'SMA-MAD',         title: 'SMA Error Analysis (MAD)', chartType: 'oscillator' },
        SmaMape:         { uiid: 'SMA-MAPE',        title: 'SMA Error Analysis (MAPE)', chartType: 'oscillator' },
        SmaMse:          { uiid: 'SMA-MSE',         title: 'SMA Error Analysis (MSE)', chartType: 'oscillator' },
        Smi:             { uiid: 'Smi',             title: 'Stochastic Momentum Index', chartType: 'oscillator' },
        Smma:            { uiid: 'Smma',            title: 'Smoothed Moving Average' },
        StarcBands:      { uiid: 'STARC',           title: 'STARC Bands' },
        Stc:             { uiid: 'Stc',             title: 'Schaff Trend Cycle', chartType: 'oscillator' },
        StdDev:          { uiid: 'STDEV',           title: 'Standard deviation', chartType: 'oscillator' },
        StdDevChannels:  { uiid: 'STDEV-CH',        title: 'Standard Deviation Channels' },
        StdDevZScore:    { uiid: 'STDEV-ZSCORE',    title: 'Standard deviation Z-score', chartType: 'oscillator' },
        Stoch:           { uiid: 'STO',             title: 'Stochastic Oscillator', chartType: 'oscillator' },
        StochRsi:        { uiid: 'StochRsi',        title: 'Stochastic RSI', chartType: 'oscillator' },
        SuperTrend:      { uiid: 'SuperTrend',      title: 'SuperTrend' },
        T3:              { uiid: 'T3',              title: 'Tillson T3 Moving Average' },
        Tema:            { uiid: 'Tema',            title: 'Triple Exponential Moving Average' },
        Tr:              { uiid: 'TR',              title: 'True Range', chartType: 'oscillator' },
        Trix:            { uiid: 'Trix',            title: 'Triple EMA Oscillator (TRIX)', chartType: 'oscillator' },
        Tsi:             { uiid: 'Tsi',             title: 'True Strength Index', chartType: 'oscillator' },
        UlcerIndex:      { uiid: 'ULCER',           title: 'Ulcer Index', chartType: 'oscillator' },
        Ultimate:        { uiid: 'Ultimate',        title: 'Ultimate Oscillator', chartType: 'oscillator' },
        VolatilityStop:  { uiid: 'VOL-STOP',        title: 'Volatility Stop' },
        Vortex:          { uiid: 'Vortex',          title: 'Vortex Indicator', chartType: 'oscillator' },
        Vwap:            { uiid: 'Vwap',            title: 'Volume Weighted Average Price' },
        Vwma:            { uiid: 'Vwma',            title: 'Volume Weighted Moving Average' },
        WilliamsR:       { uiid: 'WilliamsR',       title: 'Williams %R', chartType: 'oscillator' },
        Wma:             { uiid: 'Wma',             title: 'Weighted Moving Average' },
        ZigZag:          { uiid: 'ZIGZAG-HL',       title: 'ZigZag (High/Low)' },
        ZigZagClose:     { uiid: 'ZIGZAG-CL',       title: 'ZigZag (Close)' }
      }
    })

    app.component('Contributors', Contributors)
    app.component('CopyOrDownloadAsMarkdownButtons', CopyOrDownloadAsMarkdownButtons)
  }
} satisfies Theme
