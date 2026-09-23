import { expect, test } from '@playwright/test'

// The landing page NuGet badge sits beside the hero title where the title row
// has room for it, and in the page body otherwise (see NuGetBadge.vue and
// `.nuget-badge-hero` in custom.scss). Exactly one copy is ever visible.

const heroBadge = '.VPHero .nuget-badge-hero'
const bodyBadge = '.nuget-badge-body'

test('NuGet badge sits beside the title where the row has room', async ({ page }) => {
  for (const width of [600, 640, 768, 960, 1280, 1440]) {
    await page.setViewportSize({ width, height: 800 })
    await page.goto('/', { waitUntil: 'networkidle' })

    await expect(page.locator(heroBadge), `${width}px`).toBeVisible()
    await expect(page.locator(bodyBadge), `${width}px`).toBeHidden()

    const [name, badge, main] = await Promise.all(
      ['.VPHero .name', `${heroBadge} img`, '.VPHero .main']
        .map(async (selector) => (await page.locator(selector).boundingBox())!)
    )
    expect(badge.x, `${width}px: right of the title`).toBeGreaterThan(name.x + name.width)
    expect(badge.x + badge.width, `${width}px: within the hero`).toBeLessThanOrEqual(main.x + main.width)
    expect(badge.y, `${width}px: level with the title`).toBeGreaterThan(name.y)
    expect(badge.y + badge.height).toBeLessThan(name.y + name.height)
  }
})

test('NuGet badge stays in the page body on narrow viewports', async ({ page }) => {
  for (const width of [375, 599]) {
    await page.setViewportSize({ width, height: 800 })
    await page.goto('/', { waitUntil: 'networkidle' })

    await expect(page.locator(bodyBadge), `${width}px`).toBeVisible()
    await expect(page.locator(heroBadge), `${width}px`).toBeHidden()
  }
})
