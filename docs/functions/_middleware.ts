// Cloudflare Pages middleware: Markdown content negotiation for agents.
// A page request that prefers `text/markdown` gets the page's generated `.md`
// representation (the homepage gets `llms.txt`); everything else passes
// through to the static site unchanged. Routes that never negotiate are
// excluded from invocation in `.vitepress/public/_routes.json`.

interface PagesContext {
  request: Request
  next: () => Promise<Response>
  env: { ASSETS: { fetch: (request: Request) => Promise<Response> } }
}

function quality(accept: string, mediaType: string): number {
  let best = 0
  for (const part of accept.toLowerCase().split(',')) {
    const [type, ...params] = part.split(';').map((value) => value.trim())
    if (type !== mediaType) continue
    const q = params.find((param) => param.startsWith('q='))
    best = Math.max(best, q ? Number(q.slice(2)) || 0 : 1)
  }
  return best
}

/** True when the `Accept` header explicitly ranks Markdown at least as high as HTML. */
export function prefersMarkdown(accept: string | null): boolean {
  if (!accept) return false
  const markdown = quality(accept, 'text/markdown')
  return markdown > 0 && markdown >= quality(accept, 'text/html')
}

/** Maps a page route to its Markdown representation, or `undefined` for non-page paths. */
export function markdownPath(pathname: string): string | undefined {
  if (pathname === '/') return '/llms.txt'
  const route = pathname.replace(/\/$/, '')
  const leaf = route.slice(route.lastIndexOf('/') + 1)
  return leaf.includes('.') ? undefined : `${route}.md`
}

export async function onRequest({ request, next, env }: PagesContext): Promise<Response> {
  const url = new URL(request.url)
  const markdown = markdownPath(url.pathname)
  if (!markdown || (request.method !== 'GET' && request.method !== 'HEAD')) return next()

  if (prefersMarkdown(request.headers.get('Accept'))) {
    const asset = await env.ASSETS.fetch(new Request(new URL(markdown, url), { method: request.method }))
    if (asset.ok) {
      const response = new Response(asset.body, asset)
      response.headers.set('Content-Type', 'text/markdown; charset=utf-8')
      response.headers.set('Vary', 'Accept')
      response.headers.set('Content-Location', markdown)
      return response
    }
  }

  // Same URL, different representations: caches must key on Accept.
  const page = await next()
  const response = new Response(page.body, page)
  response.headers.append('Vary', 'Accept')
  return response
}
