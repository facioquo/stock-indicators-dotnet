import { expect, test } from '@playwright/test'
import { readFileSync } from 'fs'
import { dirname, join } from 'path'
import { fileURLToPath } from 'url'

import { CHART_INDICATORS } from '../.vitepress/theme/chart-indicators'
import { indicatorPages } from './chart-pages'

// Every multi-line overlay chart page carries a ChartKey, oscillator pages leave
// line names to the chart's own series legend, and the key's name rules hold for
// the snapshot rows that exercise them.

const __dirname = dirname(fileURLToPath(import.meta.url))
const SNAPSHOT = join(__dirname, '../.vitepress/public/data/chart-api/indicators.json')
const PAGES = join(__dirname, '../indicators')

interface Listing {
  uiid: string
  results: unknown[]
}

const listings = JSON.parse(readFileSync(SNAPSHOT, 'utf8')) as Listing[]

function multiLinePages(chartType: 'overlay' | 'oscillator') {
  return indicatorPages().filter(({ indicator }) => {
    const entry = CHART_INDICATORS[indicator]
    if ((entry?.chartType ?? 'overlay') !== chartType) return false
    const uiid = entry?.uiid?.toLowerCase()
    const listing = listings.find((l) => l.uiid.toLowerCase() === uiid)
    return (listing?.results.length ?? 0) > 1
  })
}

function hasKey(page: string, indicator: string): boolean {
  return new RegExp(`<ChartKey\\s+indicator="${indicator}"`).test(readFileSync(join(PAGES, `${page}.md`), 'utf8'))
}

test('every multi-line overlay chart page renders a ChartKey', () => {
  const pages = multiLinePages('overlay')
  expect(pages).not.toHaveLength(0)
  const missing = pages.filter(({ page, indicator }) => !hasKey(page, indicator))
  expect(missing.map((m) => `${m.page}:${m.indicator}`)).toEqual([])
})

test('no oscillator chart page renders a ChartKey', () => {
  const pages = multiLinePages('oscillator')
  expect(pages).not.toHaveLength(0)
  const extra = pages.filter(({ page, indicator }) => hasKey(page, indicator))
  expect(extra.map((m) => `${m.page}:${m.indicator}`)).toEqual([])
})

const NAMES: Array<{ page: string; names: string[] }> = [
  { page: 'fcb', names: ['Upper Band', 'Lower Band'] },
]

for (const { page, names } of NAMES) {
  test(`${page} key names its lines`, async ({ page: browser }) => {
    await browser.goto(`/indicators/${page}`)
    const items = browser.locator('.chart-key li')
    await expect(items).toHaveCount(names.length)
    expect((await items.allTextContents()).map((t) => t.trim())).toEqual(names)
  })
}
