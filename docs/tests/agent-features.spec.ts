import { expect, test } from '@playwright/test'
import { readFileSync } from 'fs'
import { dirname, join } from 'path'
import { fileURLToPath } from 'url'

const __dirname = dirname(fileURLToPath(import.meta.url))
const DIST = join(__dirname, '../.vitepress/dist')
const LLMS_TXT = join(DIST, 'llms.txt')

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

  await page.getByRole('button', { name: 'Copy Markdown' }).click()
  await expect(page.getByRole('button', { name: 'Copied' })).toBeVisible()
  expect(await page.evaluate(() => navigator.clipboard.readText()))
    .toContain('# Simple Moving Average (SMA)')

  await page.locator('summary[aria-label="More Markdown actions"]').click()
  const popupPromise = page.waitForEvent('popup')
  await page.getByRole('button', { name: 'View as Markdown' }).click()
  await expect(await popupPromise).toHaveURL(/\/indicators\/sma\.md$/)

  await page.locator('summary[aria-label="More Markdown actions"]').click()
  const downloadPromise = page.waitForEvent('download')
  await page.getByRole('button', { name: 'Download Markdown' }).click()
  const download = await downloadPromise
  expect(download.suggestedFilename()).toBe('sma.md')
  expect(readFileSync((await download.path())!, 'utf8'))
    .toContain('# Simple Moving Average (SMA)')
})
