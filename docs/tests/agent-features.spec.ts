import { expect, test } from '@playwright/test'
import { readFileSync } from 'fs'
import { dirname, join } from 'path'
import { fileURLToPath } from 'url'

const __dirname = dirname(fileURLToPath(import.meta.url))
const DIST = join(__dirname, '../.vitepress/dist')
const LLMS_TXT = join(DIST, 'llms.txt')

interface TestWebMcpTool {
  name: string
  annotations: { readOnlyHint: boolean }
  execute: (
    input: Record<string, unknown>,
    options: { signal: AbortSignal }
  ) => Promise<Record<string, unknown>>
}

interface TestSearchResult {
  query: string
  results: Array<{ title: string, url: string, description: string }>
}

interface TestPageResult {
  title: string
  url: string
  markdown: string
}

declare global {
  interface Window {
    __webMcpTools: TestWebMcpTool[]
  }
}

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
  expect(links.length).toBeGreaterThan(0)
  expect(links).toContain('/guide/getting-started.md')
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

  await page.getByRole('button', { name: 'Copy page' }).click()
  await expect(page.getByRole('button', { name: 'Copied' })).toBeVisible()
  expect(await page.evaluate(() => navigator.clipboard.readText()))
    .toContain('# Simple Moving Average (SMA)')

  await page.getByRole('button', { name: 'More page actions' }).click()
  const popupPromise = page.waitForEvent('popup')
  await page.getByRole('button', { name: 'View as Markdown' }).click()
  await expect(await popupPromise).toHaveURL(/\/indicators\/sma\.md$/)

  await page.getByRole('button', { name: 'More page actions' }).click()
  const downloadPromise = page.waitForEvent('download')
  await page.getByRole('button', { name: 'Download as Markdown' }).click()
  const download = await downloadPromise
  expect(download.suggestedFilename()).toBe('sma.md')
  expect(readFileSync((await download.path())!, 'utf8'))
    .toContain('# Simple Moving Average (SMA)')
})

test('Copy page control appears once, in the hero, on hub pages', async ({ page }) => {
  // guide/index.md, indicators.md, and utilities/index.md render via VitePress's
  // `layout: home`, which has no markdown H1 for the plugin's own injection
  // point to attach to. The theme places the control in the hero instead (see
  // `home-hero-info-after` in theme/index.ts) — regression coverage for #2236
  // and #2241 recurring on this layout.
  for (const path of ['/guide/', '/indicators/', '/utilities/']) {
    await page.goto(path, { waitUntil: 'domcontentloaded' })
    await expect(page.locator('.VPHero .markdown-copy-buttons')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Copy page' })).toHaveCount(1)
    // the hero title must not be duplicated as a second, plain heading
    await expect(page.locator('h1')).toHaveCount(1)
  }
})

test('WebMCP registers through navigator.modelContext when document lacks it', async ({ page }) => {
  await page.addInitScript(() => {
    const names: string[] = []
    Object.defineProperty(navigator, 'modelContext', {
      value: { registerTool: async ({ name }: { name: string }) => { names.push(name) } },
      configurable: true
    })
    Object.defineProperty(window, '__navigatorToolNames', { value: names })
  })

  await page.goto('/indicators/sma', { waitUntil: 'domcontentloaded' })

  await expect.poll(() => page.evaluate(() =>
    (window as unknown as { __navigatorToolNames: string[] }).__navigatorToolNames
  )).toEqual(['search_documentation', 'get_documentation_page', 'get_current_page_markdown'])
})

test('WebMCP exposes read-only documentation tools', async ({ page }) => {
  await page.addInitScript(() => {
    const tools: TestWebMcpTool[] = []
    Object.defineProperty(document, 'modelContext', {
      value: {
        registerTool: async (tool: TestWebMcpTool) => {
          tools.push(tool)
        }
      },
      configurable: true
    })
    Object.defineProperty(window, '__webMcpTools', { value: tools })
  })

  await page.goto('/indicators/sma', { waitUntil: 'domcontentloaded' })

  await expect.poll(() => page.evaluate(() => window.__webMcpTools.length)).toBe(3)

  const result = await page.evaluate(async () => {
    const tools = window.__webMcpTools
    const search = tools.find((tool) => tool.name === 'search_documentation')!
    const currentPage = tools.find((tool) => tool.name === 'get_current_page_markdown')!
    const getPage = tools.find((tool) => tool.name === 'get_documentation_page')!
    const options = { signal: new AbortController().signal }
    const originalFetch = window.fetch
    let cancellationPropagated = false
    window.fetch = (input, init) => {
      cancellationPropagated = init?.signal === options.signal
      return originalFetch(input, init)
    }

    return {
      names: tools.map((tool) => tool.name),
      readOnly: tools.every((tool) => tool.annotations.readOnlyHint),
      search: await search.execute(
        { query: 'simple moving average' }, options
      ) as unknown as TestSearchResult,
      taskSearch: await search.execute(
        { query: 'calculate SMA custom price bars installation' }, options
      ) as unknown as TestSearchResult,
      // VWAP matches "volume" in its title; Bar utilities matches every term
      coverageSearch: await search.execute(
        { query: 'price bar timestamp decimal volume' }, options
      ) as unknown as TestSearchResult,
      currentPage: await currentPage.execute({}, options) as unknown as TestPageResult,
      pages: await Promise.all(['/indicators/rsi', '/indicators/rsi.md', '/indicators/rsi/', 'http://localhost:4173/guide/getting-started']
        .map(async (path) => await getPage.execute({ path }, options) as unknown as TestPageResult)),
      invalidPaths: await Promise.allSettled(
        ['', '/', '/indicators/candlestick-patterns', '/llms-full.txt', '/indicators/../../etc/passwd', '//example.com/indicators/rsi', 'https://example.com/indicators/rsi', 'http://', 'x'.repeat(201)]
          .map((path) => getPage.execute({ path }, options))
      ).then((settled) => settled.map((outcome) =>
        outcome.status === 'rejected' ? String(outcome.reason) : 'fulfilled')),
      cancellationPropagated,
      invalidQueries: await Promise.allSettled([
        search.execute({ query: '   ' }, options),
        search.execute({ query: 'x'.repeat(201) }, options)
      ])
    }
  })

  expect(result.names).toEqual(['search_documentation', 'get_documentation_page', 'get_current_page_markdown'])
  expect(result.readOnly).toBe(true)
  expect(result.search.results[0]).toMatchObject({
    title: 'Simple Moving Average (SMA)',
    url: 'http://localhost:4173/indicators/sma.md'
  })
  expect(result.taskSearch.results[0]).toMatchObject({
    title: 'Getting started',
    url: 'http://localhost:4173/guide/getting-started.md'
  })
  expect(result.coverageSearch.results[0].title).toBe('Bar utilities')
  expect(result.pages.map(({ url }) => url)).toEqual([
    'http://localhost:4173/indicators/rsi.md',
    'http://localhost:4173/indicators/rsi.md',
    'http://localhost:4173/indicators/rsi.md',
    'http://localhost:4173/guide/getting-started.md'
  ])
  expect(result.pages[0].title).toBe('Relative Strength Index (RSI)')
  expect(result.pages[0].markdown).toMatch(/^---\nurl: \/indicators\/rsi\.md\n[\s\S]*?\ncanonical: https:\/\/dotnet\.stockindicators\.dev\/indicators\/rsi\n/)
  const tooShortOrLong = /path must contain between 1 and 200 characters/
  const notIndexed = /No documentation page matches/
  expect(result.invalidPaths).toHaveLength(9)
  result.invalidPaths.forEach((outcome, index) => {
    // '' and the 201-character path fail validation; every other input is well-formed but not indexed
    expect(outcome).toMatch(index === 0 || index === 8 ? tooShortOrLong : notIndexed)
  })
  expect(result.currentPage).toMatchObject({
    url: 'http://localhost:4173/indicators/sma.md'
  })
  expect(result.currentPage.markdown).toContain('# Simple Moving Average (SMA)')
  expect(result.cancellationPropagated).toBe(true)
  expect(result.invalidQueries.map(({ status }) => status)).toEqual(['rejected', 'rejected'])

  await page.goto('/', { waitUntil: 'domcontentloaded' })
  const homePage = await page.evaluate(async () => {
    const tool = window.__webMcpTools
      .find(({ name }) => name === 'get_current_page_markdown')!
    return tool.execute(
      {}, { signal: new AbortController().signal }
    ) as Promise<TestPageResult>
  })
  expect(homePage.url).toBe('http://localhost:4173/llms.txt')
  expect(homePage.markdown).toContain('# Stock Indicators for .NET')

  await page.route('**/search-index.json', (route) => route.fulfill({ status: 503 }))
  const fetchError = await page.evaluate(async () => {
    const tool = window.__webMcpTools.find(({ name }) => name === 'search_documentation')!
    try {
      await tool.execute({ query: 'SMA' }, { signal: new AbortController().signal })
      return ''
    } catch (error) {
      return String(error)
    }
  })
  expect(fetchError).toContain('Documentation request failed with status 503')
})
