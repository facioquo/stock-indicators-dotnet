interface WebMcpTool {
  name: string
  title: string
  description: string
  inputSchema: Record<string, unknown>
  annotations: {
    readOnlyHint: boolean
    untrustedContentHint: boolean
  }
  execute: (input: Record<string, unknown>) => Promise<string>
}

interface ModelContext {
  registerTool: (
    tool: WebMcpTool,
    options?: { signal?: AbortSignal }
  ) => Promise<void>
}

interface WebMcpDocument extends Document {
  modelContext?: ModelContext
}

interface DocumentationEntry {
  title: string
  url: string
  description: string
}

const MAX_QUERY_LENGTH = 200
const MAX_RESULTS = 10

function markdownUrl(pathname: string): URL {
  const normalizedPath = pathname.replace(/\/$/, '')
  return new URL(`${normalizedPath}.md`, window.location.origin)
}

async function fetchText(url: URL): Promise<string> {
  const response = await fetch(url)
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
    if (title === term) return score + 8
    if (title.includes(term)) return score + 5
    if (url.includes(term)) return score + 3
    if (description.includes(term)) return score + 1
    return score
  }, 0)
}

async function searchDocumentation(input: Record<string, unknown>): Promise<string> {
  const query = typeof input.query === 'string' ? input.query.trim() : ''
  if (!query || query.length > MAX_QUERY_LENGTH) {
    throw new Error(`query must contain between 1 and ${MAX_QUERY_LENGTH} characters.`)
  }

  const terms = query.toLocaleLowerCase().split(/\s+/)
  const index = await fetchText(new URL('/llms.txt', window.location.origin))
  const results = parseDocumentationIndex(index)
    .map((entry) => ({ entry, score: scoreEntry(entry, terms) }))
    .filter(({ score }) => score > 0)
    .sort((left, right) => right.score - left.score || left.entry.title.localeCompare(right.entry.title))
    .slice(0, MAX_RESULTS)
    .map(({ entry }) => entry)

  return JSON.stringify({ query, results })
}

async function getCurrentPageMarkdown(): Promise<string> {
  const url = window.location.pathname === '/'
    ? new URL('/llms.txt', window.location.origin)
    : markdownUrl(window.location.pathname)
  const markdown = await fetchText(url)
  return JSON.stringify({ title: document.title, url: url.href, markdown })
}

export function installWebMcpTools(): void {
  if (typeof document === 'undefined') return

  const modelContext = (document as WebMcpDocument).modelContext
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

  const controller = new AbortController()
  window.addEventListener('pagehide', () => controller.abort(), { once: true })
  void Promise.all(tools.map((tool) => modelContext.registerTool(tool, {
    signal: controller.signal
  }))).catch((error: unknown) => {
    console.warn('Unable to register WebMCP documentation tools.', error)
  })
}
