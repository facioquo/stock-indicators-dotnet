# StreamHub audit script

Validates StreamHub test coverage, interface compliance, and provider history testing completeness.

## Usage

```bash
# From repository root
bash tools/scripts/audit-streamhub.sh
```

## What it validates

- All StreamHub implementations have corresponding test files
- Tests inherit from `StreamHubTestBase` and implement correct observer/provider interfaces
- Tests include comprehensive provider history mutations (Add/Remove operations)
- Test base classes are properly structured

## Exit codes

- `0` - Success (no critical issues, warnings allowed)
- `1` - Failure (missing test files or interface compliance issues)

## CI/CD Integration

```yaml
- name: Audit StreamHub Tests
  run: bash tools/scripts/audit-streamhub.sh
```

## Complete documentation

For detailed information about audit checks, fixing patterns, and examples, see:

- **StreamHub Guidelines**: [.agents/skills/indicator-stream/SKILL.md](../../.agents/skills/indicator-stream/SKILL.md)
- **Canonical Test Pattern**: `tests/Library/Indicators/e-j/Ema/EmaHubTests.cs`
