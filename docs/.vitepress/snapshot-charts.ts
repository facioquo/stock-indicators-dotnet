// Regenerates the offline chart snapshot under public/data/chart-api from the
// live API: the catalog, the quotes, and one file per indicator parameter set a
// docs page renders (the chart registry plus the landing overlays).
//
// Run with `pnpm run snapshot:charts`. The snapshot is committed and refreshed
// on a schedule, never produced by `docs:build`, so a build does not depend on
// API health. The directory is replaced only after every request succeeds.

import { mkdir, rename, rm, writeFile } from 'node:fs/promises'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

import { createApiClient, createDefaultSelection, createOfflineSnapshot } from '@facioquo/indy-charts'
import type { IndicatorListing, IndicatorSelection } from '@facioquo/indy-charts'

import { CHART_API_BASE_URL } from './theme/chart-api.ts'
import { CHART_INDICATORS } from './theme/chart-indicators.ts'
import { LANDING_OVERLAY_SPECS } from './theme/landing-overlays.ts'

const here = dirname(fileURLToPath(import.meta.url))
const target = join(here, 'public/data/chart-api')
const staging = `${target}.next`

const config = { baseUrl: CHART_API_BASE_URL }

function findListing(listings: IndicatorListing[], uiid: string): IndicatorListing {
  const listing = listings.find((item) => item.uiid.toLowerCase() === uiid.toLowerCase())
  if (!listing) throw new Error(`No catalog listing for uiid "${uiid}"`)
  return listing
}

const listings = await createApiClient(config).getListings()

const requests: Array<{ uiid: string; params?: Record<string, number> }> = [
  ...Object.values(CHART_INDICATORS).flatMap(({ uiid }) => (uiid ? [{ uiid }] : [])),
  ...LANDING_OVERLAY_SPECS.map(({ uiid, params }) => ({ uiid, params }))
]

const selections: IndicatorSelection[] = requests.map(({ uiid, params }) =>
  createDefaultSelection(findListing(listings, uiid), params)
)

const files = await createOfflineSnapshot(config, { selections })

await rm(staging, { recursive: true, force: true })
const written = new Set<string>()
for (const { path, data } of files) {
  if (written.has(path)) continue
  written.add(path)
  const file = join(staging, path)
  await mkdir(dirname(file), { recursive: true })
  await writeFile(file, `${JSON.stringify(data)}\n`)
}

await rm(target, { recursive: true, force: true })
await rename(staging, target)
console.log(`Wrote ${written.size} snapshot files to ${target}`)
