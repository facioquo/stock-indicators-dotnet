# Markdown formatting standards

Use this when writing or restructuring any Markdown file in this repository. It covers the rules markdownlint cannot check and the repository-specific choices behind the rules it can.

## Line wrapping

- Write each paragraph, list item, and table cell as one source line, however long.
- `MD013` (line length) is off in `.markdownlint-cli2.jsonc` for this reason; a manual wrap is never required.
- The rule applies to prose owned by any skill or author.

## Headers

- ATX only (`#`, `##`, `###`); never Setext underlines.
- Sentence case: capitalize the first word and proper nouns (GitHub, VitePress, StreamHub, .NET); lowercase articles, prepositions, and conjunctions.
- Sequential hierarchy — never skip a level.
- Bold labels that start a list item also take sentence case ("**Next steps:**").

## Lists

- Hyphen bullets (`-`) only; two-space indent for nested items.
- Ordered lists only when sequence matters, numbered `1.`, `2.`, `3.` (`MD029` style `ordered`).

## Code blocks

- Fenced with backticks, never indented.
- A language identifier on every fence; `plaintext` when none fits.
- An outer fence longer than any fence it contains.

## Callouts

A **docs-site page** is a Markdown file under `docs/` that VitePress builds. Files that `srcExclude` in `docs/.vitepress/config.mts` excludes — such as `docs/AGENTS.md`, `docs/README.md`, `docs/PRINCIPLES.md`, and `docs/decisions/**` — are rendered by GitHub, not the site.

- On a docs-site page, use VitePress containers: `::: tip`, `::: info`, `::: note`, `::: important`, `::: warning`, `::: caution`, `::: danger`, `::: details`. Text after the name replaces the title; `{no-title}` drops it.
- Everywhere else (README files, AGENTS.md files, skills, `.github/`), use GitHub alerts: `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!WARNING]`, `> [!CAUTION]`.

## HTML elements

- Outside `docs/`, `MD033` allows these HTML elements: `a`, `abbr`, `br`, `code`, `details`, `div`, `img`, `kbd`, `p`, `sub`, `summary`, `sup`. Prefer Markdown syntax where it exists.
- `docs/.markdownlint-cli2.jsonc` turns `MD033` off so docs-site pages can use Vue components such as `<ClientOnly>` and `<StockIndicatorChart>`.
- Give every image alt text.

## Reference syntax

- **Files** — standard Markdown links with relative paths. Within a skill, link only to files in that skill's own folder.
- **Skills** — prose: "the testing-standards skill". Never a link into another skill's folder.
- **Tools** — prose, not backtick-wrapped: "the Read tool".
- **Agents** — plain @AgentName, never backtick-wrapped.
- Never use the `#file:`, `#skill:`, or `#tool:` markers.

## Mermaid diagrams

GitHub renders Mermaid; the docs site has no Mermaid plugin, so use Mermaid only in GitHub-rendered files.

- Quote every label: `A["API<br/>gateway"]`.
- Break label lines with `<br/>`, never `\n`.
- Style with strokes, not fills, so the diagram reads in light and dark themes.

## End of file

End with the last content line and a single trailing newline. No footer, no trailing `---` separator, no `Last updated:` stamp.

## Common fixes

| Pattern | Fix |
| ------- | --- |
| Title case in a header or bold label | Sentence case |
| Paragraph wrapped across lines | Join into one line |
| `*` or `+` bullets | `-` |
| Setext header | ATX |
| Fence without a language | Add one, or `plaintext` |
| GitHub alert on a docs-site page | VitePress container |
| `:::` container outside the docs site | GitHub alert |
| Backtick-wrapped tool name or @AgentName | Plain prose |
| Backtick-escaped file path used as a reference | Markdown link |
| Ordered list for unordered items | Hyphen list |
