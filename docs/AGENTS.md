# Documentation website

This folder is the VitePress 2 source for [dotnet.stockindicators.dev](https://dotnet.stockindicators.dev), deployed to Cloudflare Pages.

- Writing or restructuring a page → load the documentation skill.
- Changing `.vitepress/` config, theme, routing, or components → load the vitepress skill.
- Any Markdown edit → the markdown skill governs formatting and linting.

## Commands

Run from `docs/`. The first `pnpm install` needs GitHub Packages auth for `@facioquo/indy-charts`; set it up once per the [local development steps](README.md#local-development).

```bash
pnpm install
pnpm run docs:dev        # http://localhost:5173/
pnpm run docs:build      # output in .vitepress/dist/
pnpm run docs:preview    # serve the build on port 4173
pnpm run test            # Playwright suites; see package.json for per-project scripts
pnpm run test:links      # broken-link check over a fresh build
```

Inspect rendered pages with the Playwright MCP tools against the dev server.

## Agent-facing output

The site serves AI agents alongside people. `pnpm run test:agents` validates everything below against a fresh build.

- `vitepress-plugin-llms` emits `llms.txt`, `llms-full.txt`, and a `.md` twin of every page; `.vitepress/agent-artifacts.ts` post-processes them in `buildEnd` and writes the Agent Skills index.
  - Post-processing adds identity and provenance frontmatter, rewrites link targets to `.md`, renders containers as GitHub alerts, and resolves `{{ $frontmatter.* }}` templates.
- `agent-artifacts.ts` also writes `search-index.json` from the pages `llms.txt` lists; the WebMCP tools in `theme/webmcp.ts` search it and use it as the allowlist for direct page retrieval.
- Agent Skills live in `.vitepress/public/.well-known/agent-skills/<name>/SKILL.md`; the index and digests are generated.
- `.vitepress/routes.ts` owns the route rules (page route, `.md` path) that the config, the artifact writer, and the middleware share; the patched plugin applies the same directory-index rule.
- `functions/_middleware.ts` serves a page's `.md` twin when a request prefers `Accept: text/markdown`. Run it locally with `pnpm exec wrangler pages dev .vitepress/dist` after a build.
- Cloudflare Pages config lives in `.vitepress/public/`: `_headers`, `_redirects`, `_routes.json`, and `robots.txt`. Keep static paths listed under `exclude` in `_routes.json` so they never invoke the middleware.

## Content conventions

- Put static assets in `.vitepress/public/assets/`.
- Use Markdown image syntax for local images so VitePress emits `width`/`height`; reserve HTML `<img>` for remote images it cannot measure.
- Write callouts as VitePress containers (`::: tip`, `::: info`, `::: note`, `::: important`, `::: warning`, `::: caution`, `::: danger`, `::: details`), not GitHub alert blocks. Text after the name replaces the title; `{no-title}` drops it.

## Boundaries

✅ Always add a new page to the `sidebar` in `.vitepress/config.mts` — `llms.txt` derives its table of contents from it; add non-page Markdown to `srcExclude` instead

✅ Always add the old path to `.vitepress/public/_redirects` when a published URL changes

🚫 Never publish `@facioquo/indy-charts` to the npmjs.org registry or commit a registry token to the project `.npmrc`
