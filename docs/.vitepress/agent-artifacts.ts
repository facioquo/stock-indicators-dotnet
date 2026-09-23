import { execSync } from 'child_process'
import { createHash } from 'crypto'
import { existsSync, readdirSync, readFileSync, writeFileSync } from 'fs'
import path from 'path'
import { markdownPath, pageRoute } from './routes'

// Post-processes the agent-facing output of vitepress-plugin-llms so the
// Markdown it emits is portable (no VitePress-only syntax), self-describing
// (identity and provenance), and navigable without falling back to HTML.

export const SITE_URL = 'https://dotnet.stockindicators.dev'
export const PACKAGE_ID = 'FacioQuo.Stock.Indicators'
export const DOCS_VERSION = 'v3'

const SKILLS_DIR = '.well-known/agent-skills'
const SKILLS_SCHEMA = 'https://schemas.agentskills.io/discovery/0.2.0/schema.json'

export interface BuildInfo {
  generated: string
  commit?: string
}

export function readBuildInfo(): BuildInfo {
  let commit = process.env.GITHUB_SHA
  if (!commit) {
    try {
      commit = execSync('git rev-parse HEAD', { encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] }).trim()
    } catch {
      commit = undefined
    }
  }
  return { generated: new Date().toISOString(), commit }
}

export function identityBlock({ generated, commit }: BuildInfo): string {
  const lines = [
    `- NuGet package: \`${PACKAGE_ID}\``,
    `- Namespace: \`${PACKAGE_ID}\``,
    `- Documentation version: ${DOCS_VERSION} (current)`,
    '- Former package: `Skender.Stock.Indicators` (v2, superseded; see the migration guide below)',
    `- Install: \`dotnet add package ${PACKAGE_ID}\``,
    '- Repository: https://github.com/facioquo/stock-indicators-dotnet',
    `- Documentation: ${SITE_URL}`,
    '- Markdown: append `.md` to any page URL, or request it with `Accept: text/markdown`',
    `- Generated: ${generated}${commit ? ` from commit ${commit}` : ''}`,
  ]
  return lines.join('\n')
}

const ALERTS: Record<string, string> = {
  info: 'NOTE',
  note: 'NOTE',
  tip: 'TIP',
  important: 'IMPORTANT',
  warning: 'WARNING',
  caution: 'CAUTION',
  danger: 'CAUTION',
}

// Fence markers, including fences quoted inside a rendered container.
const FENCE = /^(?:\s*>)*\s*(`{3,}|~{3,})/

/**
 * Applies `prose` to each line outside fenced code blocks and `code` to each
 * line inside them (fence markers themselves pass through untouched).
 */
function mapLines(
  content: string,
  prose: (line: string) => string,
  code: (line: string) => string = (line) => line
): string {
  let fence: string | undefined
  return content.split('\n').map((line) => {
    const marker = line.match(FENCE)?.[1]
    if (marker && (!fence || marker.startsWith(fence))) {
      fence = fence ? undefined : marker
      return line
    }
    return fence ? code(line) : prose(line)
  }).join('\n')
}

/**
 * Removes VitePress-only markup: `{#id}` heading anchors in prose, and
 * `[!code ...]` line annotations in code (diff markers become plain words).
 */
export function stripVitePressSyntax(content: string): string {
  return mapLines(
    content,
    (line) => line.replace(/^(#{1,6}\s.*?)\s*\{#[\w-]+\}\s*$/, '$1'),
    (line) => line
      .replace(/\[!code --\]/g, 'removed')
      .replace(/\[!code \+\+\]/g, 'added')
      .replace(/\s*(?:\/\/|#|<!--)\s*\[!code [^\]]+\]\s*(?:-->)?/g, '')
  )
}

/**
 * Rewrites VitePress `::: type Title` containers as GitHub-style alert
 * blockquotes (`details` becomes a plain titled blockquote). Nested
 * containers become nested blockquotes.
 */
export function renderContainers(content: string): string {
  const out: string[] = []
  let fence: string | undefined
  let depth = 0

  for (const line of content.split('\n')) {
    const quote = '> '.repeat(depth)
    const marker = line.match(FENCE)?.[1]
    if (marker && (!fence || marker.startsWith(fence))) {
      fence = fence ? undefined : marker
    } else if (!fence) {
      const open = line.match(/^:::\s*(\w+)\s*(.*)$/)
      if (open) {
        const alert = ALERTS[open[1].toLowerCase()]
        const title = open[2].replace(/\{[^}]*\}/g, '').trim() // drops `{no-title}`-style attributes
        const inner = `${quote}>`
        if (alert) out.push(`${inner} [!${alert}]`)
        if (title) out.push(`${inner} **${title}**`, inner)
        depth++
        continue
      }
      if (/^:::\s*$/.test(line) && depth > 0) {
        if (out.at(-1) === quote.trimEnd()) out.pop()
        depth--
        continue
      }
    }
    if (!depth) out.push(line)
    else if (line.trim()) out.push(`${quote}${line}`)
    else if (fence || out.at(-1) !== quote.trimEnd()) out.push(quote.trimEnd())
  }
  return out.join('\n')
}

/** Resolves `{{ $frontmatter.<key> }}` interpolations against the page's frontmatter. */
export function resolveFrontmatterTemplates(
  content: string,
  frontmatter: Record<string, string>
): string {
  return mapLines(content, (line) =>
    line.replace(/\{\{\s*\$frontmatter\.(\w+)\s*\}\}/g, (match, key: string) => frontmatter[key] ?? match))
}

/**
 * Points root-relative documentation links at their Markdown representation
 * when one exists. External URLs, anchors, and routes without a `.md`
 * counterpart are left as they are.
 */
export function rewriteInternalLinks(content: string, markdownRoutes: Set<string>): string {
  const toMarkdown = (target: string): string => {
    const [pathname, fragment] = target.split(/(?=#)/, 2)
    if (!pathname.startsWith('/') || pathname.startsWith('//')) return target
    const route = pathname.replace(/\/$/, '')
    if (!route || path.posix.extname(route)) return target
    const markdown = `${route}.md`
    return markdownRoutes.has(markdown) ? `${markdown}${fragment ?? ''}` : target
  }

  return mapLines(content, (line) => line
    .replace(/\]\((\/[^)\s]*)((?:\s+"[^"]*")?)\)/g, (_m, target: string, title: string) => `](${toMarkdown(target)}${title})`)
    .replace(/^(\s*\[[^\]]+\]:\s*)(\/\S+)/, (_m, label: string, target: string) => `${label}${toMarkdown(target)}`))
}

/** Reads flat `key: value` and folded `key: >-` scalars from a YAML frontmatter body. */
export function parseFrontmatter(yaml: string): Record<string, string> {
  const result: Record<string, string> = {}
  let key: string | undefined
  for (const line of yaml.split('\n')) {
    const entry = line.match(/^([\w-]+):\s*(.*)$/)
    if (entry) {
      key = entry[1]
      result[key] = /^[>|][-+]?$/.test(entry[2]) ? '' : entry[2].replace(/^(['"])(.*)\1$/, '$2')
    } else if (key && /^\s+\S/.test(line)) {
      result[key] = `${result[key]} ${line.trim()}`.trim()
    }
  }
  return result
}

const FRONTMATTER = /^---\n([\s\S]*?)\n---\n/

/** Normalizes one Markdown document (frontmatter plus body). */
export function normalizeDocument(
  content: string,
  markdownRoutes: Set<string>,
  extraFrontmatter: Record<string, string> = {}
): string {
  const match = content.match(FRONTMATTER)
  const yaml = match?.[1] ?? ''
  const body = match ? content.slice(match[0].length) : content
  const frontmatter = parseFrontmatter(yaml)

  const extra = Object.entries(extraFrontmatter)
    .filter(([key]) => !(key in frontmatter))
    .map(([key, value]) => `${key}: ${value}`)
  const header = match || extra.length
    ? `---\n${[yaml, ...extra].filter(Boolean).join('\n')}\n---\n`
    : ''

  const normalized = rewriteInternalLinks(
    renderContainers(stripVitePressSyntax(resolveFrontmatterTemplates(body, frontmatter))),
    markdownRoutes
  )
  return header + normalized
}

/**
 * Normalizes a bundle of concatenated documents, each introduced by its own
 * frontmatter block (as in `llms-full.txt`).
 */
export function normalizeBundle(content: string, markdownRoutes: Set<string>): string {
  const starts = [...content.matchAll(/(?:^|\n)(?=---\nurl: )/g)].map((m) => m.index! + (m[0] ? 1 : 0))
  if (!starts.length) return normalizeDocument(content, markdownRoutes)

  const preamble = content.slice(0, starts[0])
  const documents = starts.map((start, i) => content.slice(start, starts[i + 1]))
  return preamble + documents.map((doc) => normalizeDocument(doc, markdownRoutes)).join('')
}

function listFiles(dir: string, root = dir): string[] {
  return readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name)
    if (entry.isDirectory()) return entry.name === 'assets' ? [] : listFiles(full, root)
    return [path.relative(root, full).split(path.sep).join('/')]
  })
}

function writeSkillsIndex(outDir: string): void {
  const skillsDir = path.join(outDir, SKILLS_DIR)
  if (!existsSync(skillsDir)) return

  const skills = readdirSync(skillsDir, { withFileTypes: true })
    .filter((entry) => entry.isDirectory() && existsSync(path.join(skillsDir, entry.name, 'SKILL.md')))
    .map(({ name }) => {
      const bytes = readFileSync(path.join(skillsDir, name, 'SKILL.md'))
      const frontmatter = parseFrontmatter(bytes.toString('utf8').match(FRONTMATTER)?.[1] ?? '')
      if (frontmatter.name !== name) {
        throw new Error(`Agent skill "${name}" must declare a matching \`name\` in its frontmatter.`)
      }
      return {
        name,
        type: 'skill-md',
        description: frontmatter.description,
        url: `/${SKILLS_DIR}/${name}/SKILL.md`,
        digest: `sha256:${createHash('sha256').update(bytes).digest('hex')}`,
      }
    })

  writeFileSync(
    path.join(skillsDir, 'index.json'),
    `${JSON.stringify({ $schema: SKILLS_SCHEMA, skills }, null, 2)}\n`
  )
}

/**
 * Rewrites the agent-facing artifacts in a built site; run from VitePress
 * `buildEnd` with the (rewritten) source path of every page.
 */
export function writeAgentArtifacts(outDir: string, sourcePages: string[], build: BuildInfo): void {
  const pages = listFiles(outDir).filter((file) => file.endsWith('.md') && !file.startsWith('.well-known/'))
  const markdownRoutes = new Set(pages.map((file) => `/${file}`))
  const routeByMarkdown = new Map(sourcePages.map((source) => {
    const route = pageRoute(source)
    return [markdownPath(route), route]
  }))

  const provenance = (file: string): Record<string, string> => {
    const route = routeByMarkdown.get(`/${file}`)
    if (!route) throw new Error(`No source page produces /${file}; the llms plugin and routes.ts disagree.`)
    return {
      package: PACKAGE_ID,
      docs_version: DOCS_VERSION,
      canonical: `${SITE_URL}${route}`,
      generated: build.generated,
      ...(build.commit ? { commit: build.commit } : {}),
    }
  }

  for (const file of pages) {
    const target = path.join(outDir, file)
    writeFileSync(target, normalizeDocument(readFileSync(target, 'utf8'), markdownRoutes, provenance(file)))
  }

  const llmsTxt = path.join(outDir, 'llms.txt')
  if (existsSync(llmsTxt)) {
    writeFileSync(llmsTxt, normalizeBundle(readFileSync(llmsTxt, 'utf8'), markdownRoutes))
  }

  // llms.txt carries the identity block through its template; the full bundle
  // has no template, so it gets the same block as a preamble.
  const llmsFull = path.join(outDir, 'llms-full.txt')
  if (existsSync(llmsFull)) {
    const preamble = `# Stock Indicators for .NET\n\n## Library identity\n\n${identityBlock(build)}\n\n`
    writeFileSync(llmsFull, preamble + normalizeBundle(readFileSync(llmsFull, 'utf8'), markdownRoutes))
  }

  writeSkillsIndex(outDir)
}
