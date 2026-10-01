---
name: markdown
description: Format and lint Markdown in this repository against GitHub Flavored Markdown and its markdownlint-cli2 configuration — headers, lists, code fences, callouts (VitePress containers on docs-site pages, GitHub alerts elsewhere), allowed HTML, reference syntax, and line wrapping. Use when creating or editing any .md file, when markdownlint or the CI "Lint markdown" step reports errors, or when changing a .markdownlint-cli2.jsonc file.
---

# Markdown authoring

Markdown mechanics only: syntax, structure, and reference form. The structure of an indicator page on the docs site belongs to the documentation skill.

Load the [formatting standards](references/formatting-standards.md) before writing or restructuring a file; they hold the rules a linter cannot check.

## Workflow

### Step 1: Auto-fix

```bash
npx markdownlint-cli2 --no-globs path/to/file.md --fix
```

Run from the repository root. From a subfolder the root `.markdownlint-cli2.jsonc` is not read, and default rules such as `MD013` line length fire. `--no-globs` lints only the paths you pass; without it the config's `globs` add every Markdown file in the repository.

Auto-fix handles bullet style, ATX headers, blank lines around headers and fences, and list indentation. It does not handle sentence case, hard-wrapped paragraphs, link targets, callout syntax, or reference form.

### Step 2: Review what the linter cannot see

Apply the [formatting standards](references/formatting-standards.md). The usual misses:

- **Headers** — sentence case ("How to use", not "How To Use").
- **Line wrapping** — each paragraph, list item, and table cell is one source line. `MD013` is off, so the linter never flags a hard wrap.
- **Callouts** — `:::` containers on docs-site pages, GitHub alerts everywhere else.
- **References** — Markdown links for files; skills, tools, and agents in prose.

### Step 3: Verify

```bash
npx markdownlint-cli2 --no-globs path/to/file.md
```

Zero errors required. CI runs `echo y | npx markdownlint-cli2` over the whole repository; the VS Code tasks `Lint: Markdown` and `Lint: Markdown (fix)` do the same. Then work through the [validation checklist](references/validation-checklist.md) for the semantic checks.

## Tooling and configuration

Changing a `.markdownlint-cli2.jsonc` file, adding an allowed HTML element, or diagnosing why a file is or is not linted → load the [markdown tooling reference](references/setup-guide.md).

## Do not do these

- Do not hard-wrap prose at a column. A wrapped paragraph turns every later edit into a reflow diff.
- Do not put a `:::` container in Markdown that GitHub renders; it shows as literal text. Do not put a GitHub alert (`> [!NOTE]`) on a docs-site page; the site convention is containers, which the build converts to alerts for the `.md` twins agents read.
- Do not use `#file:`, `#skill:`, or `#tool:` markers in any file.
- Do not link from a skill into another skill's folder or up to an AGENTS.md; name the other skill in prose.
- Do not add a `Last updated:` footer, trailing `---` separator, or date stamp. Git history records when a file changed.
- Do not silence a rule with an inline `<!-- markdownlint-disable -->` when restructuring the content fixes it.
