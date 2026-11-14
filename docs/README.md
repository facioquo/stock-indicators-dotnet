# Documentation website

Visit [dotnet.stockindicators.dev](https://dotnet.stockindicators.dev) to read our documentation.

This site is built with [VitePress](https://vitepress.dev) and deployed to Cloudflare Pages.

## Local development

The site renders charts via [`@facioquo/indy-charts`](https://github.com/facioquo/stock-charts) from GitHub Packages, which needs an authenticated install.

To enable `pnpm install` or `pnpm add` you'll need to extend the scopes of your local GitHub CLI (`gh`) auth token with `gh auth refresh --scopes read:packages`, confirm with `gh auth status`, then store in your user level `~/.npmrc` (not in the project `.npmrc`), which is read at a trusted level where variable expansion is not restricted.

```bash
# Write to your user-level ~/.npmrc (not the project .npmrc)
pnpm config set @facioquo:registry https://npm.pkg.github.com
pnpm config set "//npm.pkg.github.com/:_authToken" "$(gh auth token)"
```

Then `pnpm install` (or the equivalent VS Code task) pulls the package, then run:

```bash
pnpm run docs:dev
```

The site will open at `http://localhost:5173/`

### GitHub token (optional)

The contributors list is automatically populated using the GitHub API. The build process will auto-detect your GitHub token from:

1. `GITHUB_TOKEN` environment variable (if set)
2. GitHub CLI (`gh auth token`) if authenticated

To authenticate with GitHub CLI:

```bash
gh auth login
```

If no token is found, the site will build successfully but the contributors list will be empty.

## Building

```bash
pnpm run docs:build
```

The built site will be in `.vitepress/dist/`

Alternative production preview build:

```bash
pnpm run docs:preview
```
