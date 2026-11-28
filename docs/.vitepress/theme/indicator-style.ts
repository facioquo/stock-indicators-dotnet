import { onMounted, ref, type Ref } from 'vue'
import { INDICATOR_STYLES, type IndicatorStyle } from '../style-containers.ts'
import { INDICATOR_STYLE_DEFAULT, INDICATOR_STYLE_KEY } from './indicator-style-keys.ts'

export { INDICATOR_STYLES, type IndicatorStyle }

export const INDICATOR_STYLE_LABELS: Record<IndicatorStyle, string> = {
  series: 'Series',
  buffer: 'Buffer',
  stream: 'Stream'
}

function isStyle(value: unknown): value is IndicatorStyle {
  return INDICATOR_STYLES.includes(value as IndicatorStyle)
}

// One shared state for every switcher on the page (navbar, mobile menu, and
// in-page copies). The early head script in config.mts has already stamped
// `data-indicator-style` on <html>, which is what custom.scss reads.
const current: Ref<IndicatorStyle> = ref(INDICATOR_STYLE_DEFAULT as IndicatorStyle)
let initialized = false

function apply(style: IndicatorStyle): void {
  current.value = style
  document.documentElement.dataset.indicatorStyle = style
}

function initialize(): void {
  if (initialized) return
  initialized = true

  const stamped = document.documentElement.dataset.indicatorStyle
  apply(isStyle(stamped) ? stamped : (INDICATOR_STYLE_DEFAULT as IndicatorStyle))

  // follow a change made in another open tab
  window.addEventListener('storage', (event) => {
    if (event.key === INDICATOR_STYLE_KEY && isStyle(event.newValue)) apply(event.newValue)
  })
}

export function useIndicatorStyle() {
  onMounted(initialize)

  function setStyle(style: IndicatorStyle): void {
    apply(style)
    try {
      localStorage.setItem(INDICATOR_STYLE_KEY, style)
    } catch {
      // storage blocked (private mode, site data off): the choice lasts for this visit
    }
  }

  return { style: current, setStyle }
}
