<script lang="ts">
import type { IndicatorListing } from '@facioquo/indy-charts'
import { CHART_SNAPSHOT_PATH } from '../theme/chart-api'

// Module scope, so one request serves every key in the session; `<script setup>`
// would rebuild this per component instance. A failed or malformed response is
// not kept, so a later mount retries.
let snapshot: Promise<IndicatorListing[]> | undefined

function loadSnapshot(): Promise<IndicatorListing[]> {
  snapshot ??= fetch(`${CHART_SNAPSHOT_PATH}/indicators.json`)
    .then((response) => (response.ok ? response.json() : []))
    .then((data: unknown) => (Array.isArray(data) ? (data as IndicatorListing[]) : []))
    .catch(() => [] as IndicatorListing[])
    .then((listings) => {
      if (listings.length === 0) snapshot = undefined
      return listings
    })
  return snapshot
}
</script>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { CHART_INDICATORS } from '../theme/chart-indicators'

// Names each line of a multi-line overlay chart. The charts library's legend
// names an overlay indicator once, not each of its lines (oscillator panes name
// their own lines via `showSeriesLegend`), so names and styles come from the
// committed catalog snapshot, which can lag the live API the chart prefers.
const props = defineProps<{ indicator: string }>()

type Swatch = 'solid' | 'dashed' | 'dotted' | 'bar'

const SWATCH: Record<string, Swatch> = { dash: 'dashed', dots: 'dotted', bar: 'bar' }

const listings = ref<IndicatorListing[]>([])
const loaded = ref(false)

onMounted(async () => {
  listings.value = await loadSnapshot()
  loaded.value = true
})

// A display name can carry an unresolved legend template ("FCB([P1]) Upper Band")
// or repeat across lines; the template prefix is dropped and a repeat falls back
// to the tooltip name, which the snapshot keeps distinct.
const lines = computed(() => {
  const uiid = CHART_INDICATORS[props.indicator]?.uiid?.toLowerCase()
  const results = listings.value.find((l) => l.uiid.toLowerCase() === uiid)?.results ?? []
  const names = results.map((r) => r.displayName.replace(/^\w+\((?:\[P\d+\],?\s*)*\)\s*/, ''))
  return results.map((r, i) => ({
    key: r.dataName,
    name: names.filter((n) => n === names[i]).length > 1 ? r.tooltipTemplate : names[i],
    // The swatch drops the alpha channel so a faint chart line still shows in the key.
    color: r.defaultColor.replace(/^(#[0-9a-f]{6})[0-9a-f]{2}$/i, '$1'),
    swatch: SWATCH[r.lineType] ?? 'solid'
  }))
})
</script>

<template>
  <ul v-if="lines.length > 1 || !loaded" class="chart-key" role="list" aria-label="Chart lines">
    <li v-for="line in lines" :key="line.key">
      <span
        class="chart-key__swatch"
        :class="`chart-key__swatch--${line.swatch}`"
        :style="{ '--swatch-color': line.color }"
        aria-hidden="true"
      />
      {{ line.name }}
    </li>
  </ul>
</template>
