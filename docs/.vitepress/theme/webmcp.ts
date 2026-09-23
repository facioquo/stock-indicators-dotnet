import { markdownPath } from '../routes'

interface WebMcpTool {
  name: string
  title: string
  description: string
  inputSchema: Record<string, unknown>
  annotations: {
    readOnlyHint: boolean
    untrustedContentHint: boolean
  }
  execute: (
    input: Record<string, unknown>,
    options: { signal: AbortSignal }
  ) => Promise<unknown>
}

interface ModelContext {
  registerTool: (
    tool: WebMcpTool,
    options?: { signal?: AbortSignal }
  ) => Promise<void>
}

// The specification places `modelContext` on Document; Chrome's early preview
// and agent-readiness scanners expose it on Navigator.
interface WebMcpDocument extends Document {
  modelContext?: ModelContext
}

interface WebMcpNavigator extends Navigator {
  modelContext?: ModelContext
}

interface SearchIndexEntry {
  title: string
  url: string
  description: string
  headings: string[]
  text: string
}

interface IndexedPage {
  entry: SearchIndexEntry
  title: string
  description: string
  headings: string
  text: string
}

const MAX_QUERY_LENGTH = 200
const MAX_PATH_LENGTH = 200
const MAX_RESULTS = 10
// A page covering more distinct query terms always outranks one covering
// fewer; field weights only order pages with equal coverage.
const COVERAGE_WEIGHT = 100
const FIELD_WEIGHTS = { title: 8, headings: 4, description: 3, text: 1 } as const
const TITLE_PHRASE_BONUS = 1000
const STOP_WORDS = new Set([
  'a', 'an', 'and', 'are', 'can', 'do', 'for', 'how', 'i', 'in', 'is', 'my',
  'of', 'on', 'or', 'the', 'to', 'use', 'what', 'with'
])

async function fetchText(url: URL, signal: AbortSignal): Promise<string> {
  const response = await fetch(url, { signal })
  if (!response.ok) {
    throw new Error(`Documentation request failed with status ${response.status}.`)
  }
  return response.text()
}

async function loadIndex(signal: AbortSignal): Promise<IndexedPage[]> {
  const content = await fetchText(new URL('/search-index.json', window.location.origin), signal)
  return (JSON.parse(content) as SearchIndexEntry[]).map((entry) => ({
    entry,
    title: entry.title.toLocaleLowerCase(),
    description: entry.description.toLocaleLowerCase(),
    headings: entry.headings.join('\n').toLocaleLowerCase(),
    text: entry.text.toLocaleLowerCase()
  }))
}

function queryTerms(query: string): string[] {
  const words = [...new Set(query.toLocaleLowerCase().split(/[^\p{L}\p{N}.#+-]+/u).filter(Boolean))]
  const terms = words.filter((word) => !STOP_WORDS.has(word))
  return terms.length ? terms : words
}

// Matches a term at the start of a word, so `install` finds `installation`.
function wordPrefix(term: string): RegExp {
  return new RegExp(`(?:^|[^\\p{L}\\p{N}])${term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`, 'u')
}

function scorePage(page: IndexedPage, phrase: string, patterns: RegExp[]): number {
  let score = page.title.includes(phrase) ? TITLE_PHRASE_BONUS : 0
  for (const pattern of patterns) {
    const field = (Object.keys(FIELD_WEIGHTS) as Array<keyof typeof FIELD_WEIGHTS>)
      .find((name) => pattern.test(page[name]))
    if (field) score += COVERAGE_WEIGHT + FIELD_WEIGHTS[field]
  }
  return score
}

function toResult({ title, url, description }: SearchIndexEntry) {
  return { title, url: new URL(url, window.location.origin).href, description }
}

async function searchDocumentation(
  input: Record<string, unknown>,
  { signal }: { signal: AbortSignal }
): Promise<unknown> {
  const query = typeof input.query === 'string' ? input.query.trim() : ''
  if (!query || query.length > MAX_QUERY_LENGTH) {
    throw new Error(`query must contain between 1 and ${MAX_QUERY_LENGTH} characters.`)
  }

  const phrase = query.toLocaleLowerCase()
  const patterns = queryTerms(query).map(wordPrefix)
  const results = (await loadIndex(signal))
    .map((page) => ({ entry: page.entry, score: scorePage(page, phrase, patterns) }))
    .filter(({ score }) => score > 0)
    .sort((left, right) => right.score - left.score || left.entry.title.localeCompare(right.entry.title))
    .slice(0, MAX_RESULTS)
    .map(({ entry }) => toResult(entry))

  return { query, results }
}

async function getDocumentationPage(
  input: Record<string, unknown>,
  { signal }: { signal: AbortSignal }
): Promise<unknown> {
  const raw = typeof input.path === 'string' ? input.path.trim() : ''
  if (!raw || raw.length > MAX_PATH_LENGTH) {
    throw new Error(`path must contain between 1 and ${MAX_PATH_LENGTH} characters.`)
  }

  const requested = URL.parse(raw, window.location.origin)
  const target = requested?.origin === window.location.origin
    ? markdownPath(requested.pathname.replace(/\.md$/, ''))
    : undefined
  const entry = target && (await loadIndex(signal)).find((page) => page.entry.url === target)?.entry
  if (!entry) {
    throw new Error(`No documentation page matches "${raw}". Use search_documentation to find a page path.`)
  }

  const url = new URL(entry.url, window.location.origin)
  return { title: entry.title, url: url.href, markdown: await fetchText(url, signal) }
}

async function getCurrentPageMarkdown(
  _input: Record<string, unknown>,
  { signal }: { signal: AbortSignal }
): Promise<unknown> {
  const url = new URL(markdownPath(window.location.pathname) ?? '/llms.txt', window.location.origin)
  const markdown = await fetchText(url, signal)
  return { title: document.title, url: url.href, markdown }
}

export function installWebMcpTools(): void {
  if (typeof document === 'undefined') return

  const modelContext = (document as WebMcpDocument).modelContext
    ?? (navigator as WebMcpNavigator).modelContext
  if (!modelContext) return

  const tools: WebMcpTool[] = [
    {
      name: 'search_documentation',
      title: 'Search Stock Indicators documentation',
      description: 'Search the Stock Indicators for .NET documentation and return up to ten relevant pages. This operation does not change site or user data.',
      inputSchema: {
        type: 'object',
        properties: {
          query: {
            type: 'string',
            minLength: 1,
            maxLength: MAX_QUERY_LENGTH,
            description: 'Words describing the task or topic, matched against page titles, headings, descriptions, and content.'
          }
        },
        required: ['query'],
        additionalProperties: false
      },
      annotations: { readOnlyHint: true, untrustedContentHint: true },
      execute: searchDocumentation
    },
    {
      name: 'get_documentation_page',
      title: 'Get a documentation page as Markdown',
      description: 'Return the complete Markdown, with provenance frontmatter, for one page in the documentation index, such as a result from search_documentation. This operation does not change site or user data.',
      inputSchema: {
        type: 'object',
        properties: {
          path: {
            type: 'string',
            minLength: 1,
            maxLength: MAX_PATH_LENGTH,
            description: 'Page path or URL on this site, such as /indicators/rsi or /indicators/rsi.md.'
          }
        },
        required: ['path'],
        additionalProperties: false
      },
      annotations: { readOnlyHint: true, untrustedContentHint: true },
      execute: getDocumentationPage
    },
    {
      name: 'get_current_page_markdown',
      title: 'Get current page as Markdown',
      description: 'Return the complete Markdown source for the documentation page currently open in this browser tab, or the documentation index from the home page. This operation does not change site or user data.',
      inputSchema: { type: 'object', additionalProperties: false },
      annotations: { readOnlyHint: true, untrustedContentHint: true },
      execute: getCurrentPageMarkdown
    }
  ]

  void Promise.all(tools.map((tool) => modelContext.registerTool(tool))).catch((error: unknown) => {
    console.warn('Unable to register WebMCP documentation tools.', error)
  })
}
