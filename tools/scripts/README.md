# StreamHub audit script

Validates StreamHub test coverage, interface compliance, and provider history testing completeness for tasks T173-T185.

## Usage

```bash
# From repository root
bash tools/scripts/audit-streamhub.sh
```

## What it validates

- **T173**: All StreamHub implementations have corresponding test files
- **T175-T179**: Tests inherit from `StreamHubTestBase` and implement correct observer/provider interfaces
- **T180-T183**: Tests include comprehensive provider history mutations (Add/Remove operations)
- **T184-T185**: Test base classes are properly structured

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

- **Streaming Plan**: `docs/plans/streaming-indicators.plan.md`
- **StreamHub Guidelines**: `.github/instructions/indicator-stream.instructions.md`
- **Canonical Test Pattern**: `tests/Library/Indicators/e-j/Ema/EmaHubTests.cs`
