import type { Page } from '@playwright/test'
import { readFileSync, readdirSync, statSync } from 'fs'
import { join } from 'path'

/**
 * Automated runs must never produce traffic in the production Google Analytics
 * property. Three independent controls enforce that, so no single mistake —
 * a stray environment variable, a config change, a new script — can leak.
 *
 *   1. The site only injects gtag when ANALYTICS_ENABLED is exactly "true",
 *      and no test workflow sets it.
 *   2. `assertBuildHasNoAnalytics()` refuses to let a suite start against a
 *      build that contains analytics markup at all (control 1 having failed).
 *   3. `blockAnalytics()` aborts any analytics request in the browser and
 *      records the attempt so the test fails loudly (controls 1 and 2 having
 *      failed, or something injecting a request at runtime).
 *
 * Note for anyone extending these tests: never reference the real measurement
 * ID here. Nothing under `tests/` should contain it.
 */

/** Hosts that must never receive a request during an automated run. */
export const ANALYTICS_HOSTS =
  /(googletagmanager\.com|google-analytics\.com|analytics\.google\.com|doubleclick\.net|googlesyndication\.com)/i

/**
 * Markers of an analytics-instrumented page. Case-sensitive and specific:
 * a loose /gtag/i also matches `Symbol.toStringTag` in minified vendor code.
 */
const BUILD_MARKERS: Array<[label: string, pattern: RegExp]> = [
  ['googletagmanager.com', /googletagmanager\.com/],
  ['google-analytics.com', /google-analytics\.com/],
  ['gtag( call', /\bgtag\s*\(/],
  ['dataLayer', /\bdataLayer\b/],
  ['GA measurement id', /\bG-[A-Z0-9]{6,}\b/],
]

function htmlFiles(dir: string, found: string[] = []): string[] {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry)
    if (statSync(full).isDirectory()) htmlFiles(full, found)
    else if (entry.endsWith('.html')) found.push(full)
  }
  return found
}

/**
 * Fail before a suite loads any page if the build carries analytics markup.
 *
 * This is the control that matters most: it converts "the workflow happens not
 * to set ANALYTICS_ENABLED" into an enforced precondition, so a build made with
 * analytics on is never driven by automated tests at all.
 */
export function assertBuildHasNoAnalytics(distDir: string): void {
  const offenders: string[] = []

  for (const file of htmlFiles(distDir)) {
    const html = readFileSync(file, 'utf8')
    for (const [label, pattern] of BUILD_MARKERS) {
      if (pattern.test(html)) offenders.push(`${file} → ${label}`)
    }
  }

  if (offenders.length > 0) {
    throw new Error(
      'Refusing to run: the build under test contains analytics markup, so ' +
      'driving it would send automated traffic to a live property.\n' +
      'Rebuild with ANALYTICS_ENABLED unset (`pnpm run docs:build`).\n\n' +
      offenders.slice(0, 10).join('\n') +
      (offenders.length > 10 ? `\n...and ${offenders.length - 10} more` : '')
    )
  }
}

/**
 * Abort every analytics request before it leaves the browser, and return the
 * array that collects any that were attempted. Assert the array is empty at
 * the end of a test: aborting alone would hide a regression silently.
 */
export async function blockAnalytics(page: Page): Promise<string[]> {
  const attempted: string[] = []

  page.on('request', (request) => {
    if (ANALYTICS_HOSTS.test(request.url())) attempted.push(request.url())
  })

  await page.route(ANALYTICS_HOSTS, (route) => route.abort())

  return attempted
}
