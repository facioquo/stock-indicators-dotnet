import { expect, test } from '@playwright/test'
import { readdirSync, readFileSync } from 'fs'
import { dirname, join } from 'path'
import { fileURLToPath } from 'url'

// Drift check between each indicator page and the library source it documents:
// parameter names and defaults against the `To*` series signatures, parameter
// minimums against the `.Catalog.cs` listings, result tables against the result
// records, and catalog chart panes against the docs `chartConfig`. Reads source files only, so it needs no browser.

const __dirname = dirname(fileURLToPath(import.meta.url))
const ROOT = join(__dirname, '../..')
const PAGES = join(ROOT, 'docs/indicators')

function walk(dir: string, suffix: string): string[] {
  return readdirSync(dir, { withFileTypes: true }).flatMap((e) => {
    const full = join(dir, e.name)
    if (e.isDirectory()) return walk(full, suffix)
    return full.endsWith(suffix) ? [full] : []
  })
}

function topLevelSplit(text: string): string[] {
  const parts: string[] = []
  let depth = 0
  let cur = ''
  for (const ch of text) {
    if ('<(['.includes(ch)) depth++
    if ('>)]'.includes(ch)) depth--
    if (ch === ',' && depth === 0) {
      parts.push(cur)
      cur = ''
    } else cur += ch
  }
  if (cur.trim()) parts.push(cur)
  return parts.map((p) => p.trim())
}

const stripComments = (s: string): string => s.replace(/\/\*[\s\S]*?\*\//g, '').replace(/\/\/.*$/gm, '')

// ---- Series signatures -----------------------------------------------------

// IndyMath Series methods read as arithmetic (`a.Minus(b)`) rather than taking the `To` prefix.
const INDYMATH_METHODS = ['Plus', 'Minus', 'Times', 'DividedBy', 'PlusPercent', 'MaxWith', 'MinWith', 'RollingMax', 'RollingMin', 'RollingMedian']

interface Param { type: string; name: string; def: string | null }
const methods = new Map<string, Param[][]>()
for (const file of walk(join(ROOT, 'src/Indicators'), '.Series.cs')) {
  const src = stripComments(readFileSync(file, 'utf8'))
  const re = new RegExp(String.raw`public static [^\n(=]*?\b(To\w+|${INDYMATH_METHODS.join('|')})(?:<[^>(]*>)?\s*\(\s*this\s+([^,)]+?)\s+(\w+)\s*(?:,([^)]*))?\)`, 'g')
  for (const m of src.matchAll(re)) {
    const params = topLevelSplit(m[4] ?? '').flatMap((p) => {
      const pm = /^(.+?)\s+(\w+)\s*(?:=\s*(.+))?$/s.exec(p)
      return pm ? [{ type: pm[1].trim(), name: pm[2], def: pm[3]?.trim() ?? null }] : []
    })
    params.unshift({ type: m[2].trim(), name: m[3], def: null })
    methods.set(m[1], [...(methods.get(m[1]) ?? []), params])
  }
}

// ---- Catalog minimums ------------------------------------------------------

const minimums = new Map<string, Map<string, number>>()
for (const file of walk(join(ROOT, 'src/Indicators'), '.Catalog.cs')) {
  const src = stripComments(readFileSync(file, 'utf8'))
  const blocks = src.split(/internal static readonly IndicatorListing /).slice(1)
  const common = new Map<string, Map<string, number>>()
  for (const block of blocks) {
    const ident = block.split('=')[0].trim()
    const mins = new Map<string, number>()
    for (const m of block.matchAll(/\.Add(?:Enum)?Parameter<\w+>\(\s*"(\w+)"([^;]*?)(?=\n\s*\.(?:Add|With|Build)|\n\s*;|$)/gs)) {
      const min = /minimum:\s*(-?[\d.]+)/.exec(m[2])
      if (min) mins.set(m[1], Number(min[1]))
    }
    if (block.includes('new CatalogListingBuilder()')) {
      common.set(ident, mins)
      continue
    }
    const builder = /new CatalogListingBuilder\((\w+)\)/.exec(block)
    const method = /WithMethodName\((?:typeof\(\w+\),\s*)?"(\w+)"\)/.exec(block)
    const style = /WithStyle\(Style\.(\w+)\)/.exec(block)
    if (builder && method && style?.[1] === 'Series') minimums.set(method[1], common.get(builder[1]) ?? new Map())
  }
}

if (minimums.size === 0) throw new Error('no Series listings parsed from src/Indicators/**/*.Catalog.cs')

// ---- Result records --------------------------------------------------------

const records = new Map<string, Map<string, string>>()
for (const file of walk(join(ROOT, 'src'), '.cs')) {
  if (!/Result\.cs$/.test(file)) continue
  const src = stripComments(readFileSync(file, 'utf8'))
  for (const m of src.matchAll(/public (?:sealed )?record (?:class )?(\w+Result)\b/g)) {
    const props = new Map<string, string>()
    const rest = src.slice(m.index! + m[0].length)
    const ctor = /^\s*\(([\s\S]*?)\)\s*(?::|\{|;)/.exec(rest)
    if (ctor) {
      for (const p of topLevelSplit(ctor[1])) {
        const pm = /^(?:\[[^\]]*\]\s*)*([\w<>?.,\[\] ]+?)\s+(\w+)\s*(?:=[\s\S]*)?$/.exec(p)
        if (pm) props.set(pm[2], pm[1].trim())
      }
    } else {
      const open = rest.indexOf('{')
      let depth = 0
      let end = open
      for (let i = open; i < rest.length; i++) {
        if (rest[i] === '{') depth++
        if (rest[i] === '}' && --depth === 0) {
          end = i
          break
        }
      }
      for (const pm of rest.slice(open, end).matchAll(/public ([\w<>?]+) (\w+)\s*\{\s*get;/g)) props.set(pm[2], pm[1])
    }
    records.set(m[1], props)
  }
}

// ---- Catalog chart panes ---------------------------------------------------

interface Pane { range: [number | null, number | null]; thresholds: number[] }

// Catalog `WithPane` calls by `<listing id>/<pane name>`.
const catalogPanes = new Map<string, Pane>()
for (const file of walk(join(ROOT, 'src/Indicators'), '.Catalog.cs')) {
  const src = stripComments(readFileSync(file, 'utf8'))
  const id = /\.WithId\("([^"]+)"\)/.exec(src)?.[1]
  const num = (v: string): number | null => (v.trim() === 'null' ? null : Number(v))
  const calls = src.match(/\.WithPane\(/g)?.length ?? 0
  const matches = [...src.matchAll(/\.WithPane\(\s*"(\w+)"\s*,\s*([^,]+),\s*([^,)]+)((?:,\s*-?[\d.]+)*)\s*\)/g)]
  if (calls !== matches.length) throw new Error(`${file}: ${calls - matches.length} WithPane call(s) the parity regex cannot parse`)
  for (const m of matches) {
    if (!id) throw new Error(`${file}: WithPane without a WithId`)
    const thresholds = m[4].split(',').filter((t) => t.trim()).map(Number)
    catalogPanes.set(`${id}/${m[1]}`, { range: [num(m[2]), num(m[3])], thresholds })
  }
}

// The docs `chartConfig` entry (by `uiid`) each catalog pane is a copy of. The ids
// differ between the two sources for some indicators, and Aroon's two panes are
// separate entries on the docs side.
const PANE_UIIDS = new Map<string, string>([
  ['WILLR/WilliamsR', 'WILLIAMSR'],
  ['UO/Ultimate', 'ULTIMATE'],
  ['RSI/Rsi', 'RSI'],
  ['STOCH-RSI/StochRsi', 'STOCHRSI'],
  ['STOCH/Stoch', 'STO'],
  ['ADX/Adx', 'ADX'],
  ['AROON/Aroon', 'AROON UP/DOWN'],
  ['AROON/Oscillator', 'AROON OSC'],
  ['MFI/Mfi', 'MFI'],
  ['CRSI/ConnorsRsi', 'CRSI'],
])

interface ChartConfig { minimumYAxis: number | null; maximumYAxis: number | null; thresholds: { value: number }[] | null }
const docsListings = JSON.parse(
  readFileSync(join(ROOT, 'docs/.vitepress/public/data/chart-api/indicators.json'), 'utf8')
) as { uiid: string; chartConfig: ChartConfig | null }[]

// ---- Page tables -----------------------------------------------------------

interface Row { name: string; type: string; text: string }

function tableAfter(lines: string[], from: number): Row[] {
  const rows: Row[] = []
  let started = false
  for (const line of lines.slice(from + 1)) {
    if (/^#{1,4} /.test(line)) break
    if (!line.startsWith('|')) {
      if (started) break
      continue
    }
    started = true
    const cells = line.split(/(?<!\\)\|/).slice(1, -1).map((c) => c.trim())
    const m = /^`([^`]+)`(?:\s+or\s+`[^`]+`)?\s*(?:<br>)?\s*_?`?([^`_]*)`?_?/.exec(cells[0] ?? '')
    if (m && !/^-+$/.test(m[1])) rows.push({ name: m[1], type: m[2].trim(), text: cells.slice(1).join(' ') })
  }
  return rows
}

function normalizeDefault(value: string): string {
  let v = value.trim().replace(/^"|"$/g, '')
  v = v.replace(/^[\w.]+\.(\w+)$/, (whole, last: string) => (/^\d/.test(whole) ? whole : last))
  const num = /^(-?\d+(?:\.\d+)?)[fdm]?$/i.exec(v)
  return num ? String(Number(num[1])) : v.toLowerCase()
}

// Parameters of an overload that the page documents in prose rather than in
// the table.
const IGNORED_PARAMS = new Set<string>([
  'ToFractal.leftSpan', 'ToFractal.rightSpan', // documented in the overload paragraph below the table
  'ToGator.alligator', // chained overload over Alligator results; the page documents the price-bar form
  'ToRenko.brickSize', // overload for decimal brick size; the table documents the ATR-based form
])

// Documented minimums that intentionally differ from the catalog, as `documented/catalog`.
// ToMaEnvelopes.lookbackPeriods: the true minimum depends on the selected moving-average
// type, so the catalog advertises the smallest (1) and the page documents 2.
// The test fails when an entry stops drifting or either side moves, so the change that
// aligns a minimum also edits this map.
const KNOWN_MINIMUM_DRIFT = new Map<string, string>([
  ['ToMaEnvelopes.lookbackPeriods', '2/1'],
])

function minimumDrift(method: string, row: Row, type: string) {
  const catalog = minimums.get(method)?.get(row.name)
  const gt = /(greater than (?:or equal to )?|at least )`?(-?\d+)`?(?!\d|\.\d)/.exec(row.text)
  if (!gt || catalog === undefined || type !== 'int') return null
  const inclusive = gt[1].includes('or equal to') || gt[1].startsWith('at least')
  const documented = Number(gt[2]) + (inclusive ? 0 : 1)
  return documented === catalog ? null : { key: `${method}.${row.name}`, documented, catalog }
}

// First Series method a page's code samples call.
function seriesCall(markdown: string): string | undefined {
  return /\.\s*(To[A-Z]\w*)\(/.exec(markdown)?.[1]
}

// Overview pages that list indicators and document no single method.
const OVERVIEW_PAGES = new Set<string>([
  'candlestick-patterns.md', 'moving-averages.md', 'numerical-analysis.md', 'oscillators.md', 'price-channels.md',
  'price-characteristics.md', 'price-transforms.md', 'price-trends.md', 'stop-and-reverse.md', 'volume-based.md',
])

// Pages that document several Series methods in one parameter table, by page.
const PAGE_METHODS = new Map<string, string[]>([
  ['indy-math.md', INDYMATH_METHODS],
])

// Series methods a page documents: its listed set, else the first one its samples call.
function pageMethods(file: string, markdown: string): string[] {
  const call = seriesCall(markdown)
  return PAGE_METHODS.get(file) ?? (call ? [call] : [])
}

const pages = readdirSync(PAGES).filter((f) => f.endsWith('.md')).sort()

for (const file of pages) {
  const markdown = readFileSync(join(PAGES, file), 'utf8')
  const names = pageMethods(file, markdown).filter((m) => methods.has(m))
  const method = names.join('/')
  const overloads = names.length > 0 ? names.flatMap((m) => methods.get(m) ?? []) : undefined
  if (!method || !overloads) continue

  test(`${file} matches the source`, () => {
    const lines = markdown.split('\n')
    const problems: string[] = []

    const start = lines.findIndex((l) => /^##\s+Parameters\s*$/.test(l))
    const docParams = start < 0 ? [] : tableAfter(lines, start)
    const docNames = docParams.map((p) => p.name)
    const known = new Map<string, Param>()
    for (const overload of overloads) for (const p of overload) known.set(p.name, known.get(p.name) ?? p)
    const subject = overloads[0][0].name
    const sig = [...known.values()].filter((p) => !(p.name === subject && ['bars', 'quotes', 'source'].includes(p.name)))

    for (const p of sig) if (!docNames.includes(p.name) && !IGNORED_PARAMS.has(`${method}.${p.name}`)) problems.push(`${method}: parameter ${p.name} is not documented`)
    for (const n of docNames) if (!known.has(n)) problems.push(`${method}: documented parameter ${n} is not in the signature`)

    for (const row of docParams) {
      const p = known.get(row.name)
      if (!p) continue
      if (row.type && p.type.replace('?', '') !== row.type.replace('?', '')) {
        problems.push(`${method}.${row.name}: docs type ${row.type}, signature ${p.type}`)
      }
      const stated = /Default is `?(-?\d+(?:\.\d+)?|[A-Za-z_][\w.]*)`?/.exec(row.text)
      if (stated && p.def !== null && p.def !== 'null' && normalizeDefault(stated[1]) !== normalizeDefault(p.def)) {
        problems.push(`${method}.${row.name}: docs default ${stated[1]}, signature ${p.def}`)
      }
      if (!stated && p.def !== null && p.def !== 'null' && p.def !== 'default') {
        problems.push(`${method}.${row.name}: signature default ${p.def} is not stated`)
      }
      for (const name of names) {
        const drift = minimumDrift(name, row, p.type)
        if (drift && KNOWN_MINIMUM_DRIFT.get(drift.key) !== `${drift.documented}/${drift.catalog}`) {
          problems.push(`${drift.key}: docs minimum ${drift.documented}, catalog ${drift.catalog}`)
        }
      }
    }

    lines.forEach((line, i) => {
      const heading = /^###\s+`?(\w+Result)\b`?/.exec(line)
      if (!heading) return
      const record = records.get(heading[1])
      if (!record) {
        problems.push(`${heading[1]}: no matching record found in src`)
        return
      }
      const rows = tableAfter(lines, i)
      const names = rows.map((r) => r.name)
      for (const n of record.keys()) if (!names.includes(n)) problems.push(`${heading[1]}: property ${n} is not documented`)
      for (const r of rows) {
        const declared = record.get(r.name)
        if (declared === undefined) {
          problems.push(`${heading[1]}: documented property ${r.name} is not in the record`)
        } else if (r.type && r.type !== declared) {
          problems.push(`${heading[1]}.${r.name}: docs type ${r.type}, record ${declared}`)
        }
      }
    })

    expect(problems, problems.join('\n')).toEqual([])
  })
}

test('every known minimum drift still drifts', () => {
  const observed = new Map<string, string>()
  for (const file of pages) {
    const markdown = readFileSync(join(PAGES, file), 'utf8')
    const method = seriesCall(markdown)
    const overloads = method ? methods.get(method) : undefined
    if (!method || !overloads) continue
    const lines = markdown.split('\n')
    const start = lines.findIndex((l) => /^##\s+Parameters\s*$/.test(l))
    const types = new Map(overloads.flat().map((p) => [p.name, p.type]))
    for (const row of start < 0 ? [] : tableAfter(lines, start)) {
      const drift = minimumDrift(method, row, types.get(row.name) ?? '')
      if (drift) observed.set(drift.key, `${drift.documented}/${drift.catalog}`)
    }
  }
  const stale = [...KNOWN_MINIMUM_DRIFT]
    .filter(([key, pair]) => observed.get(key) !== pair)
    .map(([key, pair]) => `${key} (pinned ${pair}, observed ${observed.get(key) ?? 'none'})`)
  expect(stale, `update KNOWN_MINIMUM_DRIFT: ${stale.join(', ')}`).toEqual([])
})

test('every indicator page names a Series method the source defines', () => {
  const uncovered = pages.filter((file) => {
    const method = seriesCall(readFileSync(join(PAGES, file), 'utf8'))
    return OVERVIEW_PAGES.has(file) ? false : !method || !methods.has(method)
  })
  expect(uncovered, `no checked Series method: ${uncovered.join(', ')}`).toEqual([])
})

test('catalog chart panes match the docs chart config', () => {
  const problems: string[] = []
  for (const [key, pane] of catalogPanes) {
    const uiid = PANE_UIIDS.get(key)
    if (!uiid) {
      problems.push(`${key}: no docs chartConfig mapped in PANE_UIIDS`)
      continue
    }
    const config = docsListings.find((l) => l.uiid === uiid)?.chartConfig
    if (!config) {
      problems.push(`${key}: docs listing ${uiid} has no chartConfig`)
      continue
    }
    const docs: Pane = {
      range: [config.minimumYAxis, config.maximumYAxis],
      thresholds: (config.thresholds ?? []).map((t) => t.value)
    }
    if (JSON.stringify(docs) !== JSON.stringify(pane)) {
      problems.push(`${key}: catalog ${JSON.stringify(pane)}, docs ${uiid} ${JSON.stringify(docs)}`)
    }
  }
  for (const key of PANE_UIIDS.keys()) if (!catalogPanes.has(key)) problems.push(`${key}: mapped in PANE_UIIDS but not a catalog WithPane`)
  expect(problems, problems.join('\n')).toEqual([])
})
