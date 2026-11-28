import { readdirSync, readFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'
import { test, expect, type Page } from '@playwright/test'

/**
 * Indicator pages show one indicator style at a time (Series, Buffer, or
 * Stream). The reader's choice persists in localStorage and is applied by an
 * early head script before first paint; see theme/indicator-style.ts.
 */

const KEY = 'indicator-style'
const PAGE = '/indicators/ema'
const INDICATORS = join(dirname(fileURLToPath(import.meta.url)), '../indicators')

async function visibleStyles(page: Page): Promise<string[]> {
  return page.evaluate(() => {
    const styles = new Set<string>()
    for (const el of document.querySelectorAll<HTMLElement>('.vp-doc .style-block')) {
      if (el.offsetParent !== null) styles.add(el.dataset.style ?? '')
    }
    return [...styles].sort()
  })
}

const inPageSwitch = (page: Page) => page.locator('.vp-doc .style-switch--page')

test.describe('indicator style selection', () => {
  test('shows only Series content by default', async ({ page }) => {
    await page.goto(PAGE)
    await expect(page.locator('html')).toHaveAttribute('data-indicator-style', 'series')
    expect(await visibleStyles(page)).toEqual(['series'])
    await expect(inPageSwitch(page).getByRole('radio', { name: 'Series' })).toBeChecked()
  })

  test('remembers the chosen style across reloads and navigation', async ({ page }) => {
    await page.goto(PAGE)
    await inPageSwitch(page).getByText('Buffer', { exact: true }).click()
    expect(await visibleStyles(page)).toEqual(['buffer'])
    expect(await page.evaluate((key) => localStorage.getItem(key), KEY)).toBe('buffer')

    await page.reload()
    expect(await visibleStyles(page)).toEqual(['buffer'])
    await expect(inPageSwitch(page).getByRole('radio', { name: 'Buffer' })).toBeChecked()

    // client-side navigation away and back keeps the choice
    await inPageSwitch(page).getByRole('link', { name: 'Indicator style' }).click()
    await expect(page).toHaveURL(/\/guide\/styles\/?$/)
    await page.goBack()
    await expect(page).toHaveURL(/\/indicators\/ema$/)
    expect(await visibleStyles(page)).toEqual(['buffer'])
    await expect(inPageSwitch(page).getByRole('radio', { name: 'Buffer' })).toBeChecked()
  })

  test('applies a saved style before the app hydrates', async ({ page }) => {
    await page.addInitScript((key) => localStorage.setItem(key, 'stream'), KEY)
    // block the app bundle: only the inline head script and CSS can act
    await page.route('**/*.js', (route) => route.abort())
    await page.goto(PAGE)

    await expect(page.locator('html')).toHaveAttribute('data-indicator-style', 'stream')
    expect(await visibleStyles(page)).toEqual(['stream'])
  })

  test('navbar switch changes the style on desktop', async ({ page }) => {
    await page.goto(PAGE)
    await page.locator('.VPNavBar .style-switch--nav').getByText('Stream', { exact: true }).click()
    expect(await visibleStyles(page)).toEqual(['stream'])
    await expect(inPageSwitch(page).getByRole('radio', { name: 'Stream' })).toBeChecked()
  })

  test('mobile menu switch changes the style', async ({ page }) => {
    await page.setViewportSize({ width: 400, height: 800 })
    await page.goto(PAGE)
    await expect(page.locator('.VPNavBar .style-switch--nav')).toBeHidden()

    await page.locator('.VPNavBarHamburger').click()
    await page.locator('.VPNavScreen .style-switch--screen').getByText('Stream', { exact: true }).click()
    expect(await visibleStyles(page)).toEqual(['stream'])
  })
})

test('indicator pages keep style containers balanced and headings outside them', () => {
  // Headings inside a style container would make the outline, anchors, and
  // search sections differ by style; an unbalanced container hides the rest
  // of the page from readers of the other styles.
  const offenders: string[] = []
  for (const file of readdirSync(INDICATORS).filter((f) => f.endsWith('.md'))) {
    let depth = 0
    let fence = false
    for (const line of readFileSync(join(INDICATORS, file), 'utf8').split('\n')) {
      if (/^(`{3,}|~{3,})/.test(line)) fence = !fence
      if (fence) continue
      if (/^:::\s*(series|buffer|stream)\s*$/.test(line)) {
        if (depth) offenders.push(`${file}: nested style container: ${line}`)
        depth = 1
      } else if (depth && /^:::\s*\S/.test(line)) depth++
      else if (depth && /^:::\s*$/.test(line)) depth--
      else if (depth && /^#{1,6}\s/.test(line)) offenders.push(`${file}: heading in style container: ${line}`)
    }
    if (depth) offenders.push(`${file}: unclosed style container`)
  }
  expect(offenders).toEqual([])
})
