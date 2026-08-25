import { test, expect, type Page } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import { readFileSync } from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

const __dirname = dirname(fileURLToPath(import.meta.url))
const SITEMAP = join(__dirname, '../.vitepress/dist/sitemap.xml')

/**
 * WCAG 2.1 level A and AA — the same conformance target the previous pa11y
 * suite ran against, evaluated here by axe-core instead of HTML CodeSniffer.
 */
const WCAG_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']

/**
 * Analytics endpoints. The site only injects gtag when ANALYTICS_ENABLED is
 * exactly "true" (see config.mts), which no test or CI path sets — but tests
 * must never report to Google Analytics no matter where they run, so every
 * request to these hosts is aborted before it leaves the machine AND recorded
 * so the test fails loudly rather than silently passing on a config change.
 */
const ANALYTICS_HOSTS =
  /(googletagmanager\.com|google-analytics\.com|analytics\.google\.com|doubleclick\.net|googlesyndication\.com)/i

/**
 * Every page the site publishes, taken from the built sitemap so coverage
 * tracks the site automatically as pages are added or removed.
 */
function sitemapPaths(): string[] {
  let xml: string
  try {
    xml = readFileSync(SITEMAP, 'utf8')
  } catch {
    throw new Error(
      `Sitemap not found at ${SITEMAP}. Build the site first — ` +
      'use `pnpm run test:a11y`, which builds before running.'
    )
  }

  const paths = [...xml.matchAll(/<loc>([^<]+)<\/loc>/g)]
    .map((m) => new URL(m[1]).pathname)
    .sort()

  if (paths.length === 0) {
    throw new Error(`No <loc> entries parsed from ${SITEMAP}`)
  }

  return paths
}

/**
 * Block analytics before any navigation and collect anything that tried.
 */
async function blockAnalytics(page: Page): Promise<string[]> {
  const attempted: string[] = []

  page.on('request', (request) => {
    if (ANALYTICS_HOSTS.test(request.url())) attempted.push(request.url())
  })

  await page.route(ANALYTICS_HOSTS, (route) => route.abort())

  return attempted
}

/** Render violations as an actionable report rather than a diff of objects. */
function formatViolations(
  violations: Awaited<ReturnType<AxeBuilder['analyze']>>['violations']
): string {
  return violations
    .map((v) => {
      const targets = v.nodes
        .map((n) => `      - ${n.target.join(' ')}`)
        .join('\n')
      return `  [${v.impact ?? 'unknown'}] ${v.id}: ${v.help}\n    ${v.helpUrl}\n${targets}`
    })
    .join('\n\n')
}

const PAGES = sitemapPaths()

for (const path of PAGES) {
  test(`a11y - ${path}`, async ({ page }) => {
    const analyticsAttempts = await blockAnalytics(page)

    await page.goto(path, { waitUntil: 'domcontentloaded' })

    // Charts mount behind ClientOnly and paint after hydration; waiting for the
    // network to settle keeps the scan from racing late-inserted content.
    await page.waitForLoadState('networkidle')

    const { violations } = await new AxeBuilder({ page })
      .withTags(WCAG_TAGS)
      .analyze()

    expect(
      violations,
      violations.length
        ? `${violations.length} accessibility violation(s) on ${path}:\n\n${formatViolations(violations)}`
        : ''
    ).toEqual([])

    expect(
      analyticsAttempts,
      `Page attempted to contact analytics: ${analyticsAttempts.join(', ')}`
    ).toEqual([])
  })
}
