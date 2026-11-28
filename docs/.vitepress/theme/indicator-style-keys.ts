// Single source for the indicator style names, storage key, and default.
// Used by config.mts (early head script), style-containers.ts (markdown),
// agent-artifacts.ts (agent output), and the theme. Keep it free of Vue imports.
export const INDICATOR_STYLES = ['series', 'buffer', 'stream'] as const
export type IndicatorStyle = (typeof INDICATOR_STYLES)[number]

export const INDICATOR_STYLE_KEY = 'indicator-style'
export const INDICATOR_STYLE_DEFAULT: IndicatorStyle = 'series'

export function isIndicatorStyle(value: unknown): value is IndicatorStyle {
  return INDICATOR_STYLES.includes(value as IndicatorStyle)
}
