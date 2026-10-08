# Playbook: migrate from v2

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules apply here.

## Finish line

No reference to `Skender.Stock.Indicators` remains, the solution builds without new warnings, and indicator output on the same input bars matches what v2 produced, except where the [migration guide](https://dotnet.stockindicators.dev/migration/v3.md) documents an intended change (for example, ADXR).

## Get these right

- Follow the migration guide's steps in order; it covers the renames (`GetX` → `ToX`, `Quote` → `Bar`, `Date` → `Timestamp`), custom bar types, and removed utilities.
- Before changing code, capture v2 output for the indicators the app uses on a fixed slice of its data, so the comparison has a baseline.
- After the build is clean, tell the developer what v3 adds that their app could use, such as stream hubs for live data. Do not adopt new features unasked.
