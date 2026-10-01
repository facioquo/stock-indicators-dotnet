# Scripts

Bash scripts for maintenance tasks. The clean and stop scripts run from any directory; run the audit from the repository root.

| Script | Does | VS Code task |
| ------ | ---- | ------------ |
| `audit-streamhub.sh` | Audits StreamHub test coverage (see below) | — |
| `dotnet-clean.sh` | Runs `dotnet clean`, then deletes `bin`, `obj`, `TestResults`, `BenchmarkDotNet.Artifacts`, and `packages.lock.json` files | `Clean: .NET (full)` |
| `docs-clean.sh` | Deletes the docs site's `node_modules`, build output, VitePress cache, and lock files | `Clean: Docs (vitepress)` |
| `stop-simulation.sh` | Stops processes running `tools/simulate` | `Stop: Simulation hosts` |
| `stop-sseserver.sh` | Stops processes running `tools/sse-server`, including whatever holds port 5001 | `Stop: SseServer hosts` |

## StreamHub audit

```bash
bash tools/scripts/audit-streamhub.sh
```

CI runs it in the `Audit StreamHub tests` step of `ci.yml`. It checks that:

- every StreamHub source file (`{Name}Hub.cs`) has a matching `{Name}HubTests.cs` file
- each test class inherits `StreamHubTestBase` and implements at least one observer test interface
- the test methods those interfaces require exist, and they exercise late `Add` and `RemoveAt` on the provider history

It exits `1` when a test file is missing or a test class fails the base-class or interface check, and `0` otherwise; missing test methods and history coverage gaps print as warnings.

For the test pattern it enforces, see the [indicator-stream skill](../../.agents/skills/indicator-stream/SKILL.md) and `tests/Library/Indicators/e-j/Ema/EmaHubTests.cs`.
