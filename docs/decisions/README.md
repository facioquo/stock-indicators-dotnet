# Architecture decision records

This folder holds the project's Architecture Decision Records (ADRs) using the [MADR 4.0](https://adr.github.io/madr/) template, with two deliberate departures from it.

**A record carries no status field, and it is revised in place.** If a record is in this folder on the default branch, it is the decision. Nothing unfinished is committed — an open question stays in its pull request until it is settled, and a rejected alternative belongs in the accepted record's *Considered options*, where a reader sees it against what won.

The reason is the reader. Most readers of this folder are AI coding agents, and an agent that opens a record acts on what it says. A status lifecycle assumes a reader who checks a field, scrolls to the bottom, and follows a link to a successor; that traversal is optional, its omission is silent, and the result is work built on a decision the project already moved past. A field that reads `accepted` on every live record carries no information anyway. Git holds every prior version, so nothing is lost by rewriting a record when the decision changes.

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

`NNNN-kebab-case-title.md` where `NNNN` is a 4-digit sequence starting at `0001`. The number is permanent once assigned; never renumber and never reallocate a retired number, because citations resolve by number. When a later ADR supersedes an earlier one, reduce the earlier record to a stub — its title, one sentence of what it decided, and a pointer to its replacement — so nothing in it can be acted on. Its shape is the marker.

## Template

Use MADR 4.0:

```markdown
---
date: YYYY-MM-DD
last-revised: YYYY-MM-DD
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
