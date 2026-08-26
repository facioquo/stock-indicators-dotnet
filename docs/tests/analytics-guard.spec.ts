import { test, expect } from '@playwright/test'
import { readFileSync, readdirSync, statSync } from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

import { assertBuildHasNoAnalytics, blockAnalytics } from './analytics-guard'

const __dirname = dirname(fileURLToPath(import.meta.url))
const DIST = join(__dirname, '../.vitepress/dist')

/**
 * A deliberately fake measurement ID. The real one must never appear in test
 * code: if a control ever regressed, a test carrying the production ID would
 * put automated traffic in the production property — the exact outcome these
 * controls exist to prevent. This ID belongs to no property.
 */
const FAKE_MEASUREMENT_ID = 'G-TESTONLY000'

test.describe('analytics controls', () => {
  test('the build under test carries no analytics markup', () => {
    // The primary control. Passes because no test workflow sets
    // ANALYTICS_ENABLED, so config.mts injects no gtag snippet.
    expect(() => assertBuildHasNoAnalytics(DIST)).not.toThrow()
  })

  test('the production measurement ID appears in no built page', () => {
    // Broader than the marker scan: nothing shipped for testing should carry
    // any GA property id at all.
    const offenders: string[] = []
    const walk = (dir: string): void => {
      for (const entry of readdirSync(dir)) {
        const full = join(dir, entry)
        if (statSync(full).isDirectory()) walk(full)
        else if (entry.endsWith('.html') && /\bG-[A-Z0-9]{6,}\b/.test(readFileSync(full, 'utf8'))) {
          offenders.push(full)
        }
      }
    }
    walk(DIST)
    expect(offenders, `Pages carrying a GA measurement id:\n${offenders.join('\n')}`).toEqual([])
  })

  test('the runtime guard aborts an analytics request and reports it', async ({ page }) => {
    const attempted = await blockAnalytics(page)

    const failures: string[] = []
    page.on('requestfailed', (request) => {
      if (request.url().includes('googletagmanager')) {
        failures.push(request.failure()?.errorText ?? 'unknown')
      }
    })

    // Serve a synthetic page that tries to load gtag, so the guard is exercised
    // against a real navigation without depending on a build that has analytics
    // switched on. The request is aborted in-browser, so nothing is sent.
    await page.route('**/guard-probe', (route) =>
      route.fulfill({
        contentType: 'text/html',
        body:
          '<!doctype html><title>probe</title>' +
          `<script src="https://www.googletagmanager.com/gtag/js?id=${FAKE_MEASUREMENT_ID}"></script>` +
          '<p>probe</p>',
      })
    )

    await page.goto('/guard-probe')
    await expect(page.locator('p')).toHaveText('probe')

    // Detected: the attempt is recorded, which is what fails a real test.
    expect(attempted, 'guard did not record the analytics attempt').toHaveLength(1)
    expect(attempted[0]).toContain('googletagmanager.com')

    // Blocked: the request failed in-browser rather than completing.
    expect(failures, 'analytics request was not aborted').not.toHaveLength(0)
  })
})
