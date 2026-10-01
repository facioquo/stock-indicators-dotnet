# Performance baselines

Committed BenchmarkDotNet results that `detect-regressions.sh` compares new runs against. To run benchmarks or refresh these files, see the [benchmarking guide](../benchmarking.md); it also lists which suites form the [baseline set](../benchmarking.md#the-baseline-set).

## Files

Each suite in the baseline set has two files, which `perf.sh reset` copies from `BenchmarkDotNet.Artifacts/results/`:

- `Performance.<Suite>-report-full.json` — machine-readable; the input to regression detection
- `Performance.<Suite>-report-github.md` — human-readable tables for review

Both are committed. This folder's `.gitignore` excludes only `*.zip` exports.

`perf.sh reset` lists any `Performance.*` file here whose suite is no longer in the baseline set; `perf.sh reset --prune` deletes them.

Refresh baselines only for intentional, verified performance work, in the same change. Use git history for older baselines.
