import path from 'path'
import { fileURLToPath } from 'url'
import { defineConfig, type HeadConfig } from 'vitepress'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// Analytics must be explicitly enabled for production builds by
// setting `ANALYTICS_ENABLED=true` (opt-in). This fails safely off.
const analyticsEnabled = process.env.ANALYTICS_ENABLED === 'true'

// Google Analytics (gtag) head entries — injected only when analyticsEnabled is true.
const googleAnalytics: HeadConfig[] = [
  ['script', { async: '', src: 'https://www.googletagmanager.com/gtag/js?id=G-7602GXEZ0R' }],
  ['script', {}, `window.dataLayer = window.dataLayer || [];
function gtag(){dataLayer.push(arguments);}
gtag('js', new Date());
gtag('config', 'G-7602GXEZ0R', { cookie_flags: 'SameSite=Lax; Secure' });`],
]

// Shared top-level navigation — referenced once and reused in every sidebar context
const siteNav = {
  text: 'Documentation',
  items: [
    { text: 'Getting started', link: '/guide/getting-started' },
    { text: 'Guide', link: '/guide/' },
    { text: 'Indicators', link: '/indicators' },
    { text: 'Utilities', link: '/utilities/' },
    { text: 'Migration (v2→v3)', link: '/migration/v3' },
    { text: 'Contributing', link: '/contributing' },
    { text: 'About', link: '/about' },
  ]
}

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "stock indicators",
  titleTemplate: "Stock Indicators for .NET",
  description: "Transform price quotes into trading insights.",

  // Default to dark theme (toggle still available)
  appearance: 'dark',

  sitemap: {
    hostname: 'https://dotnet.stockindicators.dev'
  },

  head: [
    ['link', { rel: 'icon', href: '/favicon.ico' }],
    ['link', { rel: 'icon', type: 'image/svg+xml', href: '/favicon.svg' }],
    ['link', { rel: 'apple-touch-icon', sizes: '180x180', href: '/assets/icons/apple-touch-icon.png' }],
    ['link', { rel: 'icon', type: 'image/png', sizes: '32x32', href: '/assets/icons/favicon-32x32.png' }],
    ['link', { rel: 'icon', type: 'image/png', sizes: '16x16', href: '/assets/icons/favicon-16x16.png' }],
    ['link', { rel: 'manifest', href: '/assets/manifest.json' }],
    ['meta', { name: 'theme-color', content: '#1b1b1f' }],

    // Paint the first frame dark (#2169). The browser's navigation canvas is
    // drawn before the external stylesheet loads; without these it defaults to
    // white, flashing light for dark-theme (default) visitors. Both apply at
    // parse time, ahead of CSS. Light theme instead takes the brief dark flash
    // by design; custom.scss restores light backgrounds via more-specific
    // selectors once the real CSS arrives.
    // The inline fallback keeps the first frame dark, and `--vp-c-bg` is used
    // once VitePress's theme CSS has loaded so the value stays aligned with the
    // site's dark theme token. tests/theme-flash.spec.ts cross-checks it
    // against the body background at runtime.
    ['meta', { name: 'color-scheme', content: 'dark' }],
    ['style', {}, 'html, body { background-color: #1b1b1f; background-color: var(--vp-c-bg); }'],
    ['meta', { property: 'og:type', content: 'website' }],
    ['meta', { property: 'og:title', content: 'Stock Indicators for .NET' }],
    ['meta', { property: 'og:description', content: 'Transform price quotes into trading insights.' }],
    ['meta', { property: 'og:image', content: '/assets/social-banner.png' }],
    ['meta', { name: 'twitter:card', content: 'summary' }],
    ['meta', { name: 'twitter:site', content: '@daveskender' }],

    // Google Analytics (gtag) — injected only when analytics are enabled.
    ...(analyticsEnabled ? googleAnalytics : []),
  ],

  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: {
      src: '/favicon.svg',
      alt: 'Stock Indicators for .NET'
    },

    nav: [
      { text: 'Home', link: '/' },
      {
        text: 'Guide',
        items: [
          { text: 'Overview', link: '/guide/' },
          { text: 'Getting started', link: '/guide/getting-started' },
          {
            text: 'Indicator styles',
            items: [
              { text: 'Overview', link: '/guide/styles' },
              { text: 'Batch (Series)', link: '/guide/styles/batch' },
              { text: 'Buffer lists', link: '/guide/styles/buffer' },
              { text: 'Stream hubs', link: '/guide/styles/stream' }]
          },
          { text: 'Chaining', link: '/guide/chaining' },
          { text: 'Custom indicators', link: '/guide/customization' },
          { text: 'Custom observers', link: '/guide/custom-observers' }
        ]
      },
      {
        text: 'Reference',
        items: [
          { text: 'Indicators', link: '/indicators' },
          { text: 'Utilities', link: '/utilities/' },
        ]
      },
      {
        text: 'More',
        items: [
          { text: 'Migration (v2→v3)', link: '/migration/v3' },
          { text: 'Legacy docs (v2)', link: 'https://v2.dotnet.stockindicators.dev' },
          { text: 'Contributing', link: '/contributing' },
          { text: 'About', link: '/about' },
        ]
      }
    ],

    sidebar: {
      '/guide/': [
        siteNav,
        {
          text: 'Guide',
          items: [
            { text: 'Overview', link: '/guide/' },
            { text: 'Getting started', link: '/guide/getting-started' },
            {
              text: 'Indicator styles',
              items: [
                { text: 'Overview', link: '/guide/styles' },
                { text: 'Batch (Series)', link: '/guide/styles/batch' },
                { text: 'Buffer lists', link: '/guide/styles/buffer' },
                { text: 'Stream hubs', link: '/guide/styles/stream' }]
            },
            { text: 'Chaining', link: '/guide/chaining' },
            { text: 'Custom indicators', link: '/guide/customization' },
            { text: 'Custom observers', link: '/guide/custom-observers' }
          ]
        }
      ],
      '/utilities': [
        siteNav,
        {
          text: 'Utilities',
          items: [
            { text: 'Overview', link: '/utilities/' },
            {
              text: 'Bar utilities',
              link: '/utilities/bars',
              collapsed: true,
              items: [
                { text: 'Use alternate price', link: '/utilities/bars#use-alternate-price' },
                { text: 'Sort price bars', link: '/utilities/bars#sort-bars' },
                { text: 'Validate bar history', link: '/utilities/bars#validate-bar-history' },
                { text: 'Resize bar history', link: '/utilities/bars#resize-bar-history' },
                { text: 'Streaming aggregator hubs', link: '/utilities/bars#streaming-aggregator-hubs' },
                { text: 'Extended candle properties', link: '/utilities/bars#extended-candle-properties' },
              ]
            },
            {
              text: 'Result utilities',
              link: '/utilities/results',
              collapsed: true,
              items: [
                { text: 'Remove warmup periods', link: '/utilities/results#remove-warmup-periods' },
                { text: 'Condense', link: '/utilities/results#condense' },
                { text: 'Find by date', link: '/utilities/results#find-by-date' },
                { text: 'Sort results', link: '/utilities/results#sort-results' },
              ]
            },
            {
              text: 'Additional utilities',
              link: '/utilities/helpers',
              collapsed: true,
              items: [
                { text: 'Numerical methods', link: '/utilities/helpers#numerical-methods' },
                { text: 'NullMath', link: '/utilities/helpers#nullmath' },
                { text: 'DeMath', link: '/utilities/helpers#demath' },
              ]
            },
            { text: 'Indicator catalog', link: '/utilities/catalog' },
          ]
        }
      ],
      '/examples': [
        siteNav,
        {
          text: 'Examples',
          items: [
            { text: 'Getting started', link: '/examples/' },
            { text: 'Custom chart (bring your own data)', link: '/examples/custom-chart' },
          ]
        }
      ],
      '/migration/v3': [siteNav],
      '/performance': [siteNav],
      '/contributing': [siteNav],
      '/about': [siteNav],
      '/indicators': [
        siteNav,
        {
          text: 'Price trends',
          link: '/indicators/price-trends',
          collapsed: true,
          items: [
            { text: 'Average Directional Index (ADX)', link: '/indicators/adx' },
            { text: 'Aroon Indicator', link: '/indicators/aroon' },
            { text: 'ATR Trailing Stop', link: '/indicators/atr-stop' },
            { text: 'Directional Movement Index (DMI)', link: '/indicators/adx' },
            { text: 'Elder-ray Index', link: '/indicators/elder-ray' },
            { text: 'Ichimoku Cloud', link: '/indicators/ichimoku' },
            { text: 'Moving Average Convergence Divergence', link: '/indicators/macd' },
            { text: 'Pivot Points', link: '/indicators/pivot-points' },
            { text: 'Rate of Change with Bands', link: '/indicators/roc-wb' },
            { text: 'Rolling Pivots', link: '/indicators/rolling-pivots' },
            { text: 'SuperTrend', link: '/indicators/super-trend' },
            { text: 'Vortex Indicator', link: '/indicators/vortex' },
            { text: 'Williams Alligator', link: '/indicators/alligator' },
          ]
        },
        {
          text: 'Price channels',
          link: '/indicators/price-channels',
          collapsed: true,
          items: [
            { text: 'Bollinger Bands®', link: '/indicators/bollinger-bands' },
            { text: 'Donchian Channels', link: '/indicators/donchian' },
            { text: 'Fractal Chaos Bands', link: '/indicators/fcb' },
            { text: 'Keltner Channels', link: '/indicators/keltner' },
            { text: 'Moving Average Envelopes', link: '/indicators/ma-envelopes' },
            { text: 'Pivot Points', link: '/indicators/pivot-points' },
            { text: 'Price Channels', link: '/indicators/donchian' },
            { text: 'Rolling Pivot Points', link: '/indicators/rolling-pivots' },
            { text: 'STARC Bands', link: '/indicators/starc-bands' },
            { text: 'Standard Deviation Channels', link: '/indicators/std-dev-channels' },
          ]
        },
        {
          text: 'Oscillators',
          link: '/indicators/oscillators',
          collapsed: true,
          items: [
            { text: 'Awesome Oscillator', link: '/indicators/awesome' },
            { text: 'Chande Momentum Oscillator', link: '/indicators/cmo' },
            { text: 'Commodity Channel Index', link: '/indicators/cci' },
            { text: 'ConnorsRSI', link: '/indicators/connors-rsi' },
            { text: 'Detrended Price Oscillator', link: '/indicators/dpo' },
            { text: 'Gator Oscillator', link: '/indicators/gator' },
            { text: 'KDJ Index', link: '/indicators/stoch' },
            { text: 'Price Momentum Oscillator', link: '/indicators/pmo' },
            { text: 'Relative Strength Index', link: '/indicators/rsi' },
            { text: 'Schaff Trend Cycle', link: '/indicators/stc' },
            { text: 'Stochastic Momentum Index', link: '/indicators/smi' },
            { text: 'Stochastic Oscillator', link: '/indicators/stoch' },
            { text: 'Stochastic RSI', link: '/indicators/stoch-rsi' },
            { text: 'Triple EMA Oscillator (TRIX)', link: '/indicators/trix' },
            { text: 'True Strength Index', link: '/indicators/tsi' },
            { text: 'Ultimate Oscillator', link: '/indicators/ultimate' },
            { text: 'Williams Percent Range (%R)', link: '/indicators/williams-r' },
          ]
        },
        {
          text: 'Stop and reverse',
          link: '/indicators/stop-and-reverse',
          collapsed: true,
          items: [
            { text: 'ATR Trailing Stop', link: '/indicators/atr-stop' },
            { text: 'Chandelier Exit', link: '/indicators/chandelier' },
            { text: 'Parabolic SAR', link: '/indicators/parabolic-sar' },
            { text: 'SuperTrend', link: '/indicators/super-trend' },
            { text: 'Volatility Stop', link: '/indicators/volatility-stop' },
          ]
        },
        {
          text: 'Candlestick patterns',
          link: '/indicators/candlestick-patterns',
          collapsed: true,
          items: [
            { text: 'Doji', link: '/indicators/doji' },
            { text: 'Marubozu', link: '/indicators/marubozu' },
            {
              text: 'Other price patterns',
              collapsed: false,
              items: [
                { text: 'Pivots', link: '/indicators/pivots' },
                { text: 'Williams Fractal', link: '/indicators/fractal' },
              ]
            }
          ],
        },
        {
          text: 'Volume-based',
          link: '/indicators/volume-based',
          collapsed: true,
          items: [
            { text: 'Accumulation Distribution Line', link: '/indicators/adl' },
            { text: 'Chaikin Money Flow', link: '/indicators/cmf' },
            { text: 'Chaikin Oscillator', link: '/indicators/chaikin-osc' },
            { text: 'Force Index', link: '/indicators/force-index' },
            { text: 'Klinger Volume Oscillator', link: '/indicators/kvo' },
            { text: 'Money Flow Index', link: '/indicators/mfi' },
            { text: 'On-Balance Volume', link: '/indicators/obv' },
            { text: 'Price Volume Oscillator', link: '/indicators/pvo' },
            { text: 'Volume Weighted Average Price', link: '/indicators/vwap' },
            { text: 'Volume Weighted Moving Average', link: '/indicators/vwma' },

          ]
        },
        {
          text: 'Moving averages',
          link: '/indicators/moving-averages',
          collapsed: true,
          items: [
            { text: 'Arnaud Legoux Moving Average', link: '/indicators/alma' },
            { text: 'Double Exponential Moving Average', link: '/indicators/dema' },
            { text: 'Endpoint Moving Average', link: '/indicators/epma' },
            { text: 'Exponential Moving Average', link: '/indicators/ema' },
            { text: 'Hilbert Transform Instantaneous Trendline', link: '/indicators/ht-trendline' },
            { text: 'Hull Moving Average', link: '/indicators/hma' },
            { text: 'Kaufman\'s Adaptive Moving Average', link: '/indicators/kama' },
            { text: 'Least Squares Moving Average (LSMA)', link: '/indicators/epma' },
            { text: 'MESA Adaptive Moving Average', link: '/indicators/mama' },
            { text: 'McGinley Dynamic', link: '/indicators/dynamic' },
            { text: 'Modified Moving Average (MMA)', link: '/indicators/smma' },
            { text: 'Running Moving Average (RMA)', link: '/indicators/smma' },
            { text: 'Simple Moving Average', link: '/indicators/sma' },
            { text: 'Smoothed Moving Average', link: '/indicators/smma' },
            { text: 'T3 Moving Average', link: '/indicators/t3' },
            { text: 'Triple Exponential Moving Average', link: '/indicators/tema' },
            { text: 'Volume Weighted Average Price', link: '/indicators/vwap' },
            { text: 'Volume Weighted Moving Average', link: '/indicators/vwma' },
            { text: 'Weighted Moving Average', link: '/indicators/wma' },
          ]
        },
        {
          text: 'Price transforms',
          link: '/indicators/price-transforms',
          collapsed: true,
          items: [
            { text: 'Basic price bar transforms', link: '/indicators/bar-part' },
            { text: 'Ehlers Fisher Transform', link: '/indicators/fisher-transform' },
            { text: 'Heikin Ashi', link: '/indicators/heikin-ashi' },
            { text: 'Renko Charts', link: '/indicators/renko' },
            { text: 'ZigZag', link: '/indicators/zig-zag' },
          ]
        },
        {
          text: 'Price characteristics',
          link: '/indicators/price-characteristics',
          collapsed: true,
          items: [
            { text: 'Average True Range', link: '/indicators/atr' },
            { text: 'Balance of Power', link: '/indicators/bop' },
            { text: 'Bull and Bear Power', link: '/indicators/elder-ray' },
            { text: 'Choppiness Index', link: '/indicators/chop' },
            { text: 'Dominant Cycle Periods', link: '/indicators/ht-trendline' },
            { text: 'Historical Volatility (HV)', link: '/indicators/std-dev' },
            { text: 'Hurst Exponent', link: '/indicators/hurst' },
            { text: 'Momentum Oscillator (MO)', link: '/indicators/roc' },
            { text: 'Normalized Average True Range', link: '/indicators/atr' },
            { text: 'Price Momentum Oscillator (PMO)', link: '/indicators/pmo' },
            { text: 'Price Relative Strength (PRS)', link: '/indicators/prs' },
            { text: 'Rate of Change (ROC)', link: '/indicators/roc' },
            { text: 'ROC with Bands', link: '/indicators/roc-wb' },
            { text: 'Rescaled Range Analysis', link: '/indicators/hurst' },
            { text: 'True Range (TR)', link: '/indicators/tr' },
            { text: 'True Strength Index (TSI)', link: '/indicators/tsi' },
            { text: 'Ulcer Index (UI)', link: '/indicators/ulcer-index' },
            {
              text: 'Numerical analysis',
              link: '/indicators/numerical-analysis',
              collapsed: false,
              items: [
                { text: 'Beta coefficient', link: '/indicators/beta' },
                { text: 'Coefficient of determination', link: '/indicators/correlation' },
                { text: 'Correlation coefficient', link: '/indicators/correlation' },
                { text: 'Linear regression (best-fit line)', link: '/indicators/slope' },
                { text: 'Mean absolute deviation', link: '/indicators/sma-analysis' },
                { text: 'Mean absolute percentage error', link: '/indicators/sma-analysis' },
                { text: 'Mean square error', link: '/indicators/sma-analysis' },
                { text: 'R-squared (R²)', link: '/indicators/correlation' },
                { text: 'Slope (gradient)', link: '/indicators/slope' },
                { text: 'Standard deviation', link: '/indicators/std-dev' },
                { text: 'Z-score (standard score)', link: '/indicators/std-dev' },
              ]
            },
          ]
        }
      ],
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/facioquo/stock-indicators-dotnet' },
    ],

    search: {
      provider: 'local'
    },

    editLink: {
      pattern: 'https://github.com/facioquo/stock-indicators-dotnet/edit/main/docs/:path',
      text: 'Edit this page on GitHub'
    },

  },

  srcDir: '.',
  outDir: '.vitepress/dist',

  cleanUrls: true,


  // Source-file rewrites (build-time path remaps).
  // Legacy URL → page redirects live in .vitepress/public/_redirects.
  rewrites: {
    // Case-sensitivity fix: serve CONTRIBUTING.md at /contributing
    'CONTRIBUTING.md': 'contributing.md',
  },

  vite: {
    publicDir: path.resolve(__dirname, 'public'),
    server: {
      fs: {
        allow: ['..']
      },
      proxy: {
        // Proxy chart API calls to avoid CORS in local development.
        // The browser calls /chart-api-proxy/* and Vite forwards server-side.
        '/chart-api-proxy': {
          target: 'https://stock-charts-api.azurewebsites.net',
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/chart-api-proxy/, '')
        }
      }
    },
    ssr: {
      // `@facioquo/indy-charts` and its bundled deps must execute in the server
      // bundle (rather than via dynamic import) so VitePress SSG can build pages
      // that mount <StockIndicatorChart> behind <ClientOnly>.
      noExternal: ['@facioquo/indy-charts', 'chartjs-plugin-annotation', 'chart.js']
    },
    build: {
      // Local search index grows with docs; raise threshold to suppress false warning
      chunkSizeWarningLimit: 600,
      rollupOptions: {
        output: {
          assetFileNames: 'assets/[name].[hash][extname]'
        }
      }
    }
  },

  // Exclude legacy Jekyll directories and build artifacts
  srcExclude: [
    '.offline/**',
    '.bundle/**',
    '.temp/**',
    '.vs/**',
    '_site/**',
    '_layouts/**',
    '_includes/**',
    '_data/**',
    'pages/**',
    '_indicators/**',
    'decisions/**',
    `assets/**`,
    'examples/Backtest/**',
    'examples/ConsoleApp/**',
    'examples/CustomIndicatorsUsage/**',
    'examples/UseQuoteApi/**',
    'examples/**/*.{sln,csproj,cs,json,png,zip,editorconfig}',
    'plans/**',
    'shared/**',
    'tests/**',
    'test-results/**',
    'playwright-report/**',
    'vendor/**',
    '.pa11yci',
    '_headers',
    'custom-chart.md',
    'Gemfile*',
    'README.md',
    'AGENTS.md',
    'PRINCIPLES.md',
  ]
})
