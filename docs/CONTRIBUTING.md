---
title: Contributing guidelines
description: >-
  This NuGet package is an open-source project.
  Learn how to contribute issues, fixes, new indicators, new features, or to our discussions.
---


# Contributing guidelines

**Thanks for taking the time to contribute!**

This project is simpler than most, so it's a good place to start contributing to the open source community, even if you're a newbie.

We are accepting these sorts of changes and requests:

- Bug reports and fixes
- Usability improvements
- Documentation updates
- New reputable "by the book" indicators and overlays

We are not accepting things that should be done in your own wrapper code:

- Personal customizations and preferences
- Modified or augmented outputs that are not intrinsic

If you have general interest in contributing, but are not sure where to start, please [contact us](#contact-info) and we can help to find work in an area of interest.

## Reporting bugs and feature requests

If you suspect a problem, please [report a bug Issue](https://github.com/facioquo/stock-indicators-dotnet/issues/new?labels=bug&template=bug_report.md) with a detailed description of the problem, steps to reproduce, code samples, and any reference materials.  For enhancements, [create a feature Issue](https://github.com/facioquo/stock-indicators-dotnet/issues/new?labels=enhancement&template=feature_request.md).

Use the [Discussions](https://github.com/facioquo/stock-indicators-dotnet/discussions) area for general ideation and help/usage questions.

## Project management

- Planned work is managed in [the backlog](https://github.com/orgs/facioquo/projects/27).
- Work items are primarily entered as Notes (not Issues), except where an issue or feature is user reported.  With that said, Notes can be converted to Issues if in-progress and collaborative discussion is needed.

## Developing

- Read this first: [A Step by Step Guide to Making Your First GitHub Contribution](https://codeburst.io/a-step-by-step-guide-to-making-your-first-github-contribution-5302260a2940).  I also have a discussion [on Forking](https://github.com/facioquo/stock-indicators-dotnet/discussions/503) if you have questions.
- If you want to work on something specific, please mention your intention on the related [Issue](https://github.com/facioquo/stock-indicators-dotnet/issues).  If an Issue does not exist for your contribution, please create one before starting.  This will help us reserve that feature and avoid duplicative efforts.
- If you are adding a new indicator, the easiest way to do this is to copy the folder of an existing indicator and rename everything using the same naming conventions and taxonomy.  All new indicators should include [tests](#testing).
- Do not commingle multiple contributions on different topics.  Please keep changes small and separate.

### Initial setup

The recommended setup uses VS Code with the dev container (automated) or the **Setup: Dev tools** VS Code task to check for prerequisites.  For manual setup, run these commands once after cloning:

```bash
dotnet tool restore   # install .NET CLI tools
dotnet restore        # restore NuGet packages
```

## Testing

- Review the `tests/Indicators` folder for examples of unit tests.  Just copy one of these.
- New indicators should be tested against manually calculated, proven, accurate results.  It is helpful to include your manual calculations spreadsheet in the appropriate indicator test folder when [submitting changes](#submitting-changes).
- Historical Stock Bars are automatically added to unit test methods.  A `Data.Quotes.xlsx` Excel file is included in the `tests/Indicators/TestData` folder that is an exact copy of what is used in the unit tests.  Use a copy of this file for your manual calculations to ensure that it is correct.  Do not commit changes to the original file.
- We expect all unit tests to execute successfully and all Errors and Warning resolved before you submit your code.
- Failed builds or unit testing will block acceptance of your Pull Request when submitting changes.

### Regression baseline testing

Regression baselines detect unintended behavioral changes in indicators. Each baseline is a JSON file with expected outputs for standard test data.

```bash
# run all regression baseline tests
dotnet test --filter "TestCategory=Regression"

# regenerate all baselines (locally)
dotnet run --project tools/baselining -- --all

# regenerate specific baseline
dotnet run --project tools/baselining -- --indicator SMA
```

Regenerate baselines after intentional algorithm changes, .NET upgrades, or test data changes. Use the [Regenerate Baselines workflow](https://github.com/facioquo/stock-indicators-dotnet/actions/workflows/regenerate-baselines.yml) for automated regeneration via GitHub Actions.

When reviewing PRs with baseline changes, verify the reason is documented, review numeric differences, and ensure no unexpected indicators were affected.

### Performance benchmarking

Performance (timing) benchmarks run through one script: `tools/performance/perf.sh`.
Copy/paste these exact commands — run them from the repository root. Do not add
options; the defaults keep runs comparable to the committed baselines.

Requirements: .NET SDK and [`jq`](https://jqlang.github.io/jq/) (for `evaluate`
and `spot`).

```bash
# Check one indicator against the baselines (fast — use this in your dev loop)
bash tools/performance/perf.sh spot Ema

# Run the full suite and report regressions vs baselines (~1 hour)
bash tools/performance/perf.sh evaluate

# Regenerate and replace the committed baselines (~1 hour); review git diff after
bash tools/performance/perf.sh reset
```

That is everything most contributors need. For the baseline set, single-style spot
checks, CI workflows, and raw BenchmarkDotNet usage, see the
[benchmarking guide](https://github.com/facioquo/stock-indicators-dotnet/blob/main/tools/performance/benchmarking.md).

## Documentation

This site uses [VitePress](https://vitepress.dev) with Vue components and Markdown.
Our documentation site code is in the `docs` folder.
Build the site locally to test that it works properly.

```bash
# one-time: grant your gh CLI token read:packages access
gh auth refresh --scopes read:packages

# print and copy token
gh auth token
```

```shell
# ~/Users/{username}/.npmrc
@facioquo:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken={token}
```

```bash
# run dev server for documentation site from /docs folder
# site opens at http://localhost:5173/
cd docs
pnpm run docs:dev
```

When adding or updating indicators:

- Add or update the `/docs/indicators/` documentation files.
- Page image assets go in `/docs/.vitepress/public/assets/` and can be optimized to `webp` format using [ImageMagick](https://imagemagick.org) or the [cwebp Encoder CLI](https://developers.google.com/speed/webp/docs/cwebp) and a command like `cwebp -resize 832 0 -q 100 examples.png -o examples-832.webp`

**Local cache cleanup note:** To clear documentation or workspace build caches, run the VS Code task `Clean: All` (or run platform-specific clean tasks). See `.vscode/tasks.json` for exact task names and behavior.

### Accessibility testing

- Use Lighthouse in Chrome, or
- Run the automated pa11y task: **Tasks: Run Task** → `Test: Website a11y (pa11y)`, or
- Run script manually: `bash .vitepress/test-a11y.sh` (tests `localhost` build, not production)

## Submitting changes

By submitting changes to this repo you are also acknowledging and agree to the terms in both the [Developer Certificate of Origin (DCO) 1.1](https://developercertificate.org) and the [Apache 2.0 license](https://opensource.org/licenses/Apache-2.0).  These are standard open-source terms and conditions.

When ready, submit a [Pull Request](https://help.github.com/pull-requests) with a clear description of what you've done and why it's important.

### Pull Request naming convention

Pull Request titles must follow the [Conventional Commits](https://www.conventionalcommits.org) format: `type: Subject` where:

- `type` is one of: feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert, plan (lowercase)
- `Subject` starts with an uppercase letter

Examples: `feat: Add RSI indicator`, `fix: Resolve calculation error in MACD`, `docs: Update API documentation`, `plan: Define technical implementation approach`

Always write a clear log message for your commits. One-line messages are fine for most changes.

After a Pull Request is reviewed, accepted, and _squash_ merged to `main`, we may batch changes before publishing a new package version to the [public NuGet repository](https://www.nuget.org/packages/FacioQuo.Stock.Indicators).  Please be patient with turnaround time.

## Code reviews and admin

If you want to contribute administratively, do code reviews, or provide general user support, we're also currently seeking a few core people to help.  Please [contact us](#contact-info) if interested.

## Standards and guidelines

- [Guiding principles for this project](https://github.com/facioquo/stock-indicators-dotnet/discussions/648)
- [.NET Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines)
- [NuGet Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget)
- [Semantic Version 2.0](https://semver.org)

## Versioning & deployment

We use [GitVersion](https://gitversion.net) for automated [semantic versioning](https://semver.org). Version numbers are automatically generated based on branch names, commit messages, and Git history.

| Component | Format    | Description |
| --------- | --------- | ----------- |
| Major     | `x.-.-`   | Breaking changes. Use `+semver: major` in commit message. |
| Minor     | `-.x.-`   | New features, backward compatible. Use `+semver: minor` in commit message. |
| Patch     | `-.-.x`   | Bug fixes, documentation. Use `+semver: patch` (or default auto-increment). |
| Suffix    | `-label.N` | Pre-release identifier. Automatically set based on branch and build type. |

### Branch-based versioning

GitVersion automatically determines version suffixes based on the branch:

- **main branch**: Produces `2.x.x` versions
  - CI builds: `2.7.1-ci.345` (includes build metadata)
  - Production: `2.7.1` (no suffix)
- **v3 branch**: Produces `3.x.x-preview.N` versions (always has preview suffix)
- **Feature branches**: `x.x.x-{branch-name}.N` (branch name becomes suffix)

### Controlling version increments

Add semver tags to PR merge commit messages to control version bumps:

- `+semver: major` → increments major version (breaking changes)
- `+semver: minor` → increments minor version (new features)
- `+semver: patch` → increments patch version (bug fixes, default behavior)
- `+semver: none` → no version increment

Example merge commit: `feat: Add new indicator (+semver: minor)`

### Creating preview releases from stable branches

To publish a preview/pre-release version from `main` or `v2` (normally stable branches), create a Git tag with the desired suffix before triggering deployment:

```bash
# Create preview tag
git tag 2.8.0-preview.1
git push origin 2.8.0-preview.1

# Then trigger manual workflow deployment with preview=true
```

GitVersion will honor the tag's pre-release suffix. This approach works for any pre-release label (preview, rc, beta, alpha).

### Package deployment and tagging

Packages are deployed via two separate GitHub Actions workflows:

#### CI Package deploy (automatic)

**Trigger:** Push to main or `v*` branches

- Published to GitHub Packages only
- Version format: `{Major}.{Minor}.{Patch}-ci.{run_number}`
- Examples: `2.7.2-ci.1234`, `3.0.0-ci.567`
- Idempotent: Each commit gets unique incrementing run number
- No Git tags created

**Workflow:** `.github/workflows/deploy-package-github.yml`

#### Production package deploy (manual)

**Trigger:** Publishing a GitHub Release

- Published to `nuget.org` only
- Version comes directly from release tag (strips 'v' prefix)
- Examples:
  - Tag `2.8.0` → deploys `2.8.0` (stable)
  - Tag `3.0.0-preview.2` → deploys `3.0.0-preview.2` (preview)
- Published releases: Full deployment to nuget.org
- Manual dry-run: Run the workflow from the Actions tab ("Run workflow") and supply the release tag — build only, no deploy
- Saving a draft release does **not** trigger the workflow
- Git tag already exists (from release creation)

**Workflow:** `.github/workflows/deploy-package-nuget.yml`

### Version examples

| Scenario | Trigger | Version | Registry | Notes |
| :------- | :------ | :------ | :------- | :---- |
| CI build | Push to main | `2.7.2-ci.1234` | GitHub Packages | Run 1234 |
| CI build | Push to main | `2.7.2-ci.1235` | GitHub Packages | Run 1235 (next commit) |
| CI build | Push to v3 | `3.0.0-ci.567` | GitHub Packages | Run 567 |
| Production | Release tag `2.8.0` | `2.8.0` | nuget.org | Stable version |
| Production | Release tag `3.0.0-preview.2` | `3.0.0-preview.2` | nuget.org | Preview version |
| Dry-run | Manual run, tag `2.8.1` | `2.8.1` | None (dry-run) | Build only, no deploy |

For technical details, see:

- GitVersion configuration: [`src/gitversion.yml`](https://github.com/facioquo/stock-indicators-dotnet/blob/main/src/gitversion.yml)
- CI workflow: [`deploy-package-github.yml`](https://github.com/facioquo/stock-indicators-dotnet/blob/main/.github/workflows/deploy-package-github.yml)
- Production workflow: [`deploy-package-nuget.yml`](https://github.com/facioquo/stock-indicators-dotnet/blob/main/.github/workflows/deploy-package-nuget.yml)

## License

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

This repository uses a standard Apache 2.0 open-source license.  It enables open-source community development by protecting the project and contributors from certain legal risks while allowing the widest range of uses, including in closed source software.  Please review the [license](https://opensource.org/licenses/Apache-2.0) before using or contributing to the software.

## Contact info

[Start a new discussion](https://github.com/facioquo/stock-indicators-dotnet/discussions) or [submit an issue](https://github.com/facioquo/stock-indicators-dotnet/issues) if it is publicly relevant.  You can also direct message [@daveskender](https://twitter.com/messages/compose?recipient_id=27475431).

Thanks,
Dave Skender
