# Architecture decision records

This folder holds the project's Architecture Decision Records (ADRs) using the [MADR 4.0](https://adr.github.io/madr/) template.

## What goes here

Decisions that:

- Set or change a cross-cutting architectural pattern (e.g. dual-track BufferList + StreamHub, framework targets, cache mutation discipline, public-API extension surface).
- Are non-obvious to a future reader of the code and would be re-asked or re-litigated without a written record.
- Have a defensible alternative that was rejected and the rejection rationale is worth preserving.

Decisions that **don't** go here:

- Per-indicator algorithm choices — those live in the indicator's xmldoc and the doc-site page.
- Release-cycle tactics or task lists — those live in GitHub Issues.
- Tool/CLI conventions — those live in the relevant `AGENTS.md` or skill.

## File naming

`NNNN-kebab-case-title.md` where `NNNN` is a 4-digit sequence starting at `0001`. The number is permanent once assigned; do not renumber when a later ADR supersedes an earlier one — instead set the superseded ADR's `status:` to `superseded by [NNNN](NNNN-...md)`.

## Template

Use MADR 4.0:

```markdown
---
status: {proposed | accepted | rejected | superseded by [NNNN](NNNN-...md)}
date: YYYY-MM-DD
deciders: {names or roles}
consulted: {optional — names or sources}
informed: {optional — names or sources}
---

# {Decision title in sentence case}

## Context and problem statement

## Decision drivers

## Considered options

## Decision outcome

Chosen option: "...", because ...

### Consequences

### Confirmation

## Pros and cons of the options

## More information
```

## Publishing

ADR files in this folder are deliberately excluded from the published VitePress site (see `docs/.vitepress/config.mts` `srcExclude`). They are internal records for maintainers and AI agents, not user-facing documentation.

## Index

- [0001 — Use dual-track BufferList and StreamHub for incremental indicators](0001-dual-track-bufferlist-streamhub.md)
