# Markdown tooling in this repository

Use this when changing markdownlint configuration, adding an allowed HTML element, or working out why a file is or is not linted. The tooling is already installed; nothing here needs to be set up per clone.

## Prerequisites

- Node.js with `npx` (CI and agents) or `pnpm` (the VS Code tasks). There is no root `package.json`; both fetch `markdownlint-cli2` on demand, and `echo y |` answers the install prompt.

## Configuration

| File | Role |
| ---- | ---- |
| `.markdownlint-cli2.jsonc` | Root config: `globs`, `ignores`, `gitignore: true`, and the rule set |
| `docs/.markdownlint-cli2.jsonc` | Overrides for the docs site, including `MD033: false` for Vue components |

Root rule choices that shape authoring:

- `MD003` ATX headers, `MD004` dash bullets, `MD007` two-space indent, `MD029` ordered numbering, `MD046` fenced code, `MD048` backtick fences.
- `MD013` off — no line-length limit, so never hard-wrap.
- `MD024` `siblings_only` — duplicate header text is allowed under different parents.
- `MD033` — an explicit allow-list of HTML elements; add an element there only when no Markdown syntax produces it.

Files outside the lint run:

- Anything `.gitignore` excludes.
- The `ignores` list, including `.claude/skills/**` (a symlink to `.agents/skills`, which is linted directly), BenchmarkDotNet artifacts, and `tools/performance/baselines/**`.

Prefer a rule override in `config` to an inline `<!-- markdownlint-disable MD### -->` block; use the inline form only for a narrow case restructuring cannot fix, and close it with the matching `enable`.

## Commands and tasks

| Purpose | Command | VS Code task |
| ------- | ------- | ------------ |
| Lint the repository | `echo y \| npx markdownlint-cli2` | `Lint: Markdown` |
| Fix the repository | `echo y \| npx markdownlint-cli2 --fix` | `Lint: Markdown (fix)` |
| Lint named files only | `npx markdownlint-cli2 --no-globs a.md b.md` | none |

CI runs the repository lint in the quick-check job of `.github/workflows/ci.yml`.

## Editor integration

- `.vscode/extensions.json` recommends `DavidAnson.vscode-markdownlint` and `EditorConfig.EditorConfig`.
- `.vscode/settings.json` sets the markdownlint extension as the Markdown formatter, with format on save and `source.fixAll.markdownlint` on explicit save.
- `.editorconfig` keeps trailing whitespace in `*.md`.

## Scripts

- [check.sh](../scripts/check.sh) reports whether a global `markdownlint-cli2`, a config file, and the VS Code extension are present.
- [install.sh](../scripts/install.sh) installs `markdownlint-cli2` globally. The `npx` commands above do not need it.

## Verify

After a configuration change, run `echo y | npx markdownlint-cli2` and confirm zero errors across the repository, then lint one file under `docs/` with `--no-globs` to confirm the docs overrides still apply.
