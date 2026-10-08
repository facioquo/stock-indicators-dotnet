import { readdirSync, readFileSync } from 'fs'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'

const __dirname = dirname(fileURLToPath(import.meta.url))

/** Every `<StockIndicatorChart indicator="…" />` the indicator pages render. */
export function indicatorPages(): Array<{ page: string; indicator: string }> {
  const indicatorsDir = join(__dirname, '../indicators')
  const files = readdirSync(indicatorsDir)

  return files
    .filter((file) => file.endsWith('.md'))
    .flatMap((file) => {
      const body = readFileSync(join(indicatorsDir, file), 'utf8')
      // Matches only self-closing <StockIndicatorChart indicator="..." ... />
      // forms; all current pages use this form. Broaden the regex if the
      // long form <StockIndicatorChart ...></StockIndicatorChart> ever appears.
      const matches = [...body.matchAll(/<StockIndicatorChart indicator="([^"]+)"[^/]*\/>/g)]
      if (matches.length === 0) return []

      const page = file.replace(/\.md$/, '')
      return matches.map((m) => ({ page, indicator: m[1] }))
    })
}
