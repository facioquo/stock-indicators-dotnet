// The site's route rules, shared by the VitePress config, the build-time agent
// artifact writer, and the Pages Functions middleware so they cannot drift.
// vitepress-plugin-llms applies the same directory-index rule when it names
// each page's `.md` output (see `resolvePagePath` in its patch under
// `docs/patches/`); tests/agent-artifacts.spec.ts checks the two agree.

/** Maps a source page (`guide/index.md`) to its clean-URL route (`/guide/`). */
export function pageRoute(relativePath: string): string {
  return `/${relativePath.replace(/(^|\/)index\.md$/, '$1').replace(/\.md$/, '')}`
}

/**
 * Maps a page route to its Markdown representation (`/guide/` → `/guide.md`,
 * `/` → `/llms.txt`), or `undefined` for paths that are files, not pages.
 */
export function markdownPath(route: string): string | undefined {
  if (route === '/') return '/llms.txt'
  const trimmed = route.replace(/\/$/, '')
  const leaf = trimmed.slice(trimmed.lastIndexOf('/') + 1)
  return leaf.includes('.') ? undefined : `${trimmed}.md`
}
