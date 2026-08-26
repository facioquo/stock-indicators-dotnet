---
name: markdown
description: Format and validate Markdown files following GitHub Flavored Markdown standards with automated linting and manual semantic review
---

# Markdown authoring

Format and validate Markdown files following GitHub Flavored Markdown (GFM) standards, VS Code language features, and organizational conventions with automated structural fixes and manual semantic review.

## When to use this skill

- Creating or modifying any Markdown file (`.md`)
- Fixing markdown linting errors
- Setting up markdown tooling in a new repository
- Validating markdown files before committing
- Converting content to conform to organizational markdown standards

## Required tools

- `read`, `edit` — core file manipulation
- `execute` — run linting tasks and commands
- `search` — find markdown files and patterns

## Workflow

### Step 1: Run automated linting with fixes

Execute markdownlint with auto-fix to resolve structural issues:

```bash
npx markdownlint-cli2 --no-globs {filepath} --fix
```

**Note:** Use `--no-globs` when linting a single explicit file to prevent unintended glob expansion. For repository-wide linting or CI jobs, omit the flag or use explicit globs/CI-configured file lists.

**What auto-fix handles:**

- Bullet style (converts `*` and `+` to `-`)
- Header style (converts Setext to ATX)
- Blank lines around headers and code blocks
- Code fence style and language identifiers
- Nested code block fence lengths

**What auto-fix does NOT handle (requires manual review):**

- Sentence case in headers
- Sentence case in bold labels
- Content organization
- Link validity
- Semantic correctness

### Step 2: Manual semantic review and fixes

Review the file for issues that automated linting cannot fix:

**Headers:**

- Convert to sentence case: "How To Use" → "How to use"
- Capitalize only first word + proper nouns (GitHub, TypeScript, MADR)
- Lowercase articles (a, an, the), prepositions (of, to, for), conjunctions (and, but, or)

**Bold labels at list start:**

- Convert to sentence case: "**Next Steps:**" → "**Next steps:**"

**Reference syntax:**

- Remove backticks from #file: references: `` `#file:path` `` → `#file:path`
- Remove trailing punctuation after #file: tokens
- Replace #file: with markdown links in entry point files (AGENTS.md)

**Lists:**

- Convert ordered lists to unordered hyphen lists for non-sequential items
- Ensure proper indentation alignment
- Never add arbitrary linebreaks due to line length

**Code blocks:**

- Add language identifier to all fenced blocks (use `plaintext` if unknown)
- Ensure nested blocks have longer outer fences

See [formatting standards](references/formatting-standards.md) for complete rules.

### Step 3: Validate with linting

Run markdownlint without fixes to verify zero errors:

```bash
npx markdownlint-cli2 --no-globs {filepath}
```

**Expected output:** No errors or warnings

**If errors remain:**

- Review error messages for specific violations
- Fix issues manually
- Re-run validation
- Repeat until zero errors

### Step 4: Content quality checks

Verify content-level requirements:

1. **Links:** All internal links resolve to existing files
2. **Code fences:** All blocks have language identifiers
3. **Headers:** Sentence case, ATX style, sequential hierarchy (no skipping levels)
4. **Lists:** Hyphen bullets only, proper indentation
5. **Diagrams:** Mermaid diagrams use stroke styling (not fill colors)
6. **HTML:** Only allowed elements, all images have alt text

See [validation checklist](references/validation-checklist.md) for complete quality checks.

### Step 5: Optional continuous feedback

For iterative editing sessions, use watch mode for continuous feedback:

```bash
npx markdownlint-cli2 --no-globs {filepath} --watch
```

This provides real-time linting feedback during edits.

### Step 6: Repository setup (if tooling missing)

If markdown tooling is not configured in the repository, set up the complete stack:

1. **Install VS Code extensions** (vscode-markdownlint, github-markdown-preview, EditorConfig)
2. **Configure VS Code settings** (defaultFormatter, formatOnSave, codeActionsOnSave)
3. **Create linting configuration** (.markdownlint-cli2.jsonc with baseline rules)
4. **Add VS Code tasks** ("Lint: Markdown", "Lint: Markdown (auto-fix)")
5. **Verify alignment** (settings.json, extensions.json, tasks.json, editorconfig)
6. **Test setup** (run linting on existing files)

See [setup guide](references/setup-guide.md) for step-by-step configuration.

## Completion criteria

- [ ] All structural linting errors resolved (zero errors from markdownlint-cli2)
- [ ] All semantic issues fixed (sentence case headers, bold labels)
- [ ] All reference syntax corrected (#file:, markdown links)
- [ ] All content quality checks passed (links, fences, hierarchy)
- [ ] File committed to repository

## Common pitfalls

**Relying solely on automated linting:**

- Auto-fix handles structure, not semantics
- Manual review for sentence case is mandatory
- Content organization requires human judgment

**Using #file: in entry point files:**

- AGENTS.md and root instructions auto-load in many contexts
- #file: references cascade and cause context bloat
- Use standard markdown links instead

**Skipping validation after manual edits:**

- Always re-run linting after manual changes
- New edits may introduce new violations
- Zero-error verification is required

**Incorrect reference syntax:**

- Backticks break #file: token parsing
- Trailing punctuation breaks token resolution
- Wrong #tool: format for MCP servers

## One-time setup

See [markdown tooling setup guide](references/setup-guide.md) for markdownlint-cli2 installation and VS Code extension setup.

## Additional references

- [GitHub Flavored Markdown specification](https://github.github.com/gfm/)
- [markdownlint-cli2 documentation](https://github.com/DavidAnson/markdownlint-cli2)
