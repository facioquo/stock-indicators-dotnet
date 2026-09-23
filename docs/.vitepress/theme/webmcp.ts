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

interface DocumentationEntry {
  title: string
  url: string
  description: string
}

const MAX_QUERY_LENGTH = 200
const MAX_RESULTS = 10
const MATCH_SCORES = {
  exactTitle: 8,
  partialTitle: 5,
  url: 3,
  description: 1
} as const

function markdownUrl(pathname: string): URL {
  const normalizedPath = pathname.replace(/\/$/, '')
  return new URL(`${normalizedPath}.md`, window.location.origin)
}

async function fetchText(url: URL, signal: AbortSignal): Promise<string> {
  const response = await fetch(url, { signal })
  if (!response.ok) {
    throw new Error(`Documentation request failed with status ${response.status}.`)
  }
  return response.text()
}

function parseDocumentationIndex(content: string): DocumentationEntry[] {
  return [...content.matchAll(/^- \[([^\]]+)\]\((\/[^)]+\.md)\)(?:: (.*))?$/gm)]
    .map((match) => ({
      title: match[1],
      url: new URL(match[2], window.location.origin).href,
      description: match[3] ?? ''
    }))
}

function scoreEntry(entry: DocumentationEntry, terms: string[]): number {
  const title = entry.title.toLocaleLowerCase()
  const description = entry.description.toLocaleLowerCase()
  const url = entry.url.toLocaleLowerCase()

  return terms.reduce((score, term) => {
    if (title === term) return score + MATCH_SCORES.exactTitle
    if (title.includes(term)) return score + MATCH_SCORES.partialTitle
    if (url.includes(term)) return score + MATCH_SCORES.url
    if (description.includes(term)) return score + MATCH_SCORES.description
    return score
  }, 0)
}

async function searchDocumentation(
  input: Record<string, unknown>,
  { signal }: { signal: AbortSignal }
): Promise<unknown> {
  const query = typeof input.query === 'string' ? input.query.trim() : ''
  if (!query || query.length > MAX_QUERY_LENGTH) {
    throw new Error(`query must contain between 1 and ${MAX_QUERY_LENGTH} characters.`)
  }

  const terms = query.toLocaleLowerCase().split(/\s+/)
  const index = await fetchText(new URL('/llms.txt', window.location.origin), signal)
  const results = parseDocumentationIndex(index)
    .map((entry) => ({ entry, score: scoreEntry(entry, terms) }))
    .filter(({ score }) => score > 0)
    .sort((left, right) => right.score - left.score || left.entry.title.localeCompare(right.entry.title))
    .slice(0, MAX_RESULTS)
    .map(({ entry }) => entry)

  return { query, results }
}

async function getCurrentPageMarkdown(
  _input: Record<string, unknown>,
  { signal }: { signal: AbortSignal }
): Promise<unknown> {
  const url = window.location.pathname === '/'
    ? new URL('/llms.txt', window.location.origin)
    : markdownUrl(window.location.pathname)
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
            description: 'Words to find in documentation titles, URLs, and descriptions.'
          }
        },
        required: ['query'],
        additionalProperties: false
      },
      annotations: { readOnlyHint: true, untrustedContentHint: true },
      execute: searchDocumentation
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
