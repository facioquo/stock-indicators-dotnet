# Markdown validation checklist

Use this after the lint run passes, before committing a Markdown change. Each item is one markdownlint cannot verify.

## Structure

- [ ] `npx markdownlint-cli2 --no-globs <file>` reports zero errors.
- [ ] Headers are sentence case and skip no level.
- [ ] Every paragraph, list item, and table cell is one source line.
- [ ] Every fence has a language identifier.
- [ ] The file ends with one trailing newline and no footer, separator, or date stamp.

## Rendering target

- [ ] Callouts match where the file renders: `:::` containers on docs-site pages, GitHub alerts elsewhere.
- [ ] Vue components appear only on docs-site pages.
- [ ] Mermaid appears only in GitHub-rendered files, with quoted labels, `<br/>` line breaks, and stroke styling.
- [ ] Every image has alt text.

## References

- [ ] Every relative link resolves to an existing file, and every `#anchor` to an existing heading.
- [ ] Skills, tools, and agents are named in prose; none is backtick-wrapped.
- [ ] No `#file:`, `#skill:`, or `#tool:` markers.
- [ ] A skill links only to files inside its own folder.

## Content

- [ ] Present tense; no history, "previously", or migration narrative.
- [ ] No rule restated from another file — link or name its owner instead.
