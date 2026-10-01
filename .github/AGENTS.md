# GitHub configuration

This folder holds GitHub Actions workflows, issue templates, and Copilot instruction files.

```plaintext
.github/
├── workflows/       # CI, website, package deploy, and performance workflows
├── instructions/    # Copilot file-pattern routing to skills
├── ISSUE_TEMPLATE/  # Bug report and feature request forms
└── zizmor.yml       # Workflow security-scan configuration
```

## Workflows

- `ci.yml` is the primary pipeline; its `gate` job is the required status check and skips draft pull requests.
- `lint-workflows.yml` runs actionlint and zizmor (`--min-severity=medium`) on every workflow change. Run `pipx run zizmor .github/workflows` locally before pushing one.

## Boundaries

✅ Always keep an instruction file to a pointer at the skill that owns the guidance

⚠️ Ask before deleting an instruction file — check what its `applyTo` pattern routes first

🚫 Never place skills here — they live in `.agents/skills/`
