import { test, expect } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import { readFileSync } from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

import { mockStockChartsApi, CHART_TERMINAL_SELECTOR } from './chart-api-mock'
import { assertBuildHasNoAnalytics, blockAnalytics } from './analytics-guard'

const __dirname = dirname(fileURLToPath(import.meta.url))
const DIST = join(__dirname, '../.vitepress/dist')
const LLMS_TXT = join(DIST, 'llms.txt')
const SITEMAP = join(DIST, 'sitemap.xml')

/**
 * WCAG 2.1 level A and AA — the same conformance target the previous pa11y
 * suite ran against, evaluated here by axe-core instead of HTML CodeSniffer.
 */
const WCAG_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']

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

// Checked while collecting tests, before a single page is opened: if the build
// carries analytics markup, no test in this file runs at all.
assertBuildHasNoAnalytics(DIST)

const PAGES = sitemapPaths()

test('LLM index contains unique resolvable Markdown links', () => {
  const content = readFileSync(LLMS_TXT, 'utf8')
  const links = [...content.matchAll(/\]\((\/[^)#?]+\.md)\)/g)].map((match) => match[1])
  const duplicates = links.filter((link, index) => links.indexOf(link) !== index)
  const missing = links.filter((link) => {
    const outputPath = join(DIST, link.slice(1))
    try {
      readFileSync(outputPath)
      return false
    } catch {
      return true
    }
  })

  expect(content).toMatch(/^# Stock Indicators for \.NET$/m)
  expect(duplicates).toEqual([])
  expect(missing).toEqual([])
})

test('Markdown page actions expose and retrieve source content', async ({ context, page }) => {
  await context.grantPermissions(['clipboard-read', 'clipboard-write'])
  await page.goto('/indicators/sma', { waitUntil: 'domcontentloaded' })

  await expect(page.locator('link[rel="alternate"][type="text/markdown"]')).toHaveAttribute(
    'href',
    '/indicators/sma.md'
  )

  const copyButton = page.getByRole('button', { name: 'Copy page' })
  await copyButton.click()
  await expect(page.getByRole('button', { name: 'Copied' })).toBeVisible()

  const popupPromise = page.waitForEvent('popup')
  await page.getByRole('button', { name: 'View as Markdown' }).click()
  await expect(await popupPromise).toHaveURL(/\/indicators\/sma\.md$/)

  const downloadPromise = page.waitForEvent('download')
  await page.getByRole('button', { name: 'Download Markdown' }).click()
  await expect.poll(async () => (await downloadPromise).suggestedFilename()).toBe('sma.md')
})

test('WebMCP exposes read-only documentation tools', async ({ page }) => {
  await page.addInitScript(() => {
    const tools: Array<Record<string, unknown>> = []
    Object.defineProperty(document, 'modelContext', {
      value: {
        registerTool: async (tool: Record<string, unknown>) => {
          tools.push(tool)
        }
      },
      configurable: true
    })
    Object.defineProperty(window, '__webMcpTools', { value: tools })
  })

  await page.goto('/indicators/sma', { waitUntil: 'domcontentloaded' })

  await expect.poll(() => page.evaluate(() => (
    (window as unknown as { __webMcpTools: unknown[] }).__webMcpTools.length
  ))).toBe(2)

  const result = await page.evaluate(async () => {
    const tools = (window as unknown as {
      __webMcpTools: Array<{
        name: string
        annotations: { readOnlyHint: boolean }
        execute: (input: Record<string, unknown>) => Promise<string>
      }>
    }).__webMcpTools
    const search = tools.find((tool) => tool.name === 'search_documentation')
    const currentPage = tools.find((tool) => tool.name === 'get_current_page_markdown')

    return {
      names: tools.map((tool) => tool.name),
      readOnly: tools.every((tool) => tool.annotations.readOnlyHint),
      search: JSON.parse(await search!.execute({ query: 'simple moving average' })),
      currentPage: JSON.parse(await currentPage!.execute({}))
    }
  })

  expect(result.names).toEqual(['search_documentation', 'get_current_page_markdown'])
  expect(result.readOnly).toBe(true)
  expect(result.search.results[0]).toMatchObject({
    title: 'Simple Moving Average (SMA)',
    url: 'http://localhost:4173/indicators/sma.md'
  })
  expect(result.currentPage).toMatchObject({
    url: 'http://localhost:4173/indicators/sma.md'
  })
  expect(result.currentPage.markdown).toContain('# Simple Moving Average (SMA)')

  await page.goto('/', { waitUntil: 'domcontentloaded' })
  const homePage = await page.evaluate(async () => {
    const tool = (window as unknown as {
      __webMcpTools: Array<{
        name: string
        execute: (input: Record<string, unknown>) => Promise<string>
      }>
    }).__webMcpTools.find(({ name }) => name === 'get_current_page_markdown')
    return JSON.parse(await tool!.execute({}))
  })
  expect(homePage.url).toBe('http://localhost:4173/llms.txt')
  expect(homePage.markdown).toContain('# Stock Indicators for .NET')
})

for (const path of PAGES) {
  test(`a11y - ${path}`, async ({ page }) => {
    const analyticsAttempts = await blockAnalytics(page)

    // Serve charts from the same fixtures the chart suite uses. Without this
    // the scan waits on the live API — slow, and it would only ever scan the
    // failure UI rather than a rendered chart.
    await mockStockChartsApi(page)

    await page.goto(path, { waitUntil: 'domcontentloaded' })

    // Web-first waits rather than `networkidle`, which Playwright discourages
    // for tests. The content root proves the page rendered; charts mount behind
    // ClientOnly after hydration, so any chart on the page must also reach a
    // terminal state before axe looks at it.
    await expect(page.locator('.VPContent')).toBeVisible()

    const charts = page.locator('[data-testid$="-root"]')
    for (let i = 0; i < (await charts.count()); i++) {
      await expect(
        charts.nth(i).locator(CHART_TERMINAL_SELECTOR).first()
      ).toBeVisible({ timeout: 20_000 })
    }

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
