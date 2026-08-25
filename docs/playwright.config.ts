import { defineConfig, devices } from '@playwright/test'

/**
 * Playwright configuration for the documentation site's browser tests.
 *
 * Two projects share one browser stack and one preview server:
 *   charts - visual chart rendering, mocked against static fixture data
 *   a11y   - axe-core WCAG 2.1 A/AA scan of every page in the sitemap
 *
 * Run via the package scripts, or drive Playwright directly:
 *   pnpm run test          # both projects
 *   pnpm run test:charts
 *   pnpm run test:a11y
 *   pnpm exec playwright test --ui
 *
 * `webServer` builds before previewing so every entry point is self-contained
 * — an IDE run button or a bare `playwright test` gets a fresh build rather
 * than previewing stale output. Playwright starts `webServer` before it loads
 * spec files, so the build is also what puts `.vitepress/dist/sitemap.xml` in
 * place for the a11y project, which enumerates its pages from it at
 * collection time.
 */
export default defineConfig({
  testDir: './tests',
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  // Chart tests keep `fullyParallel: false` so the whole file runs serially in
  // one worker; more than one worker lets the a11y file advance at the same
  // time rather than waiting behind it.
  workers: process.env.CI ? 2 : 4,
  reporter: process.env.CI ? [['github'], ['html', { open: 'never' }]] : 'list',

  use: {
    baseURL: 'http://localhost:4173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'charts',
      testMatch: /charts\.spec\.ts/,
      fullyParallel: false,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'a11y',
      testMatch: /a11y\.spec\.ts/,
      fullyParallel: true,
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'pnpm run docs:build && pnpm run docs:preview',
    url: 'http://localhost:4173',
    reuseExistingServer: !process.env.CI,
    timeout: 180_000,
  },
})
