// Cloudflare Pages middleware: Markdown content negotiation for agents.
// A page request that prefers `text/markdown` gets the page's generated `.md`
// representation (the homepage gets `llms.txt`); everything else passes
// through to the static site unchanged.
//
// `.vitepress/public/_routes.json` excludes static paths from invoking this
// function at all. `markdownPath` independently passes through any path whose
// last segment has a file extension, so a path missing from that exclude list
// costs a function invocation (quota), never a wrong response.

import { markdownPath } from '../.vitepress/routes'

export { markdownPath }

// The subset of Cloudflare's `EventContext` this middleware uses. A local
// shape keeps @cloudflare/workers-types' global declarations, which conflict
// with the DOM library, out of the docs' TypeScript and Playwright builds.
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

async function fetchMarkdown(
  env: PagesContext['env'],
  request: Request,
  path: string
): Promise<Response | undefined> {
  try {
    const asset = await env.ASSETS.fetch(new Request(new URL(path, request.url), { method: request.method }))
    if (!asset.ok) return undefined
    const response = new Response(asset.body, asset)
    response.headers.set('Content-Type', 'text/markdown; charset=utf-8')
    response.headers.set('Vary', 'Accept')
    response.headers.set('Content-Location', path)
    return response
  } catch {
    // A failed asset lookup degrades to the HTML page rather than an error.
    return undefined
  }
}

export async function onRequest({ request, next, env }: PagesContext): Promise<Response> {
  const markdown = markdownPath(new URL(request.url).pathname)
  if (!markdown || (request.method !== 'GET' && request.method !== 'HEAD')) return next()

  if (prefersMarkdown(request.headers.get('Accept'))) {
    const response = await fetchMarkdown(env, request, markdown)
    if (response) return response
  }

  // Same URL, different representations: caches must key on Accept.
  const page = await next()
  const response = new Response(page.body, page)
  response.headers.append('Vary', 'Accept')
  return response
}
