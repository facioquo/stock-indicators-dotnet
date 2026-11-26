# Agent skills

This folder holds the repository's agent skills: one folder per skill, each with a `SKILL.md` entry point. The root AGENTS.md lists them and says when to load each.

Read this file before creating or editing a skill. To use a skill, read its `SKILL.md` and stop here.

## Primary directive

Ensure developers and AI agents can correctly execute complex, domain-specific tasks on their first attempt by loading the appropriate skill at the right time — reducing human clarification rounds and rework.

## Secondary directives

1. Keep every skill accurate against the current code, so an agent following it produces work that builds and passes review (not as important as primary)
2. Keep each skill self-contained and under 16K characters, so it loads cheaply and never depends on another file to make sense (not as important as #1)

## Skill layout

[agentskills.io](https://agentskills.io/specification) is the canonical specification.

```plaintext
.agents/skills/
├── <skill-name>/
│   ├── SKILL.md        # Entry point; frontmatter `name` matches the folder
│   ├── assets/         # Copyable templates (*.template.*)
│   ├── references/     # Detail loaded on demand, linked from SKILL.md
│   └── scripts/        # Executable helpers
└── AGENTS.md           # This file
```

Every skill except vitepress is written for this repository only: repository paths, types, and commands belong in it. [.agents/README.md](../README.md) keeps them out of library consumers' installs.

## Authoring rules

- Write for the executing agent: imperative, present tense, sentence-case headings, one source line per paragraph or bullet.
- Write the `description` as a trigger — what the skill covers and when to load it.
- Link every file in `references/`, `assets/`, and `scripts/` from `SKILL.md` at the point where the agent decides to load it.
- State each rule in one skill and name that skill in prose elsewhere ("the testing-standards skill"); never link into another skill's folder or up to an AGENTS.md.
- Verify every path, type, and command against the code before writing it.

## Commands

```bash
pnpm dlx skills list                        # List installed skills
pnpm dlx skills check                       # Check community skills for updates
pnpm dlx skills update --project --all      # Update community skills from their sources
pnpm dlx skills find <query>                # Search the skills.sh registry
pnpm dlx skills init .agents/skills/<name>  # Scaffold a new skill
pnpm dlx skills add <owner/repo> --skill <name> --agent universal --yes  # Add --copy for a private source
pnpm dlx skills remove <name>
```

`skills-lock.json` pins each community skill's source and hash; the VS Code task `Update: Skills` runs the update.

## Boundaries

✅ Always scaffold a new skill with `pnpm dlx skills init` and add it to the skills table in the root AGENTS.md

⚠️ Ask before deleting a skill — first remove every reference to it

🚫 Never edit a skill listed in `skills-lock.json` directly, including its version notes and references; update it only with `pnpm dlx skills update --project --all`

🚫 Never add files under `.claude/skills/` — it is a symlink to this folder

🚫 Never use `#skill:`, `#file:`, or `#tool:` markers — name skills and tools in prose and link files with Markdown links
