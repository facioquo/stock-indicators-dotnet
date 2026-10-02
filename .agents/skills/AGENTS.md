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
npx skills list              # List installed skills
npx skills check             # Check community skills for updates
npx skills update --project --all  # Update community skills from their sources
npx skills find <query>      # Search the skills.sh registry
npx skills init .agents/skills/<name>  # Scaffold a new skill
npx skills add <owner/repo> --skill <name> --agent universal --yes  # Add --copy for a private source
npx skills remove <name>
```

`skills-lock.json` pins each community skill's source and hash; the VS Code task `Update: Skills` runs the update.

## Boundaries

✅ Always scaffold a new skill with `npx skills init` and add it to the skills table in the root AGENTS.md

⚠️ Ask before deleting a skill — first remove every reference to it

🚫 Never hand-edit a community skill (vitepress); update it from its source

🚫 Never add files under `.claude/skills/` — it is a symlink to this folder

🚫 Never use `#skill:`, `#file:`, or `#tool:` markers — name skills and tools in prose and link files with Markdown links
