import { test, expect } from '@playwright/test'

/**
 * Guards against the light-theme flash on page load (#2169).
 *
 * The browser paints the navigation canvas before external stylesheets
 * arrive. The inline <style> + color-scheme meta injected in config.mts
 * must keep that first frame dark; custom.scss then restores light
 * backgrounds for visitors who explicitly chose the light theme.
 */

const DARK_CANVAS = 'rgb(27, 27, 31)' // #1b1b1f, matches theme-color meta
const LIGHT_CANVAS = 'rgb(255, 255, 255)'

const APPEARANCE_KEY = 'vitepress-theme-appearance'

test.describe('theme flash on load (#2169)', () => {
  test('canvas is dark before external stylesheets load', async ({ page }) => {
    // Hold back all external CSS so the pre-CSS window stays open long
    // enough to sample it deterministically.
    await page.route('**/*.css*', async route => {
      await new Promise(resolve => setTimeout(resolve, 5_000))
      await route.continue()
    })

    await page.goto('/', { waitUntil: 'commit' })

    // Wait for the head inline style to be parsed, then assert atomically
    // that the canvas is dark while no external stylesheet has landed yet.
    await expect
      .poll(
        () =>
          page.evaluate(() => ({
            bg: getComputedStyle(document.documentElement).backgroundColor,
            externalCssLoaded: [...document.styleSheets].some(s => s.href)
          })),
        { timeout: 3_000 }
      )
      .toEqual({ bg: DARK_CANVAS, externalCssLoaded: false })
  })

  test('default (dark) theme stays dark after full load', async ({ page }) => {
    await page.goto('/', { waitUntil: 'load' })

    await expect(page.locator('html')).toHaveClass(/dark/)
    const bg = await page.evaluate(
      () => getComputedStyle(document.documentElement).backgroundColor
    )
    expect(bg).toBe(DARK_CANVAS)
  })

  test('explicit light preference restores light canvas and controls', async ({ page }) => {
    await page.addInitScript(
      key => localStorage.setItem(key, 'light'),
      APPEARANCE_KEY
    )

    await page.goto('/', { waitUntil: 'load' })

    await expect(page.locator('html')).not.toHaveClass(/dark/)
    const state = await page.evaluate(() => ({
      bg: getComputedStyle(document.documentElement).backgroundColor,
      colorScheme: getComputedStyle(document.documentElement).colorScheme
    }))
    expect(state.bg).toBe(LIGHT_CANVAS)
    expect(state.colorScheme).toBe('light')
  })
})
