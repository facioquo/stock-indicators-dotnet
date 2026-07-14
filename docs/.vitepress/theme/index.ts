import { h } from 'vue'
import type { Theme } from 'vitepress'
import DefaultTheme from 'vitepress/theme'
import './custom.scss'
import Contributors from '../components/Contributors.vue'
import { setupIndyChartsForVue } from '@facioquo/indy-charts/vue'
import { DARK_SURFACE, LIGHT_SURFACE } from './chart-theme'
import { CHART_API_BASE_URL, CHART_API_RESILIENCE } from './chart-api'

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
    return h(DefaultTheme.Layout, null, {
      'nav-bar-title-after': () => h('span', { class: 'nav-title-below' }, 'for .NET')
    })
  },
  enhanceApp({ app }) {
    installDevApiProxy()

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
        Adl:             { uiid: 'Adl',             title: 'Accumulation Distribution Line' },
        Adx:             { uiid: 'Adx',             title: 'Average Directional Index' },
        Alligator:       { uiid: 'Alligator',       title: 'Williams Alligator' },
        Alma:            { uiid: 'Alma',            title: 'Arnaud Legoux Moving Average' },
        Aroon:           { uiid: 'AROON UP/DOWN',   title: 'Aroon' },
        AroonOsc:        { uiid: 'AROON OSC',       title: 'Aroon Oscillator' },
        Atr:             { uiid: 'Atr',             title: 'Average True Range' },
        Atrp:            { uiid: 'ATRP',            title: 'Average True Range Percent' },
        AtrStop:         { uiid: 'ATR-STOP-HL',     title: 'ATR Trailing Stop (High/Low)' },
        AtrStopClose:    { uiid: 'ATR-STOP-CLOSE',  title: 'ATR Trailing Stop (Close)' },
        Awesome:         { uiid: 'AO',              title: 'Awesome Oscillator' },
        Beta:            { uiid: 'Beta',            title: 'Beta' },
        BollingerBands:  { uiid: 'BB',              title: 'Bollinger Bands®' },
        BollingerBandsPctB: { uiid: 'BB-PCTB',      title: 'Bollinger Bands® %B' },
        Bop:             { uiid: 'Bop',             title: 'Balance of Power' },
        Cci:             { uiid: 'Cci',             title: 'Commodity Channel Index' },
        ChaikinOsc:      { uiid: 'CHAIKIN',         title: 'Chaikin Oscillator' },
        Chandelier:      { uiid: 'CHEXIT-LONG',     title: 'Chandelier Exit (long)' },
        ChandelierShort: { uiid: 'CHEXIT-SHORT',    title: 'Chandelier Exit (short)' },
        Chop:            { uiid: 'Chop',            title: 'Choppiness Index' },
        Cmf:             { uiid: 'Cmf',             title: 'Chaikin Money Flow' },
        Cmo:             { uiid: 'Cmo',             title: 'Chande Momentum Oscillator' },
        ConnorsRsi:      { uiid: 'CRSI',            title: 'ConnorsRSI' },
        DcPeriods:       { uiid: 'DCPERIOD',        title: 'Dominant Cycle Periods' },
        Dema:            { uiid: 'Dema',            title: 'Double Exponential Moving Average' },
        Doji:            { uiid: 'DOJI',            title: 'Doji' },
        Donchian:        { uiid: 'Donchian',        title: 'Donchian Channels' },
        Dpo:             { uiid: 'Dpo',             title: 'Detrended Price Oscillator' },
        Dynamic:         { uiid: 'DYN',             title: 'McGinley Dynamic' },
        ElderRay:        { uiid: 'ELDER-RAY',       title: 'Elder-ray Index' },
        Ema:             { uiid: 'Ema',             title: 'Exponential Moving Average' },
        Epma:            { uiid: 'Epma',            title: 'Endpoint Moving Average' },
        Fcb:             { uiid: 'Fcb',             title: 'Fractal Chaos Bands' },
        FisherTransform: { uiid: 'FISHER',          title: 'Ehlers Fisher Transform' },
        ForceIndex:      { uiid: 'FORCE',           title: 'Force Index' },
        Fractal:         { uiid: 'Fractal',         title: 'Williams Fractal' },
        Gator:           { uiid: 'GATOR',           title: 'Gator Oscillator' },
        HL2:             { uiid: 'HL2',             title: 'Median Price (HL2)' },
        HLC3:            { uiid: 'HLC3',            title: 'Typical Price (HLC3)' },
        Hma:             { uiid: 'Hma',             title: 'Hull Moving Average' },
        HtTrendline:     { uiid: 'HT Trendline',    title: 'Hilbert Transform Instantaneous Trendline' },
        Hurst:           { uiid: 'Hurst',           title: 'Hurst Exponent' },
        Ichimoku:        { uiid: 'Ichimoku',        title: 'Ichimoku Cloud' },
        Kama:            { uiid: 'Kama',            title: "Kaufman's Adaptive Moving Average" },
        Keltner:         { uiid: 'Keltner',         title: 'Keltner Channels' },
        Kvo:             { uiid: 'Kvo',             title: 'Klinger Volume Oscillator' },
        Linear:          { uiid: 'LINEAR',          title: 'Linear regression' },
        Macd:            { uiid: 'Macd',            title: 'Moving Average Convergence Divergence' },
        MaEnvelopes:     { uiid: 'MA-ENV',          title: 'Moving Average Envelopes' },
        Mama:            { uiid: 'Mama',            title: 'MESA Adaptive Moving Average' },
        Marubozu:        { uiid: 'MARUBOZU',        title: 'Marubozu' },
        Mfi:             { uiid: 'Mfi',             title: 'Money Flow Index' },
        Obv:             { uiid: 'Obv',             title: 'On-Balance Volume' },
        OC2:             { uiid: 'OC2',             title: 'Open-Close Average (OC2)' },
        OHL3:            { uiid: 'OHL3',            title: 'Open-High-Low Average (OHL3)' },
        OHLC4:           { uiid: 'OHLC4',           title: 'Average Price (OHLC4)' },
        ParabolicSar:    { uiid: 'PSAR',            title: 'Parabolic SAR' },
        PivotPoints:     { uiid: 'PIVOT-POINTS',    title: 'Pivot Points' },
        Pivots:          { uiid: 'PIVOTS',          title: 'Pivots' },
        Pmo:             { uiid: 'Pmo',             title: 'Price Momentum Oscillator' },
        Pvo:             { uiid: 'Pvo',             title: 'Price Volume Oscillator' },
        Roc:             { uiid: 'Roc',             title: 'Rate of Change' },
        RocWb:           { uiid: 'RocWb',           title: 'Rate of Change with Bands' },
        RollingPivots:   { uiid: 'ROLLING-PIVOTS',  title: 'Rolling Pivot Points' },
        Rsi:             { uiid: 'RSI',             title: 'Relative Strength Index' },
        Slope:           { uiid: 'Slope',           title: 'Slope and linear regression' },
        Sma:             { uiid: 'SMA',             title: 'Simple Moving Average' },
        Smi:             { uiid: 'Smi',             title: 'Stochastic Momentum Index' },
        Smma:            { uiid: 'Smma',            title: 'Smoothed Moving Average' },
        StarcBands:      { uiid: 'STARC',           title: 'STARC Bands' },
        Stc:             { uiid: 'Stc',             title: 'Schaff Trend Cycle' },
        StdDev:          { uiid: 'STDEV',           title: 'Standard deviation' },
        StdDevChannels:  { uiid: 'STDEV-CH',        title: 'Standard Deviation Channels' },
        StdDevZScore:    { uiid: 'STDEV-ZSCORE',    title: 'Standard deviation Z-score' },
        Stoch:           { uiid: 'STO',             title: 'Stochastic Oscillator' },
        StochRsi:        { uiid: 'StochRsi',        title: 'Stochastic RSI' },
        SuperTrend:      { uiid: 'SuperTrend',      title: 'SuperTrend' },
        T3:              { uiid: 'T3',              title: 'T3 Moving Average' },
        Tema:            { uiid: 'Tema',            title: 'Triple Exponential Moving Average' },
        Tr:              { uiid: 'TR',              title: 'True Range' },
        Trix:            { uiid: 'Trix',            title: 'Triple EMA Oscillator (TRIX)' },
        Tsi:             { uiid: 'Tsi',             title: 'True Strength Index' },
        UlcerIndex:      { uiid: 'ULCER',           title: 'Ulcer Index' },
        Ultimate:        { uiid: 'Ultimate',        title: 'Ultimate Oscillator' },
        VolatilityStop:  { uiid: 'VOL-STOP',        title: 'Volatility Stop' },
        Vortex:          { uiid: 'Vortex',          title: 'Vortex Indicator' },
        Vwap:            { uiid: 'Vwap',            title: 'Volume Weighted Average Price' },
        Vwma:            { uiid: 'Vwma',            title: 'Volume Weighted Moving Average' },
        WilliamsR:       { uiid: 'WilliamsR',       title: 'Williams %R' },
        Wma:             { uiid: 'Wma',             title: 'Weighted Moving Average' },
        ZigZag:          { uiid: 'ZIGZAG-HL',       title: 'ZigZag (High/Low)' },
        ZigZagClose:     { uiid: 'ZIGZAG-CL',       title: 'ZigZag (Close)' }
      }
    })

    app.component('Contributors', Contributors)
  }
} satisfies Theme
