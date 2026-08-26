# Documentation website

This folder contains the VitePress documentation site for Stock Indicators at [dotnet.stockindicators.dev](https://dotnet.stockindicators.dev).

Load #skill:vitepress for VitePress configuration, routing, theme, and component guidance.

Load #skill:markdown for general Markdown authoring standards, linting workflow, and validation.

## Quick start

```bash
# one-time per gh CLI token (see "Indy Charts" below for why)
gh auth refresh --scopes read:packages

# from /docs folder — opens at http://localhost:5173/
pnpm install
pnpm run docs:dev
```

## Indy Charts

Indy Charts ([`@facioquo/indy-charts`](https://github.com/facioquo/stock-charts/pkgs/npm/indy-charts)) is a public npm package hosted in the facioquo GitHub org's [GitHub Packages](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-npm-registry#installing-a-package) registry, with source code in the [`facioquo/stock-charts`](https://github.com/facioquo/stock-charts) repository. It is not, and must not be, published to the npmjs.org public registry.

## Local development auth to enable `pnpm install` or `pnpm add`

Extend the scopes of your local GitHub CLI (`gh`) auth token with `gh auth refresh --scopes read:packages`, confirm with `gh auth status`, then store in your user level `~/.npmrc` (not in the project `.npmrc`), which is read at a trusted level where variable expansion is not restricted.

```bash
# Write to your user-level ~/.npmrc (not the project .npmrc)
pnpm config set @facioquo:registry https://npm.pkg.github.com
pnpm config set "//npm.pkg.github.com/:_authToken" "$(gh auth token)"
```

Then `pnpm install` (or the equivalent VS Code task) pulls the package.

## Build and preview

```bash
# Production build
pnpm run docs:build

# Preview production build (port 4173)
pnpm run docs:preview
```

## Visual inspection with Playwright

Use the Playwright MCP tool to visually inspect docs work against the dev server.

Start the dev server first (port 5173):

```bash
# from /docs folder
pnpm run docs:dev
```

Then use the #tool:playwright MCP tool to navigate, screenshot, and inspect pages:

- Navigate to `http://localhost:5173/<page-path>` to inspect any page
- Take screenshots to verify layout, typography, and component rendering
- Inspect element state, check link targets, and validate page structure

## Content guidelines

- Add indicator pages to the `indicators/` directory
- Place image assets in `.vitepress/public/assets/`
- Prefer Markdown image syntax (`![alt](image.png)`) for local images — VitePress measures them at build time and emits `width`/`height`, which prevents layout shift. Reserve HTML `<img>` for remote images (badges, shields) that cannot be measured
- Optimize images to webp (example): `cwebp -resize 832 0 -q 100 input.png -o output.webp`
- All pages require YAML front matter with title, description, and layout metadata
- Never add arbitrary linebreaks in Markdown prose or list items due to line length

## VitePress alert blocks

Use VitePress native container syntax instead of GitHub alert syntax in docs pages:

- `::: tip ✨` — helpful suggestions
- `::: note` — neutral asides and clarifications
- `::: important` — points the reader must not miss
- `::: warning 🚩` — important warnings
- `::: caution` — actions with a risk of adverse outcome
- `::: danger` — critical warnings
- `::: info` — informational highlights (default)
- `::: details` — collapsible sections

Text after the container name replaces the default title. To drop the title bar entirely, use the `no-title` attribute: `::: tip {no-title}`.

GitHub alert blocks (`> [!NOTE]`, `> [!WARNING]`) are still preferred in non-website Markdown files.
