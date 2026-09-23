import { expect, test } from '@playwright/test'
import { createHash } from 'crypto'
import { existsSync, readdirSync, readFileSync } from 'fs'
import { dirname, join, relative, sep } from 'path'
import { fileURLToPath } from 'url'
import {
  DOCS_VERSION,
  normalizeDocument,
  PACKAGE_ID,
  renderContainers,
  rewriteInternalLinks,
  stripVitePressSyntax,
} from '../.vitepress/agent-artifacts'
import { markdownPath, onRequest, prefersMarkdown } from '../functions/_middleware'

// Build-output checks for the agent-facing artifacts: the llms.txt bundle,
// per-page Markdown, discovery files, and redirect/canonical consistency.

const __dirname = dirname(fileURLToPath(import.meta.url))
const DIST = join(__dirname, '../.vitepress/dist')
const SITE_URL = 'https://dotnet.stockindicators.dev'

const read = (file: string): string => readFileSync(join(DIST, file), 'utf8')

function listFiles(dir: string): string[] {
  return readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = join(dir, entry.name)
    if (entry.isDirectory()) return entry.name === 'assets' ? [] : listFiles(full)
    return [relative(DIST, full).split(sep).join('/')]
  })
}

/** Removes fenced code so checks only see prose. */
const prose = (content: string): string => content.replace(/^(`{3,}|~{3,})[\s\S]*?^\1\s*$/gm, '')

/** True when a root-relative route resolves to a built page without a further hop. */
function isPage(route: string): boolean {
  const file = route.replace(/^\//, '')
  if (!file) return true
  return route.endsWith('/')
    ? existsSync(join(DIST, file, 'index.html'))
    : existsSync(join(DIST, `${file}.html`))
}

const pageMarkdown = (): string[] => listFiles(DIST)
  .filter((file) => file.endsWith('.md') && !file.startsWith('.well-known/'))

test.describe('Markdown normalization', () => {
  test('renders containers as alerts and leaves fenced code alone', () => {
    const input = [
      '::: warning 🚩 Heads up',
      'Body line',
      '',
      '```md',
      '::: tip',
      '```',
      ':::',
      '',
      '```md',
      '::: tip inside code',
      '```',
    ].join('\n')

    expect(renderContainers(input)).toBe([
      '> [!WARNING]',
      '> **🚩 Heads up**',
      '>',
      '> Body line',
      '>',
      '> ```md',
      '> ::: tip',
      '> ```',
      '',
      '```md',
      '::: tip inside code',
      '```',
    ].join('\n'))
  })

  test('renders nested containers as nested quotes', () => {
    const input = ['::: details Outer', 'a', '::: warning', 'b', ':::', 'c', ':::', 'after'].join('\n')
    expect(renderContainers(input)).toBe([
      '> **Outer**', '>', '> a', '> > [!WARNING]', '> > b', '> c', 'after',
    ].join('\n'))
  })

  test('points internal links at Markdown only where it exists', () => {
    const routes = new Set(['/indicators/sma.md', '/utilities.md'])
    const input = [
      '[a](/indicators/sma) [b](/indicators/sma#params) [c](/utilities/)',
      '[d](/indicators/none) [e](https://example.com/x) [f](#local) [g](/llms.txt)',
      '```',
      '[h](/indicators/sma)',
      '```',
    ].join('\n')

    expect(rewriteInternalLinks(input, routes)).toBe([
      '[a](/indicators/sma.md) [b](/indicators/sma.md#params) [c](/utilities.md)',
      '[d](/indicators/none) [e](https://example.com/x) [f](#local) [g](/llms.txt)',
      '```',
      '[h](/indicators/sma)',
      '```',
    ].join('\n'))
  })

  test('strips heading anchors and code annotations', () => {
    const input = [
      '## Aroon Indicator {#aroon-indicator}',
      '```csharp',
      'var a = quotes.GetSma(20); // [!code --]',
      'var b = bars.ToSma(20);    // [!code ++]',
      'var c = 1; // [!code highlight]',
      '## Not a heading {#kept}',
      '```',
    ].join('\n')

    expect(stripVitePressSyntax(input)).toBe([
      '## Aroon Indicator',
      '```csharp',
      'var a = quotes.GetSma(20); // removed',
      'var b = bars.ToSma(20);    // added',
      'var c = 1;',
      '## Not a heading {#kept}',
      '```',
    ].join('\n'))
  })

  test('resolves frontmatter templates and appends provenance', () => {
    const input = '---\nurl: /x.md\ndescription: >-\n  Folded\n  text\n---\n\n{{ $frontmatter.description }}\n'
    expect(normalizeDocument(input, new Set(), { package: 'P' }))
      .toBe('---\nurl: /x.md\ndescription: >-\n  Folded\n  text\npackage: P\n---\n\nFolded text\n')
  })
})

test.describe('Markdown negotiation middleware', () => {
  test('honors explicit Markdown preference only', () => {
    expect(prefersMarkdown('text/markdown')).toBe(true)
    expect(prefersMarkdown('text/markdown, text/html;q=0.9')).toBe(true)
    expect(prefersMarkdown('text/html, text/markdown;q=0.5')).toBe(false)
    expect(prefersMarkdown('text/html,application/xhtml+xml,*/*;q=0.8')).toBe(false)
    expect(prefersMarkdown('*/*')).toBe(false)
    expect(prefersMarkdown(null)).toBe(false)
  })

  test('maps page routes to their Markdown representation', () => {
    expect(markdownPath('/')).toBe('/llms.txt')
    expect(markdownPath('/indicators/sma')).toBe('/indicators/sma.md')
    expect(markdownPath('/guide/')).toBe('/guide.md')
    expect(markdownPath('/indicators/sma.md')).toBeUndefined()
    expect(markdownPath('/assets/app.js')).toBeUndefined()

    // every negotiated route must resolve in the build output
    for (const route of ['/', '/guide/', '/utilities/', '/indicators', '/indicators/sma']) {
      expect(existsSync(join(DIST, markdownPath(route)!))).toBe(true)
    }
  })

  test('serves Markdown when preferred and HTML otherwise', async () => {
    const assets = async (request: Request): Promise<Response> => {
      const file = join(DIST, new URL(request.url).pathname)
      return existsSync(file)
        ? new Response(readFileSync(file, 'utf8'), { headers: { 'Content-Type': 'text/plain' } })
        : new Response('missing', { status: 404 })
    }
    const context = (url: string, accept: string) => ({
      request: new Request(url, { headers: { Accept: accept } }),
      next: async () => new Response('<html></html>', { headers: { 'Content-Type': 'text/html' } }),
      env: { ASSETS: { fetch: assets } },
    })

    const markdown = await onRequest(context(`${SITE_URL}/indicators/sma`, 'text/markdown'))
    expect(markdown.headers.get('Content-Type')).toBe('text/markdown; charset=utf-8')
    expect(markdown.headers.get('Vary')).toBe('Accept')
    expect(await markdown.text()).toContain('# Simple Moving Average (SMA)')

    const html = await onRequest(context(`${SITE_URL}/indicators/sma`, 'text/html'))
    expect(html.headers.get('Content-Type')).toBe('text/html')
    expect(html.headers.get('Vary')).toBe('Accept')

    const missing = await onRequest(context(`${SITE_URL}/indicators/not-a-page`, 'text/markdown'))
    expect(missing.headers.get('Content-Type')).toBe('text/html')

    const failing = await onRequest({
      ...context(`${SITE_URL}/indicators/sma`, 'text/markdown'),
      env: { ASSETS: { fetch: () => Promise.reject(new Error('binding fault')) } },
    })
    expect(failing.headers.get('Content-Type')).toBe('text/html')
  })
})

test.describe('Build output', () => {
  test('llms.txt names the current package and documentation version', () => {
    const identity = read('llms.txt').match(/^## Library identity\n\n([\s\S]*?)\n\n## /m)?.[1] ?? ''

    expect(identity).toContain('NuGet package: `FacioQuo.Stock.Indicators`')
    expect(identity).toContain('Namespace: `FacioQuo.Stock.Indicators`')
    expect(identity).toContain('Documentation version: v3')
    expect(identity).toContain('`Skender.Stock.Indicators` (v2, superseded')
    expect(identity).toMatch(/Generated: \d{4}-\d{2}-\d{2}T/)
    expect(read('llms-full.txt')).toContain(identity)
  })

  test('page Markdown carries provenance and stays within Markdown', () => {
    const pages = pageMarkdown()
    const routes = new Set(pages.map((file) => `/${file}`))
    expect(pages.length).toBeGreaterThan(50)

    for (const file of pages) {
      const content = read(file)
      expect(content, file).toMatch(/^---\n[\s\S]*?\npackage: FacioQuo\.Stock\.Indicators\n/)
      expect(content, file).toMatch(/\ndocs_version: v3\n/)
      // the Markdown twin and its rendered page name the same canonical URL
      const canonical = content.match(/\ncanonical: (\S+)\n/)?.[1] ?? ''
      const route = canonical.slice(SITE_URL.length)
      expect(isPage(route), `${file} → ${canonical}`).toBe(true)
      const html = route.endsWith('/') ? `${route.slice(1)}index.html` : `${route.slice(1)}.html`
      expect(read(html), file).toContain(`<link rel="canonical" href="${canonical}"`)

      // no internal link should point at an HTML page that has a Markdown twin
      const htmlLinks = [...prose(content).matchAll(/\]\((\/[^)\s#]*)/g)]
        .map((match) => match[1].replace(/\/$/, ''))
        .filter((route) => route && routes.has(`${route}.md`))
      expect(htmlLinks, file).toEqual([])
    }
  })

  test('Markdown outputs contain no VitePress-only syntax', () => {
    for (const file of [...pageMarkdown(), 'llms.txt', 'llms-full.txt']) {
      const content = prose(read(file))
      expect(content, file).not.toMatch(/^:::/m)
      expect(content, file).not.toMatch(/\{\{\s*\$frontmatter/)
      expect(content, file).not.toMatch(/^#{1,6}\s.*\{#[\w-]+\}\s*$/m)
      expect(read(file), file).not.toContain('[!code ')
    }
  })

  test('robots.txt allows AI agents and declares content signals', () => {
    const robots = read('robots.txt')
    expect(robots).toMatch(/^User-agent: \*$/m)
    expect(robots).toMatch(/^User-agent: GPTBot$/m)
    expect(robots).toMatch(/^User-agent: ClaudeBot$/m)
    expect(robots).toMatch(/^Content-Signal: search=yes, ai-input=yes, ai-train=yes$/m)
    expect(robots).not.toMatch(/^Disallow: \/\s*$/m)
    expect(robots).toContain(`Sitemap: ${SITE_URL}/sitemap.xml`)
  })

  test('Cloudflare Pages config ships with the site', () => {
    const headers = read('_headers')
    expect(headers).toMatch(/^\/\n {2}Link: .*rel="describedby"/m)

    const routes = JSON.parse(read('_routes.json')) as { include: string[], exclude: string[] }
    expect(routes.include).toEqual(['/*'])
    expect(routes.exclude).toContain('/assets/*')
    expect(routes.exclude.length).toBeLessThanOrEqual(100)
  })

  test('agent skills index matches its published skills', () => {
    const index = JSON.parse(read('.well-known/agent-skills/index.json')) as {
      $schema: string
      skills: Array<{ name: string, type: string, url: string, digest: string }>
    }
    expect(index.$schema).toBe('https://schemas.agentskills.io/discovery/0.2.0/schema.json')
    expect(index.skills.map(({ name }) => name)).toEqual(['stock-indicators-dotnet'])

    for (const skill of index.skills) {
      const bytes = readFileSync(join(DIST, skill.url))
      expect(skill.type).toBe('skill-md')
      expect(skill.digest).toBe(`sha256:${createHash('sha256').update(bytes).digest('hex')}`)
      // the hand-written skill must name the same identity as the generated output
      const markdown = bytes.toString('utf8')
      expect(markdown).toContain(`package: ${PACKAGE_ID}`)
      expect(markdown).toContain(`docs_version: ${DOCS_VERSION}`)
      expect(markdown).toContain(`dotnet add package ${PACKAGE_ID}`)
    }
  })

  test('legacy redirects land on canonical pages in one hop', () => {
    const rules = read('_redirects').split('\n')
      .filter((line) => line.trim() && !line.startsWith('#'))
      .map((line) => line.trim().split(/\s+/))
    const sources = new Set(rules.map(([source]) => source))
    const canonical = [...read('sitemap.xml').matchAll(/<loc>([^<]+)<\/loc>/g)]
      .map((match) => match[1].slice(SITE_URL.length) || '/')

    expect(rules.length).toBeGreaterThan(100)
    expect(rules.filter(([, target]) => !isPage(target))).toEqual([])
    expect(canonical.filter((route) => sources.has(route))).toEqual([])
  })
})

test('rendered pages declare canonical and page-specific metadata', async ({ page }) => {
  const cases = [
    { path: '/indicators/sma', url: `${SITE_URL}/indicators/sma`, title: 'Simple Moving Average (SMA)' },
    { path: '/guide/', url: `${SITE_URL}/guide/`, title: 'Guide' },
    { path: '/', url: `${SITE_URL}/`, title: 'Stock Indicators for .NET' },
  ]

  for (const { path, url, title } of cases) {
    await page.goto(path, { waitUntil: 'domcontentloaded' })
    await expect(page.locator('link[rel="canonical"]')).toHaveAttribute('href', url)
    await expect(page.locator('meta[property="og:url"]')).toHaveAttribute('content', url)
    await expect(page.locator('meta[property="og:title"]')).toHaveCount(1)
    await expect(page.locator('meta[property="og:title"]')).toHaveAttribute('content', new RegExp(`^${title.replace(/[.()]/g, '\\$&')}`))
    await expect(page.locator('meta[property="og:description"]')).toHaveCount(1)
  }
})
