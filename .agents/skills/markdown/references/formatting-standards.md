# Markdown formatting standards

Complete formatting requirements for Markdown files in this organization.

## Editorial style

- **Voice:** Present tense, imperative mood ("Run the command" not "You should run")
- **Headers:** Sentence case only (first word + proper nouns capitalized)
- **Focus:** Current directives only; exclude historical context and migration details
- **Tone:** Direct and actionable for autonomous agent execution

## Headers

- **Style:** ATX only (`#`, `##`, `###`); never Setext (`===`, `---`)
- **Capitalization:** Sentence case only
  - Capitalize: First word + proper nouns (GitHub, TypeScript, MADR)
  - Lowercase: Articles (a, an, the), prepositions (of, to, for), conjunctions (and, but, or)
- **Spacing:** Blank line before and after every header
- **Hierarchy:** Sequential (no skipping levels: `#` → `##` → `###`)

## Lists

- **Bullets:** Always use hyphens (`-`); never asterisks (`*`) or plus signs (`+`)
- **Indentation:** Must align with body text start position (GitHub Flavored Markdown standard)
- **Ordering:** Use ordered lists (1., 2., 3.) only when sequence matters
- **Bold labels:** Use sentence case for bold labels that start list items
- Never add arbitrary linebreaks due to line length

## Code blocks

- **Fencing:** Always use fenced blocks (` ``` `)
- **Language identifier:** Required on all fences; use `plaintext` when language is unknown
- **Spacing:** Blank line before and after fences
- **Nesting:** Outer fences must have more backticks than inner fences
- **Indenting:** Never indent code blocks, except where directly related to preceding list item

## Reference syntax

### Skill references

- Use `#skill:skill-name` for intentional context loading
- Use standard markdown links for optional references
- Never use #file: for skill subordinate files

### File references

- Use #file: sparingly for intentional context loading
- Use standard markdown links for optional references
- Prefer markdown links over backtick-escaped paths
- Never use #file: in entry point files (AGENTS.md, root instructions)

### Tool references

- Use `#tool:category/name` format
- Do not wrap in backticks
- MCP servers use `server/tool` format (e.g., `#tool:github/pull_request_read`)

### Agent references

- Use `@AgentName` syntax
- Wrap in backticks in documentation (e.g., `` `@Planner` ``)
- Use plain `@AgentName` in agent files for invocation

## Mermaid diagrams

- Use ` ```mermaid ` with language identifier
- Always bar labels within square brackets
- Use stroke styling, not fill colors (theme compatibility)
- Test rendering in both dark and light themes

## HTML elements

Only these elements are allowed:

- `<details>`, `<summary>` (collapsible sections)
- `<br>` (line breaks in tables)
- `<sub>`, `<sup>`, `<kbd>`, `<abbr>` (semantic formatting)
- `<a>`, `<img>` (links and images when Markdown syntax insufficient)

## Common patterns to fix

| Error pattern | Fix |
| ------------- | --- |
| Title case in headers | Convert to sentence case |
| Title case in bold labels | Convert to sentence case |
| Arbitrary line breaks | Remove unnatural line breaks in prose and lists |
| Asterisk bullets | Replace with hyphens |
| Setext headers | Convert to ATX |
| Missing blank lines | Add around headers and code blocks |
| Backticks in #file: | Remove backticks |
| #file: in entry points | Use markdown links |
| Ordered lists (non-sequential) | Convert to unordered |
| Trailing punctuation after #file: | Remove or add space |
| Missing language in fences | Add language or `plaintext` |
