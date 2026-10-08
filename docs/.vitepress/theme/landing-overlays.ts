// Overlays drawn on the landing page chart. The snapshot generator reads the
// same list, so every parameter set the page requests is captured for offline use.

const EMA_FAST_COLOR = '#ff4d8d'
const EMA_SLOW_COLOR = '#26c6da'
const LINEAR_COLOR = '#ff7f11'
const MARUBOZU_COLOR = '#9aa5b1'

export interface LandingOverlaySpec {
  uiid: string
  label: string
  params?: Record<string, number>
  colors?: string[]
}

export const LANDING_OVERLAY_SPECS: LandingOverlaySpec[] = [
  { uiid: 'Ema', label: 'EMA(200)', params: { lookbackPeriods: 200 }, colors: [EMA_SLOW_COLOR] },
  { uiid: 'Ema', label: 'EMA(50)', params: { lookbackPeriods: 50 }, colors: [EMA_FAST_COLOR] },
  { uiid: 'LINEAR', label: 'LINEAR(30)', params: { lookbackPeriods: 30 }, colors: [LINEAR_COLOR] },
  { uiid: 'MARUBOZU', label: 'MARUBOZU(90%)', params: { minBodyPercent: 90 }, colors: [MARUBOZU_COLOR] },
  { uiid: 'ATR-STOP-CLOSE', label: 'ATR-STOP(21,3,CLOSE)', params: { lookbackPeriods: 21, multiplier: 3 } }
]
